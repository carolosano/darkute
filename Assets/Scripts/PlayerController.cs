using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private bool allowDiagonals = true;

    [Header("Colisiones")]
    [SerializeField] private LayerMask solidLayer;
    [SerializeField] private float probeRadius = 0.15f;

    [Header("Dash (Esquive hacia atrás)")]
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    [SerializeField] private float dashSpeed = 9f;
    [SerializeField] private float dashDuration = 0.14f;
    [SerializeField] private float dashCooldown = 0.35f;
    [Tooltip("Nombre del layer alternativo que ignora colisiones con enemigos durante el dash")]
    [SerializeField] private string dashLayerName = "PlayerDash";
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;

    private Vector2 input;
    private Vector2 moveDir;
    private Vector2 lastLookDir = Vector2.down;

    private bool isDashing;
    private float nextDashAllowed;
    private int originalLayer;
    private int dashLayer = -1;
    private bool usingIgnoreCollisionFallback;
    private bool enemiesIgnored;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
        rb.useFullKinematicContacts = true;

        originalLayer = gameObject.layer;
        dashLayer = LayerMask.NameToLayer(dashLayerName);
        usingIgnoreCollisionFallback = (dashLayer == -1);
    }

    private void Update()
    {
        // Si está dashing, no aceptar input de movimiento normal
        if (isDashing) return;

        // Input crudo
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y);

        // Eje dominante
        if (!allowDiagonals && input.sqrMagnitude > 0f)
        {
            if (Mathf.Abs(x) >= Mathf.Abs(y)) input.y = 0f;
            else input.x = 0f;
        }

        moveDir = input.normalized;

        // Animaciones
        bool isMoving = moveDir.sqrMagnitude > 0.0001f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.y);
            lastLookDir = moveDir;
        }
        else
        {
            animator.SetFloat("moveX", lastLookDir.x);
            animator.SetFloat("moveY", lastLookDir.y);
        }

        // Input de dash
        if (Input.GetKeyDown(dashKey))
            TryDash();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        if (moveDir.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 nextPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
        bool blocked = Physics2D.OverlapCircle(nextPos, probeRadius, solidLayer) != null;

        if (!blocked)
        {
            rb.MovePosition(nextPos);
        }
        else
        {
            if (Mathf.Abs(moveDir.x) > 0.0001f)
            {
                Vector2 tryX = rb.position + new Vector2(moveDir.x, 0f) * moveSpeed * Time.fixedDeltaTime;
                if (Physics2D.OverlapCircle(tryX, probeRadius, solidLayer) == null)
                {
                    rb.MovePosition(tryX);
                    return;
                }
            }
            if (Mathf.Abs(moveDir.y) > 0.0001f)
            {
                Vector2 tryY = rb.position + new Vector2(0f, moveDir.y) * moveSpeed * Time.fixedDeltaTime;
                if (Physics2D.OverlapCircle(tryY, probeRadius, solidLayer) == null)
                {
                    rb.MovePosition(tryY);
                    return;
                }
            }
        }
    }

    // ================= DASH ==================
    private void TryDash()
    {
        if (isDashing || Time.time < nextDashAllowed) return;

        // Dirección opuesta a donde mira
        Vector2 dir = -lastLookDir;

        // Si está mirando solo verticalmente, dash por defecto hacia izquierda
        if (Mathf.Abs(dir.x) < 0.1f && Mathf.Abs(dir.y) > 0.1f)
            dir = Vector2.left;

        StartCoroutine(DashCoroutine(dir));
    }

    private IEnumerator DashCoroutine(Vector2 dir)
    {
        isDashing = true;
        nextDashAllowed = Time.time + dashCooldown;

        // Reproducir animación correcta
        if (dir.x < 0)
            animator.Play("DashLeft");
        else
            animator.Play("DashRight");

        BeginPassThroughEnemies();

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dir.normalized * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        EndPassThroughEnemies();

        // 🔄 Al terminar el dash, volver a Idle/Walk automáticamente
        animator.SetBool("isMoving", false);
        animator.SetFloat("moveX", lastLookDir.x);
        animator.SetFloat("moveY", lastLookDir.y);

        isDashing = false;
    }

    // ============ IGNORAR ENEMIGOS =============
    private void BeginPassThroughEnemies()
    {
        if (!usingIgnoreCollisionFallback && dashLayer != -1)
        {
            gameObject.layer = dashLayer;
        }
        else
        {
            int playerLayer = originalLayer;
            int enemyLayerIndex = FirstLayerFromMask(enemyLayer);
            if (enemyLayerIndex != -1)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, enemyLayerIndex, true);
                enemiesIgnored = true;
            }
        }
    }

    private void EndPassThroughEnemies()
    {
        if (!usingIgnoreCollisionFallback && dashLayer != -1)
        {
            gameObject.layer = originalLayer;
        }
        else if (enemiesIgnored)
        {
            int playerLayer = originalLayer;
            int enemyLayerIndex = FirstLayerFromMask(enemyLayer);
            if (enemyLayerIndex != -1)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, enemyLayerIndex, false);
            }
            enemiesIgnored = false;
        }
    }

    private int FirstLayerFromMask(LayerMask mask)
    {
        int m = mask.value;
        if (m == 0) return -1;
        for (int i = 0; i < 32; i++)
        {
            if ((m & (1 << i)) != 0) return i;
        }
        return -1;
    }

    // ============================================
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((Vector2)transform.position + moveDir * moveSpeed * Time.fixedDeltaTime, probeRadius);
    }

    public Vector2 FacingDirection
    {
        get { return new Vector2(animator.GetFloat("moveX"), animator.GetFloat("moveY")).normalized; }
    }
}




using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [Tooltip("Permite diagonales; si lo desactivás, prioriza el eje dominante.")]
    [SerializeField] private bool allowDiagonals = true;

    [Header("Colisiones")]
    [Tooltip("Capas que bloquean el movimiento (paredes/obstáculos).")]
    [SerializeField] private LayerMask solidLayer;
    [Tooltip("Radio del chequeo de colisión en el punto objetivo.")]
    [SerializeField] private float probeRadius = 0.15f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 input;
    private Vector2 moveDir;
    private Vector2 lastLookDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // ⚙️ Config para Rigidbody2D Dynamic (top-down sin gravedad)
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        // 🧯 Frenar empujes y “rebotes” no deseados
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;

        // (Opcional, no molesta si queda) útil cuando el otro es Kinematic
        rb.useFullKinematicContacts = true;
    }

    private void Update()
    {
        // 1) Input crudo
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        input = new Vector2(x, y);

        // 2) Si no querés diagonales, priorizá eje dominante
        if (!allowDiagonals && input.sqrMagnitude > 0f)
        {
            if (Mathf.Abs(x) >= Mathf.Abs(y)) input.y = 0f;
            else                              input.x = 0f;
        }

        // 3) Dirección normalizada (diagonal no más rápida)
        moveDir = input.normalized;

        // 4) Animación
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
            // Idle mirando a la última dirección
            animator.SetFloat("moveX", lastLookDir.x);
            animator.SetFloat("moveY", lastLookDir.y);
        }
    }

    private void FixedUpdate()
    {
        // Si no hay input, cancelá cualquier velocidad residual de colisiones
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Cálculo del siguiente punto deseado
        Vector2 nextPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        // Chequeo simple de colisión en el punto objetivo
        bool blocked = Physics2D.OverlapCircle(nextPos, probeRadius, solidLayer) != null;
        if (!blocked)
        {
            rb.MovePosition(nextPos);
        }
        else
        {
            // Intento por ejes para deslizar junto a paredes
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

    // Debug visual del probe
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((Vector2)transform.position + moveDir * moveSpeed * Time.fixedDeltaTime, probeRadius);
    }

    public Vector2 FacingDirection
    {
        get
        {
            return new Vector2(animator.GetFloat("moveX"), animator.GetFloat("moveY")).normalized;
        }
    }
}


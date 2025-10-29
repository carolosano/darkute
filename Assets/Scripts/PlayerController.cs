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

        
        rb.useFullKinematicContacts = true;  
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        input = new Vector2(x, y);

        
        if (!allowDiagonals && input.sqrMagnitude > 0f)
        {
            if (Mathf.Abs(x) >= Mathf.Abs(y)) input.y = 0f;
            else                              input.x = 0f;
        }

        
        moveDir = input.normalized;

        
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
    }

    private void FixedUpdate()
    {
        if (moveDir.sqrMagnitude < 0.0001f) return;

        
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//No tenés el script Enemy agregado al GameObject del enemigo.
// Asegurate de arrastrar el Enemy.cs al prefab/objeto del enemigo en el inspector.

//El collider que detecta OverlapCircleAll está en un child del enemigo (p. ej. un objeto hijo con collider) y el script Enemy está en el padre.
// Solución segura: usar GetComponentInParent<Enemy>() (en PlayerAttack) en vez de GetComponent<Enemy>(), para cubrir ambos casos.

//No coincidencia de layer / LayerMask. Si PlayerAttack.enemyLayer no incluye la layer del enemigo, OverlapCircleAll no devolverá al enemigo.
// Revisá las layers y el enemyLayer en el inspector.

//Collider desactivado o no está en 2D (usás Physics2D).
// Asegurate que sea Collider2D y esté activo.
public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    private bool isMoving;

    private Vector2 input;
    private Animator animator;

    public LayerMask solidObjectsLayer;

    private PlayerAttack playerAttack; // referencia al script de ataque

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>(); 
    }

    public void Update()
{
    HandleMovement();
    HandleAttack();
}

private void HandleMovement()
{
    if (isMoving) return;

    input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

    // Prioridad horizontal sobre vertical
    if (input.x != 0) input.y = 0;

    if (input == Vector2.zero)
    {
        animator.SetBool("isMoving", false);
        return;
    }

    animator.SetFloat("moveX", input.x);
    animator.SetFloat("moveY", input.y);

    Vector3 targetPos = transform.position + (Vector3)input;

    if (IsWalkable(targetPos))
        StartCoroutine(Move(targetPos));

    animator.SetBool("isMoving", isMoving);
}

private void HandleAttack()
{
    if (Input.GetKeyDown(KeyCode.K))
    {
        playerAttack.DoAttack();
    }
}


    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }
}


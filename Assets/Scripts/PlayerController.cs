using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    private bool isMoving;

    private Vector2 input;
    private Animator animator;

    public LayerMask solidObjectsLayer;

    

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
    }

    public void Update()
{
    HandleMovement();
    /*HandleAttack();*/
}

private void HandleMovement()
{
    if (isMoving) return;

    input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

    
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


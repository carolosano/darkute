using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;

    public LayerMask enemyLayer; 
    public float attackRange = 0.5f;
    public int attackDamage = 1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void DoAttack()
    {
        
        animator.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            Enemy enemyScript = hit.GetComponentInParent<Enemy>();
            if (enemyScript != null)
            {
                
                enemyScript.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("Collider detectado sin script Enemy en parent: " + hit.name);
            }
        }

    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


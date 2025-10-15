using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vida = 50f;

    [Header("Ataque")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float danio = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private string triggerAtaque = "Ataque";

    private float lastAttackTime;
    private Animator animator;
    private Transform player;

    private void Start()
    {
        animator = GetComponent<Animator>();

        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        
        if (distance < attackDistance && Time.time > lastAttackTime + attackCooldown)
        {
            Atacar();
            lastAttackTime = Time.time;
        }
    }

    private void Atacar()
    {
        
        if (animator != null)
        {
            animator.SetTrigger(triggerAtaque);
        }

        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TomarDanio(danio);
        }

        Debug.Log("Enemigo atacó al jugador");
    }

    
    public void TomarDanio(float danio)
    {
        vida -= danio;
        if (vida <= 0)
        {
            Muerte();
        }
    }

    private void Muerte()
    {
        if (animator != null)
            animator.SetTrigger("Muerte");

        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        
        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    

}


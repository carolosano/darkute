using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Combat : MonoBehaviour
{
    [Header("Hitbox (AttackController)")]
    [SerializeField] private Transform controladorGolpe;     
    [SerializeField] private float distanciaGolpe = 0.6f;    
    [SerializeField] private float radioGolpe = 0.5f;        
    [SerializeField] private LayerMask enemyLayer = ~0;     

    [Header("Daño / Anim")]
    [SerializeField] private float danioGolpe = 20f;
    [SerializeField] private string triggerNombre = "Ataque";

    private Animator animator;
    private PlayerController playerController;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        if (controladorGolpe == null)
        {
            var t = transform.Find("AttackController");
            if (t != null) controladorGolpe = t;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            HacerAtaque();
    }

    private void HacerAtaque()
    {
        if (animator == null) return;

        Vector2 dir = playerController != null ? playerController.FacingDirection : Vector2.down;

        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);

        if (controladorGolpe != null)
            controladorGolpe.localPosition = (Vector3)(dir.normalized * distanciaGolpe);

        animator.SetTrigger(triggerNombre);

        Golpe();
    }

    private void Golpe()
    {
        if (controladorGolpe == null) return;

        Collider2D[] hits = (enemyLayer.value == ~0)
            ? Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe)
            : Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe, enemyLayer);

        foreach (var c in hits)
        {
            if (c.attachedRigidbody && c.attachedRigidbody.gameObject == gameObject) continue;
            if (!c.CompareTag("Enemigo")) continue;

            var e = c.GetComponent<Enemy>();
            if (e != null) e.TomarDanio(danioGolpe, transform.position); 

        }
    }

    public void GolpePorAnimEvent() => Golpe();
    private void OnDrawGizmosSelected()
    {
        if (controladorGolpe == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
    }
}

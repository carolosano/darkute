using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Combat : MonoBehaviour
{
    [Header("Hitbox (AttackController)")]
    [SerializeField] private Transform controladorGolpe;     // arrastrá aquí el hijo "AttackController"
    [SerializeField] private float distanciaGolpe = 0.6f;    // offset desde el centro del player
    [SerializeField] private float radioGolpe = 0.5f;        // radio del círculo de impacto
    [SerializeField] private LayerMask enemyLayer = ~0;      // opcional: filtrar solo capa de enemigos

    [Header("Daño / Anim")]
    [SerializeField] private float danioGolpe = 20f;
    [SerializeField] private string triggerNombre = "Ataque";

    private Animator animator;
    private PlayerController playerController;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        // Si no fue asignado en el inspector, buscar un hijo llamado exactamente "AttackController"
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

        // Tomar la dirección actual de mirada del player (ya dijiste que esta propiedad la tenés resuelta)
        Vector2 dir = playerController != null ? playerController.FacingDirection : Vector2.down;

        // Setear params para el BlendTree 2D de ataque
        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);

        // Reposicionar el AttackController hacia donde mira
        if (controladorGolpe != null)
            controladorGolpe.localPosition = (Vector3)(dir.normalized * distanciaGolpe);

        // Disparar anim
        animator.SetTrigger(triggerNombre);

        // Aplicar daño (si preferís por Animation Event, comentá esta línea y usa GolpePorAnimEvent)
        Golpe();
    }

    private void Golpe()
    {
        if (controladorGolpe == null) return;

        // Si querés filtrar por capa, usá el overload con layerMask; si no, OverlapCircleAll simple
        Collider2D[] hits = (enemyLayer.value == ~0)
            ? Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe)
            : Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe, enemyLayer);

        foreach (var c in hits)
        {
            if (c.attachedRigidbody && c.attachedRigidbody.gameObject == gameObject) continue;
            if (!c.CompareTag("Enemigo")) continue;

            var e = c.GetComponent<Enemy>();
            if (e != null) e.TomarDanio(danioGolpe);
        }
    }

    // Útil si sincronizás el golpe con un Animation Event en el frame de impacto
    public void GolpePorAnimEvent() => Golpe();

    private void OnDrawGizmosSelected()
    {
        if (controladorGolpe == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
    }
}

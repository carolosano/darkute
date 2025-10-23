using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking, Dead }
    private State state = State.Patrolling;

    [Header("Vida")]
    [SerializeField] private float vidaMax = 50f;
    private float vida;

    [Header("Detección / Movimiento")]
    [SerializeField] private float detectionRange = 5f;      // Radio para empezar a perseguir (centro a centro)
    [SerializeField] private float chaseSpeed = 3.5f;        // Velocidad en persecución
    [SerializeField] private float loseSightMultiplier = 1.5f;
    [SerializeField] private float stopToAttackBuffer = 0.3f;

    [Header("Ataque (lógica FSM)")]
    [SerializeField] private float attackDistance = 2f;      // Umbral para pasar a estado Attacking (centro a centro)

    [Header("Golpe (Hitbox real)")]
    [SerializeField] private Transform attackPoint;          // Un hijo al frente del enemigo (la “mano”)
    [SerializeField] private float attackRadius = 0.9f;      // Radio del golpe
    [SerializeField] private LayerMask playerMask;           // Capa del Player (poné la capa del jugador)
    [SerializeField] private float danio = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private string triggerAtaque = "Ataque";

    [Header("Muerte / Pool")]
    [SerializeField] private float deathDeactivateDelay = 1.5f;

    [Header("Animator (nombres de estados)")]
    [Tooltip("Nombre EXACTO del estado Idle (puede ser un BlendTree).")]
    [SerializeField] private string idleStateName = "IdleBT";
    [Tooltip("Nombre EXACTO del estado Walk (BlendTree).")]
    [SerializeField] private string walkStateName = "WalkBT";
    [SerializeField] private float crossFadeDuration = 0.05f;
    [Header("Ataque / Ajustes finos")]
    [SerializeField] private string attackStateName = "Ataque"; // XXX nombre del estado de la anim de ataque
    [SerializeField] private float attackLockTime = 0.35f;      // XXX tiempo mínimo que dejamos “quieto” al enemigo tras atacar
    [SerializeField] private float stickRangeTime = 0.15f;      // XXX ventana de pegajosidad del rango (en segundos)

    private float lastInRangeAt = -999f;      // XXX último momento en que detectamos al player en el hitbox
    private float attackLockUntil = 0f;       // XXX hasta cuándo “bloqueamos” moverse/cambiar


    private bool isDead;
    private Animator animator;
    private Transform player;
    private Collider2D col;
    private Patrullar patrullar;

    public SimpleEnemyPool poolOwner { get; set; }

    private float _nextAttackAllowed;

    // -------------------- LIFECYCLE --------------------

    private void Awake()
    {
        animator  = GetComponentInChildren<Animator>();
        col       = GetComponent<Collider2D>();
        patrullar = GetComponent<Patrullar>();
    }

    private void OnEnable()
    {
        isDead = false;
        vida   = vidaMax;
        state  = State.Patrolling;

        if (col != null) col.enabled = true;
        if (patrullar != null) patrullar.enabled = true;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            SetAnim(0f, false);
            SafeGoIdle();
        }

        _nextAttackAllowed = 0f; // listo para atacar de inmediato si entra en rango
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Ejecutar estado actual
        switch (state)
        {
            case State.Patrolling: TickPatrolling(); break;
            case State.Chasing:    TickChasing();    break;
            case State.Attacking:  TickAttacking();  break;
        }

        // Transiciones por distancia (macro)
        switch (state)
        {
            case State.Patrolling:
                if (dist <= detectionRange)
                {
                    state = State.Chasing;
                    if (patrullar != null) patrullar.enabled = false; // cortar patrulla
                }
                break;

            case State.Chasing:
                // Si ya estamos muy cerca, pasamos a Attacking (macro),
                // pero el golpe real lo decide OverlapCircle en TickAttacking()
                if (dist <= attackDistance)
                {
                    state = State.Attacking;
                    // XXX SetAnim(GetMoveXFacing(), false);
                    // XXX SafeGoIdle();
                }
                else if (dist > detectionRange * loseSightMultiplier)
                {
                    state = State.Patrolling;
                    if (patrullar != null) patrullar.enabled = true;  // volver a patrullar
                    SetAnim(0f, false);
                    SafeGoIdle();
                }
                break;

            case State.Attacking:
                // Si se aleja bastante, volvemos a persecución
                if (dist > attackDistance + stopToAttackBuffer)
                {
                    state = State.Chasing;
                }
                break;
        }
    }

    // -------------------- ESTADOS --------------------

    private void TickPatrolling()
    {
        // En patrulla, la animación la maneja Patrullar.cs
        if (patrullar == null || !patrullar.enabled)
        {
            SetAnim(0f, false);
            SafeGoIdle();
        }
    }

    private void TickChasing()
    {
        if (Time.time < attackLockUntil)
        {
            // Estamos en el “lock” post-ataque: quedarse quieto mirando al jugador
            float dx = player.position.x - transform.position.x;
            float attackMoveX = Mathf.Abs(dx) < 0.001f ? 0f : Mathf.Sign(dx);
            SetAnim(attackMoveX, false);

            return;
        }
        Vector2 pos    = transform.position;
        Vector2 target = (Vector2)player.position;
        Vector2 dir    = target - pos;

        bool moving = dir.sqrMagnitude > 0.0001f;
        float moveX = 0f;
        
        if (moving) moveX = Mathf.Sign(dir.x); // -1 izquierda, +1 derecha

        SetAnim(moveX, moving);
        if (moving) SafeGoWalk();

        if (moving)
        {
            Vector2 step = dir.normalized * chaseSpeed * Time.deltaTime;
            transform.position = pos + step;
        }
    }

        private void TickAttacking()
    {
        // Mirar al jugador (solo ajustamos moveX; NO forzamos crossfades aquí)
        float dx = player.position.x - transform.position.x;
        float moveX = Mathf.Abs(dx) < 0.001f ? 0f : Mathf.Sign(dx);
        SetAnim(moveX, false); // quieto para atacar

        bool enRango = InAttackRange();
        if (enRango) lastInRangeAt = Time.time;

        bool rangoPegajoso = (Time.time - lastInRangeAt) <= stickRangeTime;
        bool cooldownListo = Time.time >= _nextAttackAllowed;

        // Mientras está "lockeado" por el ataque, no dejes que otras lógicas interrumpan
        bool enLockDeAtaque = Time.time < attackLockUntil;

        // Disparar ataque si: cooldown listo Y (en rango ahora o lo estuvo hace poquito)
        if (!enLockDeAtaque && cooldownListo && (enRango || rangoPegajoso))
        {
            // Dispara trigger (Animator: Any State -> Attack con Trigger)
            if (animator != null) animator.SetTrigger(triggerAtaque);

            // Aplica daño inmediato (si preferís por Animation Event, mové esto al evento)
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TomarDanio(danio);

            // Logs de debug útiles
            Debug.Log($"[ENEMY] Ataque disparado. Daño={danio}, enRango={enRango}, pegajoso={rangoPegajoso}");

            // Cooldown y lock para no ser interrumpidos
            _nextAttackAllowed = Time.time + attackCooldown;
            attackLockUntil = Time.time + attackLockTime;
        }

        // TIP: si querés detectar si realmente está en la anim de ataque:
        // var st = animator.GetCurrentAnimatorStateInfo(0);
        // bool enAnimAtaque = st.IsName(attackStateName);
    }

    // -------------------- HITBOX / RANGO --------------------

    private bool InAttackRange()
    {
        if (attackPoint == null) return false;
        // Chequeo directo por física 2D: si toca al jugador, está en rango, aunque esté “quieto”
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerMask);
        return (hit != null);
    }

    // -------------------- ANIM HELPERS --------------------

    private void SetAnim(float moveX, bool isMoving)
    {
        if (animator == null) return;
        animator.SetFloat("moveX", Mathf.Clamp(moveX, -1f, 1f));
        animator.SetBool ("isMoving", isMoving);
    }

    private float GetMoveXFacing()
    {
        return animator != null ? Mathf.Sign(animator.GetFloat("moveX")) : 1f;
    }

    private void SafeGoWalk()
    {
        if (animator == null) return;
        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (!st.IsName(walkStateName))
            animator.CrossFade(walkStateName, crossFadeDuration, 0);
    }

    private void SafeGoIdle()
    {
        if (animator == null) return;
        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (!st.IsName(idleStateName))
            animator.CrossFade(idleStateName, crossFadeDuration, 0);
    }

    // -------------------- VIDA / MUERTE / POOL --------------------

    public void TomarDanio(float danio)
    {
        if (isDead) return;
        vida -= danio;
        if (vida <= 0f) Muerte();
    }

    private void Muerte()
    {
        if (isDead) return;
        isDead = true;
        state  = State.Dead;

        if (animator != null) animator.SetTrigger("Muerte");
        if (col != null) col.enabled = false;
        if (patrullar != null) patrullar.enabled = false;

        StartCoroutine(DevolverAlPoolDespues(deathDeactivateDelay));
    }

    private System.Collections.IEnumerator DevolverAlPoolDespues(float t)
    {
        yield return new WaitForSeconds(t);
        if (poolOwner != null)
            poolOwner.ReturnToPool(this.gameObject);
        else
            gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        // Detección macro
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque macro (transición de estado)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Hitbox real del golpe
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}

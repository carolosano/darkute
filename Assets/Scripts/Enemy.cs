using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking, Dead }
    private State state = State.Patrolling;
    private System.Collections.IEnumerator _hitCR;   // referencia a la corrutina de hit actual

    [Header("Vida")]
    [SerializeField] private float vidaMax = 50f;
    private float vida;

    [Header("Detección / Movimiento")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float loseSightMultiplier = 1.5f;
    [SerializeField] private float stopToAttackBuffer = 0.3f;

    [Header("Ataque (lógica FSM)")]
    [SerializeField] private float attackDistance = 2f;

    [Header("Golpe (Hitbox real)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.9f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float danio = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private string triggerAtaque = "Ataque";

    [Header("Muerte / Pool")]
    [SerializeField] private float deathDeactivateDelay = 1.5f;

    [Header("Animator (nombres de estados)")]
    [SerializeField] private string idleStateName = "IdleBT";
    [SerializeField] private string walkStateName = "WalkBT";
    [SerializeField] private float crossFadeDuration = 0.05f;

    [Header("Ataque / Ajustes finos")]
    [SerializeField] private string attackStateName = "Ataque";
    [SerializeField] private float attackLockTime = 0.35f;
    [SerializeField] private float stickRangeTime = 0.15f;

    // === NUEVO: Reacción al golpe ===
    [Header("Reacción al golpe (Enemy)")]
    [SerializeField] private float hitStunTime = 0.12f;
    [SerializeField] private float knockbackDistance = 0.5f;
    [SerializeField] private float knockbackDuration = 0.08f;
    [SerializeField] private LayerMask solidLayer;
    [SerializeField] private float skin = 0.02f;

    private bool hitStunned;

    private float lastInRangeAt = -999f;
    private float attackLockUntil = 0f;
    private bool isDead;
    private Animator animator;
    private Transform player;
    private Collider2D col;
    private Patrullar patrullar;
    public SimpleEnemyPool poolOwner { get; set; }
    private float _nextAttackAllowed;

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

        _nextAttackAllowed = 0f;
        hitStunned = false;
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

        if (hitStunned)
        {
            float faceX = Mathf.Sign(player.position.x - transform.position.x);
            if (float.IsNaN(faceX)) faceX = 1f;
            animator.SetFloat("moveX", faceX);
            animator.SetBool("isMoving", false);
            return;
        }


        float dist = Vector2.Distance(transform.position, player.position);

        // 1) Ejecutar lógica del estado actual
        switch (state)
        {
            case State.Patrolling: TickPatrolling(); break;
            case State.Chasing:    TickChasing();    break;
            case State.Attacking:  TickAttacking();  break;
        }

        // 2) Transiciones entre estados (segundo switch)
        switch (state)
        {
            case State.Patrolling:
                if (dist <= detectionRange)
                {
                    state = State.Chasing;
                    if (patrullar != null) patrullar.enabled = false; // cortar patrulla al empezar persecución
                }
                break;

            case State.Chasing:
                if (dist <= attackDistance)
                {
                    state = State.Attacking;

                    // ✅ reset: permití atacar apenas entro a Attacking
                    _nextAttackAllowed = 0f;
                    attackLockUntil    = 0f;
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
                // ✅ si se salió del rango de ataque, volver a perseguir y limpiar locks
                if (dist > attackDistance + stopToAttackBuffer)
                {
                    state = State.Chasing;
                    attackLockUntil    = 0f;           // limpiar lock
                    _nextAttackAllowed = Time.time;    // listo para reintentar cuando toque
                    if (patrullar != null) patrullar.enabled = false; // seguimos sin patrulla en persecución
                }
                break;
        }
    }


    private void TickPatrolling()
    {
        // En patrulla, que se encargue el script Patrullar.
        // Si por algún motivo Patrullar está apagado, quedate idle mirando al jugador.
        if (patrullar == null || !patrullar.enabled)
        {
            float faceX = LookAtPlayerX();
            SetAnim(faceX, false);
            SafeGoIdle();
        }
    }

        private void TickChasing()
    {
        // Bloqueo breve tras atacar: mirá al jugador pero no te muevas
        if (Time.time < attackLockUntil)
        {
            float faceX = LookAtPlayerX();
            SetAnim(faceX, false);
            return;
        }

        // 🚫 Asegurá que la patrulla esté cortada mientras perseguís
        if (patrullar != null && patrullar.enabled)
            patrullar.enabled = false;

        // ✅ Movimiento "a prueba de balas": SIEMPRE hacia el jugador
        Vector2 from = transform.position;
        Vector2 to   = player.position;

        if ((to - from).sqrMagnitude > 0.000001f)
        {
            Vector2 dir = (to - from).normalized;

            // Para el Animator: sólo eje X para tu BlendTree WalkBT (left/right)
            float faceX = Mathf.Abs(dir.x) < 0.0001f ? 0f : Mathf.Sign(dir.x);
            SetAnim(faceX, true);
            SafeGoWalk();

            float step = chaseSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(from, to, step);
        }
        else
        {
            float faceX = LookAtPlayerX();
            SetAnim(faceX, false);
            SafeGoIdle();
        }
    }

    private float _attackLockMax = 0.6f; // failsafe

    private void TickAttacking()
    {
        float dx = player.position.x - transform.position.x;
        float moveX = Mathf.Abs(dx) < 0.001f ? 0f : Mathf.Sign(dx);
        SetAnim(moveX, false);
       

        bool enRango = InAttackRange();
        if (enRango) lastInRangeAt = Time.time;

        bool rangoPegajoso   = (Time.time - lastInRangeAt) <= stickRangeTime;
        bool cooldownListo   = Time.time >= _nextAttackAllowed;
        bool enLockDeAtaque  = Time.time < attackLockUntil;

        // 🩹 Failsafe: si estamos “lockeados” por demasiado tiempo, liberá
        if (enLockDeAtaque && (attackLockUntil - Time.time) > _attackLockMax)
        {
            Debug.LogWarning("[ENEMY DEBUG] Lock de ataque anormalmente largo; reseteando.");
            attackLockUntil = 0f;
            enLockDeAtaque  = false;
        }

        // Log de estado
        Debug.Log($"[ENEMY DEBUG] Estado: {state} | enRango={enRango} | cooldownListo={cooldownListo} | enLock={enLockDeAtaque}");

        if (!enLockDeAtaque && cooldownListo && (enRango || rangoPegajoso))
        {
            if (animator != null)
{
            // Si tenés un estado de animación llamado igual a attackStateName, lo reproducimos
            animator.CrossFade(attackStateName, 0.05f);
            Debug.Log($"[ENEMY DEBUG] Animación '{attackStateName}' activada");
}


            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TomarDanio(danio);
                Debug.Log($"[ENEMY DEBUG] Golpe aplicado. Daño={danio}");
            }

            var react = player.GetComponent<PlayerHitReaction>();
            if (react != null) react.OnHit(transform.position);

            // ✅ Seteá nuevo cooldown y lock (cortos)
            _nextAttackAllowed = Time.time + attackCooldown; // ej. 1.5s
            attackLockUntil    = Time.time + attackLockTime; // ej. 0.35s
        }

        // Si se alejó del rango, volver a perseguir
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackDistance + stopToAttackBuffer)
        {
            state = State.Chasing;
            if (patrullar != null) patrullar.enabled = false;
        }
    }                               


    // Helper claro para “hacia dónde mirar” en X
    private float LookAtPlayerX()
    {
        if (player == null) return 1f;
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.0001f) return 0f;
        return Mathf.Sign(dx); // -1 izq / +1 der
    }

    private bool InAttackRange()
    {
        if (attackPoint == null) return false;
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerMask);
        return (hit != null);
    }

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

    // ========= REACCIÓN AL DAÑO (Enemy) =========

    // Para compatibilidad si alguien llama sin posición:
    public void TomarDanio(float danio) => TomarDanio(danio, transform.position + Vector3.left);

    // Nuevo: con posición del atacante
    public void TomarDanio(float danio, Vector2 attackerPos)
    {
        if (isDead) return;

        vida -= danio;

        if (vida > 0f)
        {
            PlayHitReaction(attackerPos);
        }
        else
        {
            Muerte();
        }
    }
    private void PlayHitReaction(Vector2 attackerPos)
    {
        // si ya hay una reacción en curso, la reiniciamos para que se vea otra vez
        if (_hitCR != null) StopCoroutine(_hitCR);
        _hitCR = EnemyHitSequence(attackerPos);
        StartCoroutine(_hitCR);
    }
    


    private System.Collections.IEnumerator EnemyKnockback(Vector2 dir)
    {
        hitStunned = true;

        float t = 0f;
        while (t < knockbackDuration)
        {
            float step = (knockbackDistance / knockbackDuration) * Time.deltaTime;
            float moved = EnemyCastAndMove(dir, step);
            if (moved <= 0f) break;
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(hitStunTime);
        hitStunned = false;
    }

    private float EnemyCastAndMove(Vector2 dir, float distance)
    {
        if (distance <= 0f) return 0f;
        var myCol = GetComponent<Collider2D>();

        RaycastHit2D[] hits = new RaycastHit2D[4];
        int count = myCol.Cast(dir, hits, distance + skin);

        float allowed = distance;
        for (int i = 0; i < count; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;
            if (((1 << h.collider.gameObject.layer) & solidLayer) == 0) continue;

            float candidate = h.distance - skin;
            if (candidate < allowed) allowed = Mathf.Max(0f, candidate);
        }

        if (allowed > 0f)
        {
            transform.position = (Vector2)transform.position + dir * allowed;
            return allowed;
        }
        return 0f;
    }
        private System.Collections.IEnumerator EnemyHitSequence(Vector2 attackerPos)
        {
            hitStunned = true;

            // 1️⃣ Determinar hacia dónde mirar y reproducir la animación de golpe
            float faceX = (attackerPos.x < transform.position.x) ? -1f : 1f;
            animator.SetFloat("faceX", faceX);
            animator.SetBool("isMoving", false);

            animator.ResetTrigger("Hit");
            animator.SetTrigger("Hit");

            if (patrullar != null)
                patrullar.enabled = false;

            // 2️⃣ Knockback leve solo si tiene Rigidbody2D (rebote corto y realista)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = ((Vector2)transform.position - attackerPos).normalized;
                rb.linearVelocity = Vector2.zero; // reset previo
                rb.AddForce(dir * 2.5f, ForceMode2D.Impulse); // fuerza leve (ajustá si querés más/menos rebote)
            }

            // 3️⃣ Pequeño “stun” visual, el enemigo se queda quieto un instante
            yield return new WaitForSeconds(hitStunTime + 0.1f);

            // 4️⃣ Frena el movimiento residual y vuelve a perseguir
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            hitStunned = false;
            state = State.Chasing;
            _hitCR = null;
        }




    // ========= MUERTE / POOL =========

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}

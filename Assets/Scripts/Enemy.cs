using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking, Dead }
    private State state = State.Patrolling;

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
    [Tooltip("Nombre EXACTO del estado Idle (puede ser un BlendTree).")]
    [SerializeField] private string idleStateName = "IdleBT";
    [Tooltip("Nombre EXACTO del estado Walk (BlendTree).")]
    [SerializeField] private string walkStateName = "WalkBT";
    [SerializeField] private float crossFadeDuration = 0.05f;
    [Header("Ataque / Ajustes finos")]
    [SerializeField] private string attackStateName = "Ataque"; 
    [SerializeField] private float attackLockTime = 0.35f;     
    [SerializeField] private float stickRangeTime = 0.15f;     

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

        switch (state)
        {
            case State.Patrolling: TickPatrolling(); break;
            case State.Chasing:    TickChasing();    break;
            case State.Attacking:  TickAttacking();  break;
        }

        switch (state)
        {
            case State.Patrolling:
                if (dist <= detectionRange)
                {
                    state = State.Chasing;
                    if (patrullar != null) patrullar.enabled = false;
                }
                break;

            case State.Chasing:

                if (dist <= attackDistance)
                {
                    state = State.Attacking;
                }
                else if (dist > detectionRange * loseSightMultiplier)
                {
                    state = State.Patrolling;
                    if (patrullar != null) patrullar.enabled = true;  
                    SetAnim(0f, false);
                    SafeGoIdle();
                }
                break;

            case State.Attacking:
                if (dist > attackDistance + stopToAttackBuffer)
                {
                    state = State.Chasing;
                }
                break;
        }
    }


    private void TickPatrolling()
    {
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
        
        if (moving) moveX = Mathf.Sign(dir.x); 

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
        float dx = player.position.x - transform.position.x;
        float moveX = Mathf.Abs(dx) < 0.001f ? 0f : Mathf.Sign(dx);
        SetAnim(moveX, false); 

        bool enRango = InAttackRange();
        if (enRango) lastInRangeAt = Time.time;

        bool rangoPegajoso = (Time.time - lastInRangeAt) <= stickRangeTime;
        bool cooldownListo = Time.time >= _nextAttackAllowed;
        bool enLockDeAtaque = Time.time < attackLockUntil;

        if (!enLockDeAtaque && cooldownListo && (enRango || rangoPegajoso))
        {
            if (animator != null) animator.SetTrigger(triggerAtaque);

            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TomarDanio(danio);

            Debug.Log($"[ENEMY] Ataque disparado. Daño={danio}, enRango={enRango}, pegajoso={rangoPegajoso}");

            _nextAttackAllowed = Time.time + attackCooldown;
            attackLockUntil = Time.time + attackLockTime;
        }
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

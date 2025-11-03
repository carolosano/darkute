using UnityEngine;

public class Patrullar : MonoBehaviour
{
    [SerializeField] private float velocidadMovimiento = 2f;
    [SerializeField] private Transform[] puntosMovimientos;
    [SerializeField] private float distanciaMinima = 0.1f;

    private int indice;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (puntosMovimientos == null || puntosMovimientos.Length == 0)
        {
            enabled = false;
            return;
        }

        // Elegí un punto inicial
        indice = Random.Range(0, puntosMovimientos.Length);
        SetAnim(0f, false); // Idle al empezar
    }

    private void Update()
    {
        // Si me deshabilitan desde Enemy (en Chasing), no ejecuto nada
        if (!enabled) return;

        var objetivo = puntosMovimientos[indice];
        if (objetivo == null) { enabled = false; return; }

        Vector2 pos  = transform.position;
        Vector2 dest = objetivo.position;

        float dist = Vector2.Distance(pos, dest);

        if (dist > distanciaMinima)
        {
            // Mover SIEMPRE hacia el punto con MoveTowards (coherente con Enemy)
            float step = velocidadMovimiento * Time.deltaTime;
            Vector2 next = Vector2.MoveTowards(pos, dest, step);
            transform.position = next;

            // Para tu BlendTree de Walk (left/right) alimentamos moveX = signo de delta.x
            float dx = dest.x - pos.x;
            float moveX = Mathf.Abs(dx) < 0.0001f ? 0f : Mathf.Sign(dx);

            SetAnim(moveX, true);
        }
        else
        {
            // Llegó → elegir nuevo punto
            indice = Random.Range(0, puntosMovimientos.Length);
            SetAnim(0f, false); // Idle breve antes de ir al siguiente
        }
    }

    private void SetAnim(float moveX, bool isMoving)
    {
        if (!animator) return;
        animator.SetFloat("moveX", Mathf.Clamp(moveX, -1f, 1f));
        animator.SetBool ("isMoving", isMoving);
    }
}


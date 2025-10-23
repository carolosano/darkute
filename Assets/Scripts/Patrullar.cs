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

        indice = Random.Range(0, puntosMovimientos.Length);
        SetAnim(0f, false); // idle al iniciar
    }

    private void Update()
    {
        var objetivo = puntosMovimientos[indice];
        if (!objetivo) { enabled = false; return; }

        Vector2 pos = transform.position;
        Vector2 dest = objetivo.position;
        Vector2 dir = dest - pos;

        bool moving = dir.sqrMagnitude > (distanciaMinima * distanciaMinima);
        float moveX = 0f;

        if (moving)
        {
            // Eje dominante: si va más en X que en Y, usá ese signo; si es vertical, mantené la última mirada según destino.x
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                moveX = Mathf.Sign(dir.x);
            else
                moveX = Mathf.Sign(dest.x - pos.x);

            // Mover
            Vector2 step = dir.normalized * (velocidadMovimiento * Time.deltaTime);
            transform.position = pos + step;
        }
        else
        {
            // Llegó → elegir nuevo punto
            indice = Random.Range(0, puntosMovimientos.Length);
        }

        SetAnim(moveX, moving);
    }

    private void SetAnim(float moveX, bool isMoving)
    {
        if (!animator) return;
        animator.SetFloat("moveX", Mathf.Clamp(moveX, -1f, 1f));
        animator.SetBool ("isMoving", isMoving);
    }
}

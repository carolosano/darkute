using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Combat : MonoBehaviour
{
    [SerializeField] private Transform controladorGolpe;
    [SerializeField] private float radioGolpe = 1f;
    [SerializeField] private float danioGolpe = 20f;
    [SerializeField] private string triggerNombre = "Ataque"; 

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            HacerAtaque();
        }
    }

    private void HacerAtaque()
    {
        // animación
        if (animator != null)
        {
            animator.SetTrigger(triggerNombre);
        }

        
        Golpe();

        Debug.Log("Ataque ejecutado (K) - trigger: " + triggerNombre);
    }

    private void Golpe()
    {
        if (controladorGolpe == null) return;

        Collider2D[] objetos = Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe);

        foreach (Collider2D colisionador in objetos)
        {
            // 1) Ignorarme a mí mismo (por si acaso)
            if (colisionador.gameObject == this.gameObject) continue;

            // 2) Solo pegarle a enemigos (esto ya lo tenías)
            if (colisionador.CompareTag("Enemigo"))
            {
                Enemy e = colisionador.GetComponent<Enemy>();
                if (e != null) e.TomarDanio(danioGolpe);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (controladorGolpe == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
    }

    
    public void GolpePorAnimEvent()
    {
        Golpe();
    }
}


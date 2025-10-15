using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float vida;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
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
        animator.SetTrigger("Muerte");
    }
}

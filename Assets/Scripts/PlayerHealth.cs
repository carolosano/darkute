using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float vida = 100f;

    public void TomarDanio(float danio)
    {
        vida -= danio;
        Debug.Log("Player recibió daño. Vida restante: " + vida);
        if (vida <= 0)
        {
            Debug.Log("El jugador murió");
        }
    }
}


using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidaMaxima;
    [SerializeField] private int vidaActual;
    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void TomarDanio(int danio)
    {
        int vidaTemporal = vidaActual - danio;
        vidaTemporal = Mathf.Clamp(vidaTemporal, 0, vidaMaxima);
        vidaActual = vidaTemporal;

        if (vidaActual <= 0)
        {
            DestruirJugador();
        }
    }

    private void DestruirJugador()
    {
        Destroy(gameObject);  
    } 

}

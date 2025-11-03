using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    [SerializeField] private float vidaMax = 100f;
    private float vida;

    private void Awake()
    {
        vida = vidaMax;
    }

    public void TomarDanio(float danio)
    {
        vida -= danio;
        Debug.Log($"[PLAYER DEBUG] Jugador recibió daño: -{danio} | Vida actual: {vida}");

        if (vida <= 0f)
        {
            Debug.Log("[PLAYER DEBUG] Jugador muerto.");
            Muerte();
        }
    }

    private void Muerte()
    {
        Debug.Log("[PLAYER DEBUG] Ejecutando muerte del jugador...");

        // Podés hacer una de estas acciones:
        // 1️⃣ Desactivar el jugador:
        gameObject.SetActive(false);

        // 2️⃣ O reiniciar la escena:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Curar(float cantidad)
    {
        vida = Mathf.Min(vida + cantidad, vidaMax);
        Debug.Log($"[PLAYER DEBUG] Jugador curado: +{cantidad} | Vida actual: {vida}");
    }
}



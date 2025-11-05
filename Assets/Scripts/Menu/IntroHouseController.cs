using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroHouseController : MonoBehaviour
{
    [Header("Configuración de la puerta")]
    [SerializeField] private Transform puerta;
    [SerializeField] private string nextSceneName = "SampleScene";

    private Transform player;
    private bool playerInDoor = false;

    private void Start()
    {
        // Buscamos al jugador automáticamente
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning("[IntroHouseController] No se encontró un objeto con tag 'Player'.");

        if (puerta == null)
            Debug.LogWarning("[IntroHouseController] Asigná el Transform de la puerta en el Inspector.");
    }

    private void Update()
    {
        if (playerInDoor && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[IntroHouseController] Tecla E presionada dentro del trigger. Cargando escena...");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == player)
        {
            playerInDoor = true;
            Debug.Log("[IntroHouseController] Jugador dentro del área de la puerta.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == player)
        {
            playerInDoor = false;
            Debug.Log("[IntroHouseController] Jugador salió del área de la puerta.");
        }
    }

    private void OnDrawGizmos()
    {
        if (puerta == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(puerta.position, new Vector3(1f, 1f, 0));
    }
}


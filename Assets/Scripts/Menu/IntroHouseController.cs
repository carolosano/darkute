using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroHouseController : MonoBehaviour
{
    [Header("Configuración de escena")]
    [SerializeField] private Transform player;                  // Referencia al jugador
    [SerializeField] private Transform puerta;                  // Puerta o punto de salida
    [SerializeField] private string nextScene = "SampleScene";  // Escena a cargar
    [SerializeField] private KeyCode useKey = KeyCode.E;        // Tecla para usar la puerta

    private bool playerInDoor = false;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // Si el jugador está en el área de la puerta y presiona la tecla, carga la siguiente escena
        if (playerInDoor && Input.GetKeyDown(useKey))
        {
            Debug.Log("[IntroHouse] Puerta usada, cargando siguiente escena...");
            SceneManager.LoadScene(nextScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detecta si el jugador entró al área de la puerta
        if (other.CompareTag("Player") && puerta && other.transform == player)
        {
            playerInDoor = true;
            Debug.Log("[IntroHouse] Jugador cerca de la puerta. Presioná E para salir.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Detecta si el jugador salió del área de la puerta
        if (other.CompareTag("Player") && other.transform == player)
        {
            playerInDoor = false;
        }
    }

    // Dibuja el área de detección en la escena
    private void OnDrawGizmos()
    {
        if (puerta == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(puerta.position, new Vector3(1f, 1f, 0));
    }
}



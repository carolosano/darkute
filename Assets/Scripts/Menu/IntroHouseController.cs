using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroHouseController : MonoBehaviour
{
    [Header("Configuración de escena")]
    [SerializeField] private Transform player;                  
    [SerializeField] private Transform puerta;                 
    [SerializeField] private string nextScene = "SampleScene"; 
    [SerializeField] private KeyCode useKey = KeyCode.E;        

    private bool playerInDoor = false;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        if (playerInDoor && Input.GetKeyDown(useKey))
        {
            Debug.Log("[IntroHouse] Puerta usada, cargando siguiente escena...");
            SceneManager.LoadScene(nextScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && puerta && other.transform == player)
        {
            playerInDoor = true;
            Debug.Log("[IntroHouse] Jugador cerca de la puerta. Presioná E para salir.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == player)
        {
            playerInDoor = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (puerta == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(puerta.position, new Vector3(1f, 1f, 0));
    }
}



using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroHouseController : MonoBehaviour
{
    [Header("Configuración de la puerta")]
    [SerializeField] private Transform puerta;
    [SerializeField] private string nextSceneName = "SampleScene";

    [Header("UI Prompt")]
    [SerializeField] private Image promptImage;             
    [SerializeField] private CanvasGroup promptCanvasGroup; 
    [SerializeField] private float promptFadeSpeed = 10f;

    private Transform player;
    private bool playerInDoor = false;
    private float targetAlpha = 0f;

    private void Start()
    {

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning("[IntroHouseController] No se encontró un objeto con tag 'Player'.");

        if (puerta == null)
            Debug.LogWarning("[IntroHouseController] Asigná el Transform de la puerta en el Inspector.");

        if (promptCanvasGroup) promptCanvasGroup.alpha = 0f;
        if (promptImage) promptImage.enabled = (promptCanvasGroup == null) ? false : promptImage.enabled;
    }

    private void Update()
    {
        UpdatePromptFade();

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
            SetPromptVisible(true);
            Debug.Log("[IntroHouseController] Jugador dentro del área de la puerta.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == player)
        {
            playerInDoor = false;
            SetPromptVisible(false);
            Debug.Log("[IntroHouseController] Jugador salió del área de la puerta.");
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptCanvasGroup)
            targetAlpha = visible ? 1f : 0f;
        else if (promptImage)
            promptImage.enabled = visible;
    }

    private void UpdatePromptFade()
    {
        if (!promptCanvasGroup) return;
        if (Mathf.Approximately(promptCanvasGroup.alpha, targetAlpha)) return;

        promptCanvasGroup.alpha = Mathf.MoveTowards(
            promptCanvasGroup.alpha,
            targetAlpha,
            promptFadeSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmos()
    {
        if (puerta == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(puerta.position, new Vector3(1f, 1f, 0));
    }
}


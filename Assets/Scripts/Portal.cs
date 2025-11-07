using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Configuración del Portal")]
    [SerializeField] private string sceneToLoad = "EscenaPortal";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI Prompt")]
    [SerializeField] private Image promptImage;             // Imagen "Presioná E"
    [SerializeField] private CanvasGroup promptCanvasGroup; // Opcional: para fade
    [SerializeField] private float promptFadeSpeed = 10f;

    private bool playerInRange = false;
    private float targetAlpha = 0f;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void Awake()
    {
        if (promptCanvasGroup) promptCanvasGroup.alpha = 0f;
        if (promptImage) promptImage.enabled = (promptCanvasGroup == null) ? false : promptImage.enabled;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        Debug.Log("Jugador dentro del portal. Presiona E para entrar.");
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        SetPromptVisible(false);
    }

    private void Update()
    {
        UpdatePromptFade();

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Debug.Log("Cambiando a escena: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptCanvasGroup)
        {
            targetAlpha = visible ? 1f : 0f;
        }
        else if (promptImage)
        {
            promptImage.enabled = visible;
        }
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
}

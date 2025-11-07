using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneChangeTrigger : MonoBehaviour
{
    [Header("Escena destino")]
    [SerializeField] private string sceneToLoad = "IntroDueño"; 

    [Header("Interacción")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float delayBeforeLoad = 0f;

    [Header("UI Prompt")]
    [SerializeField] private Image promptImage;            
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private float promptFadeSpeed = 10f;   

    private bool playerInRange;
    private bool triggered;
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
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        SetPromptVisible(false);
    }

    private void Update()
    {
        UpdatePromptFade();

        if (!playerInRange || triggered) return;

        if (Input.GetKeyDown(interactKey))
        {
            triggered = true;
            if (delayBeforeLoad > 0f) Invoke(nameof(LoadTargetScene), delayBeforeLoad);
            else LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneChangeTrigger] sceneToLoad no asignado.");
            triggered = false;
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"[SceneChangeTrigger] La escena '{sceneToLoad}' no está en Build Settings o el nombre no coincide.");
            triggered = false;
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
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

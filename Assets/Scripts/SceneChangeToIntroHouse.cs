using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class SceneChangeToIntroHouse : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "IntroHouse";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float delayBeforeLoad = 0f;

#if ENABLE_INPUT_SYSTEM
    [Header("New Input System (opcional)")]
    [SerializeField] private InputActionReference interactAction; // Button
#endif

    private bool playerInRange;
    private bool triggered;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        Debug.Log("[SceneChangeToIntroHouse] Player ENTRÓ al trigger.");
        // (opcional) mostrar UI "Presioná E para entrar"
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        Debug.Log("[SceneChangeToIntroHouse] Player SALIÓ del trigger.");
        // (opcional) ocultar UI
    }

    private void Update()
    {
        if (!playerInRange || triggered) return;

        bool pressed = Input.GetKeyDown(interactKey);
#if ENABLE_INPUT_SYSTEM
        if (!pressed && interactAction != null)
        {
            // Asegurate de Enable() la Action en tu InputActionsAsset o desde acá:
            if (!interactAction.action.enabled) interactAction.action.Enable();
            pressed = interactAction.action.WasPressedThisFrame();
        }
#endif
        if (!pressed) return;

        Debug.Log("[SceneChangeToIntroHouse] Tecla de interacción detectada. Intentando cargar escena...");
        triggered = true;

        if (delayBeforeLoad > 0f)
            Invoke(nameof(LoadTargetScene), delayBeforeLoad);
        else
            LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneChangeToIntroHouse] sceneToLoad vacío.");
            triggered = false;
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"[SceneChangeToIntroHouse] La escena '{sceneToLoad}' no está en Build Settings o el nombre no coincide.");
            triggered = false;
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnDrawGizmosSelected()
    {
        var c = GetComponent<Collider2D>();
        if (!c) return;
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        if (c is BoxCollider2D b)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(b.offset, b.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(b.offset, b.size);
        }
    }
}

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InteractDoor : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Collider2D que es el TRIGGER de interacción (no el sólido).")]
    [SerializeField] private Collider2D triggerRef;

    [Tooltip("Componente que desactiva el collider/sprite al abrir.")]
    [SerializeField] private LockedGate gateToOpen;

    [Header("Input (Legacy)")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

#if ENABLE_INPUT_SYSTEM
    [Header("Input (New Input System)")]
    [Tooltip("Acción tipo Button (ej: 'Interact'). Si se asigna, se usa esto además de la tecla E.")]
    [SerializeField] private InputActionReference interactAction;
#endif

    [Header("Mensajes / Debug")]
    [SerializeField] private string needKeyMessage = "Necesitas una llave para abrir esta puerta.";
    [SerializeField] private bool debugBypassKey = false; // para probar sin llave

    private bool playerInRange;
    private PlayerInventory playerInv;
    private bool used;

    private void Awake()
    {
        if (gateToOpen == null) gateToOpen = GetComponent<LockedGate>();
        if (triggerRef == null)
        {
            // Buscar uno que sea trigger en este GO
            foreach (var c in GetComponents<Collider2D>())
            {
                if (c.isTrigger) { triggerRef = c; break; }
            }
        }
        if (triggerRef == null)
            Debug.LogWarning("[InteractDoor] No se asignó triggerRef y no se encontró un Collider2D isTrigger en este GO.");
#if ENABLE_INPUT_SYSTEM
        if (interactAction != null) interactAction.action.Enable();
#endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerRef == null) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInv = other.GetComponent<PlayerInventory>();
            // Debug.Log("[InteractDoor] Player en rango.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (triggerRef == null) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInv = null;
            // Debug.Log("[InteractDoor] Player fuera de rango.");
        }
    }

    private void Update()
    {
        if (!playerInRange || used) return;

        bool pressed = Input.GetKeyDown(interactKey);
#if ENABLE_INPUT_SYSTEM
        if (!pressed && interactAction != null)
            pressed = interactAction.action.WasPressedThisFrame();
#endif
        if (pressed) TryOpen();
    }

    private void TryOpen()
    {
        if (used) return;

        if (!debugBypassKey)
        {
            if (playerInv == null)
            {
                Debug.LogWarning("[InteractDoor] PlayerInventory no encontrado en el Player.");
                return;
            }
            if (!playerInv.HasKey)
            {
                Debug.Log(needKeyMessage);
                return;
            }
            if (!playerInv.ConsumeKey())
            {
                Debug.Log(needKeyMessage);
                return;
            }
        }

        used = true;

        if (gateToOpen != null)
        {
            gateToOpen.Open();
            if (ProgressManager.Instance != null)
                ProgressManager.Instance.SetStage(ProgressStage.CP6_DoorOpened);
            Debug.Log("[InteractDoor] Puerta abierta.");
        }
        else
        {
            Debug.LogWarning("[InteractDoor] gateToOpen no asignado. No se abrió nada.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visual ayuda: caja alrededor del trigger
        if (triggerRef != null)
        {
            Gizmos.matrix = triggerRef.transform.localToWorldMatrix;
            Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
            var box = triggerRef as BoxCollider2D;
            if (box) Gizmos.DrawCube(box.offset, box.size);
        }
    }
}


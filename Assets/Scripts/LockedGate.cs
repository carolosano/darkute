using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockedGate : MonoBehaviour
{
    [Header("Apertura")]
    [SerializeField] private bool autoOpenWhenKeyObtained = true;
    [SerializeField] private GameObject[] visualsToHide; // sprites/tiles que quieras ocultar

    private Collider2D col;
    private bool opened;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        // Asegurate que sea sólido (IsTrigger = false) para bloquear
        col.isTrigger = false;
    }

    private void OnEnable()
    {
        PlayerInventory.OnKeyObtained += HandleKey;
    }

    private void OnDisable()
    {
        PlayerInventory.OnKeyObtained -= HandleKey;
    }

    private void HandleKey(PlayerInventory inv)
    {
        if (!autoOpenWhenKeyObtained || opened) return;
        Open();
    }

    [ContextMenu("Open (Editor)")]
    public void Open()
    {
        if (opened) return;
        opened = true;

        if (col != null) col.enabled = false;
        foreach (var go in visualsToHide)
        {
            if (go != null) go.SetActive(false);
        }
        Debug.Log("[LockedGate] Puerta/Canal desbloqueado.");
    }
}

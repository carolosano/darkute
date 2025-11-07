using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockedGate : MonoBehaviour
{
    [Header("Apertura")]
    [SerializeField] private bool autoOpenWhenKeyObtained = false; 
    [SerializeField] private GameObject[] visualsToHide; 

    private Collider2D col;
    private bool opened;
    public bool IsOpened => opened;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
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
        gameObject.SetActive(false);
    }
}

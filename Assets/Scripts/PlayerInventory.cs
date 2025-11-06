using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    public static event Action<PlayerInventory> OnKeyObtained;

    [SerializeField] private int keys = 0;

    public void AddKey()
    {
        keys = Mathf.Max(0, keys + 1);
        OnKeyObtained?.Invoke(this);
        Debug.Log($"[PlayerInventory] Llave obtenida. Total: {keys}");
    }

    public bool HasKey => keys > 0;

    public bool ConsumeKey()
    {
        if (keys <= 0) return false;
        keys--;
        return true;
    }
}



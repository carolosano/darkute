// PlayerInventory.cs
using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey { get; private set; }

    // Evento para avisar a puertas/locks
    public static event Action<PlayerInventory> OnKeyObtained;

    public void AddKey()
    {
        if (HasKey) return;
        HasKey = true;
        Debug.Log("[PlayerInventory] Llave obtenida.");
        OnKeyObtained?.Invoke(this);
    }
}

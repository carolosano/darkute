using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    [Tooltip("Opcional: sonido/partículas al recoger")]
    [SerializeField] private AudioSource sfx;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            inv.AddKey();
            if (sfx != null) sfx.Play();
        }

        Destroy(gameObject);
    }
}


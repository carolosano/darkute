using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [Header("Configuración del pickup")]
    [Tooltip("Cuántos medios corazones cura. 2 = 1 corazón completo.")]
    [SerializeField] private int halvesToHeal = 2;

    [Tooltip("Destruye el objeto inmediatamente al recogerlo.")]
    [SerializeField] private bool destroyOnPickup = true;

    [Tooltip("Opcional: sonido o efecto visual al recoger.")]
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private float sfxVolume = 0.8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo el jugador puede recogerlo
        if (!other.CompareTag("Player")) return;

        // Buscar componente de vida del jugador
        var playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Curar exactamente 1 corazón (2 medios)
            playerHealth.CurarMedios(halvesToHeal);

            Debug.Log("[HEART PICKUP] Jugador recogió un corazón (+1 vida).");
        }

        // Reproducir sonido si existe
        if (pickupSFX != null)
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position, sfxVolume);

        // Destruir el objeto al recogerlo
        if (destroyOnPickup)
            Destroy(gameObject);
    }
}



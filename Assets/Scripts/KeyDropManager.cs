using UnityEngine;

public class KeyDropManager : MonoBehaviour
{
    [Header("Prefab de la llave a spawnear")]
    [SerializeField] private GameObject keyPrefab;

    [SerializeField] private Vector2 dropOffset = new Vector2(0f, 0.35f);

    private int alive;
    private bool keySpawned;

    private void OnEnable()
    {
        Enemy.OnAnyEnemySpawned += HandleSpawned;
        Enemy.OnAnyEnemyDied    += HandleDied;
    }

    private void OnDisable()
    {
        Enemy.OnAnyEnemySpawned -= HandleSpawned;
        Enemy.OnAnyEnemyDied    -= HandleDied;
    }

    private void Start()
    {
        // Conteo inicial por si ya hay enemigos activos en escena
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        alive = 0;
        foreach (var e in enemies)
        {
            if (e != null && !e.IsDead) alive++;
        }

        keySpawned = false;
        Debug.Log($"[KeyDropManager] Enemigos vivos (inicio): {alive}");
    }

    private void HandleSpawned(Enemy e)
    {
        alive++;
        // Debug.Log($"[KeyDropManager] Spawn -> vivos: {alive}");
    }

    private void HandleDied(Enemy e)
    {
        alive = Mathf.Max(0, alive - 1);
        // Debug.Log($"[KeyDropManager] Muere -> vivos: {alive}");

        if (!keySpawned && alive == 0)
        {
            if (keyPrefab == null)
            {
                Debug.LogWarning("[KeyDropManager] keyPrefab no asignado, no se puede spawnear la llave.");
                return;
            }

            // Spawnear donde murió el último enemigo
            Vector3 pos = (e != null ? e.transform.position : Vector3.zero) + (Vector3)dropOffset;
            Instantiate(keyPrefab, pos, Quaternion.identity);
            keySpawned = true;
            Debug.Log("[KeyDropManager] ¡Último enemigo! Llave spawneada.");
        }
    }
}

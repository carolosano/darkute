using UnityEngine;
using System.Collections.Generic;

public class KeyDropManager : MonoBehaviour
{
    [Header("Prefab de la llave a spawnear")]
    [SerializeField] private GameObject keyPrefab;

    [SerializeField] private Vector2 dropOffset = new Vector2(0f, 0.35f);

    [Header("Opcional: validar total")]
    [Tooltip("Si > 0, solo dropea cuando deathsCount == expectedTotal y no queda nadie vivo.")]
    [SerializeField] private int expectedTotal = 0;

    private readonly HashSet<Enemy> aliveSet = new HashSet<Enemy>();
    private int deathsCount;
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
        // Poblado inicial por si ya hay enemigos en escena
        aliveSet.Clear();
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e != null && !e.IsDead)
                aliveSet.Add(e);
        }

        deathsCount = 0;
        keySpawned = false;
        Debug.Log($"[KeyDropManager] Vivos (inicio): {aliveSet.Count}");
    }

    private void HandleSpawned(Enemy e)
    {
        if (e == null) return;
        aliveSet.Add(e);
        // Debug.Log($"[KeyDropManager] Spawn -> vivos: {aliveSet.Count}");
    }

    private void HandleDied(Enemy e)
    {
        // Importante: remover solo si estaba en el set (evita doble decremento)
        bool removed = (e != null) && aliveSet.Remove(e);
        if (removed) deathsCount++;

        // Debug.Log($"[KeyDropManager] Muere -> vivos: {aliveSet.Count} | deaths: {deathsCount}");

        if (keySpawned) return;
        if (aliveSet.Count > 0) return;
        if (expectedTotal > 0 && deathsCount != expectedTotal) return;

        if (keyPrefab == null)
        {
            Debug.LogWarning("[KeyDropManager] keyPrefab no asignado, no se puede spawnear la llave.");
            return;
        }

        Vector3 pos = (e != null ? e.transform.position : Vector3.zero) + (Vector3)dropOffset;
        Instantiate(keyPrefab, pos, Quaternion.identity);
        keySpawned = true;
        Debug.Log("[KeyDropManager] ¡Último enemigo! Llave spawneada.");
        if (ProgressManager.Instance != null)
        ProgressManager.Instance.SetStage(ProgressStage.CP5_LastEnemyDown);
    }
}

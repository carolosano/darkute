using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private SimpleEnemyPool pool;
    [SerializeField] private Transform[] spawnPoints;

    public void SpawnOne()
    {
        if (spawnPoints.Length == 0) return;
        var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
        pool.Spawn(p.position, Quaternion.identity);
    }

    // Ejemplo: spawnear uno al inicio
    private void Start()
    {
        SpawnOne();
    }
}

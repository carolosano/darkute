using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemyPool : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialSize = 5;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var go = Instantiate(enemyPrefab, transform);
            go.SetActive(false);

            var enemy = go.GetComponent<Enemy>();
            if (enemy != null) enemy.poolOwner = this;

            pool.Enqueue(go);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
        }
        else
        {
            go = Instantiate(enemyPrefab, transform);
            var enemy = go.GetComponent<Enemy>();
            if (enemy != null) enemy.poolOwner = this;
        }

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);
        return go;
    }

    public void ReturnToPool(GameObject enemyGO)
    {
        enemyGO.SetActive(false);
        enemyGO.transform.SetParent(this.transform);
        enemyGO.transform.localPosition = Vector3.zero;
        enemyGO.transform.localRotation = Quaternion.identity;

        pool.Enqueue(enemyGO);
    }
}


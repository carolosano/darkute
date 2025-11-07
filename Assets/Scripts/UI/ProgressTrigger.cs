using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProgressTrigger : MonoBehaviour
{
    [SerializeField] private ProgressStage stageToSet = ProgressStage.CP4_MapZone;
    [SerializeField] private bool oneShot = true;

    private bool used;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetStage(stageToSet);
    }
}


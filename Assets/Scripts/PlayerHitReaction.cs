using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerHitReaction : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 0.6f;
    [SerializeField] private float knockbackDuration = 0.08f;
    [SerializeField] private float hitStunTime = 0.12f;
    [SerializeField] private LayerMask solidLayer;
    [SerializeField] private float skin = 0.02f;

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool stunned;

    private System.Collections.IEnumerator _hitCR;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        col  = GetComponent<Collider2D>();
    }

    public void OnHit(Vector2 attackerPos)
    {
        
        if (_hitCR != null) StopCoroutine(_hitCR);
        _hitCR = HitSequence(attackerPos);
        StartCoroutine(_hitCR);
    }

    private System.Collections.IEnumerator HitSequence(Vector2 attackerPos)
    {
        stunned = true;

        
        float faceX = (attackerPos.x < transform.position.x) ? -1f : 1f;
        anim.SetFloat("faceX", faceX);
        anim.SetBool("isMoving", false);

        
        anim.ResetTrigger("Hit");
        anim.SetTrigger("Hit");

        
        Vector2 dir = ((Vector2)transform.position - attackerPos).normalized;

        float t = 0f;
        while (t < knockbackDuration)
        {
            float step = (knockbackDistance / knockbackDuration) * Time.deltaTime;
            float moved = CastAndMove(dir, step);
            if (moved <= 0f) break;
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(hitStunTime);

        stunned = false;
        _hitCR = null;
    }

    private float CastAndMove(Vector2 dir, float distance)
    {
        if (distance <= 0f) return 0f;

        RaycastHit2D[] hits = new RaycastHit2D[4];
        int count = col.Cast(dir, hits, distance + skin);

        float allowed = distance;
        for (int i = 0; i < count; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;
            if (((1 << h.collider.gameObject.layer) & solidLayer) == 0) continue;

            float candidate = h.distance - skin;
            if (candidate < allowed) allowed = Mathf.Max(0f, candidate);
        }

        if (allowed > 0f)
        {
            rb.MovePosition(rb.position + dir * allowed);
            return allowed;
        }
        return 0f;
    }
}

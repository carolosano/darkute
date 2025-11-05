using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida (en corazones)")]
    [SerializeField] private int maxHearts = 4;     // total de corazones
    [SerializeField] private bool logDebug = true;  // logs útiles
    [SerializeField] private CanvasGroup fadePanel;


    // Vida actual en "medios corazones" (ej: 4 corazones = 8 medios)
    private int currentHalves;

    // Referencia al HUD
    private HeartsUI heartsUI;

    private void Awake()
    {
        currentHalves = maxHearts * 2;
    }

    private void Start()
    {
       
        heartsUI = FindFirstObjectByType<HeartsUI>();
        ActualizarHUD();
    }

    public void TomarDanio(float danio)
    {
        QuitarMediosCorazones(1);
    }

    public void QuitarMediosCorazones(int halvesToRemove)
    {
        int before = currentHalves;
        currentHalves = Mathf.Clamp(currentHalves - Mathf.Max(1, halvesToRemove), 0, maxHearts * 2);

        if (logDebug)
            Debug.Log($"[PLAYER] Daño recibido: -{halvesToRemove} medio(s). " +
                      $"Vida (medios): {before} -> {currentHalves}");

        ActualizarHUD();

        if (currentHalves <= 0)
            Muerte();
    }

    public void CurarMedios(int halvesToHeal)
    {
        int before = currentHalves;
        currentHalves = Mathf.Clamp(currentHalves + Mathf.Max(1, halvesToHeal), 0, maxHearts * 2);

        if (logDebug)
            Debug.Log($"[PLAYER] Curado: +{halvesToHeal} medio(s). " +
                      $"Vida (medios): {before} -> {currentHalves}");

        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        if (heartsUI == null) return;

        
        int fullHearts = currentHalves / 2;
       
        bool hasHalf = (currentHalves % 2) == 1;

        heartsUI.ActualizarHearts(fullHearts, hasHalf, maxHearts);
    }

    private void Muerte()
    {
        if (logDebug) Debug.Log("[PLAYER] Muerto.");

        var anim = GetComponent<Animator>();

        float faceX = 1f;
        if (anim != null)
        {
        
            var mx = anim.GetFloat("moveX");
            faceX = Mathf.Abs(mx) > 0.001f ? Mathf.Sign(mx) : 1f;

            anim.ResetTrigger("Hit");
            anim.ResetTrigger("Ataque");
            anim.SetFloat("faceX", faceX);
            anim.SetTrigger("Muerte");
        }

        var controller = GetComponent<PlayerController>();
        if (controller) controller.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        StartCoroutine(FadeOutAndLoadMenuMuerte());
    }



    private System.Collections.IEnumerator FadeOutAndLoadMenuMuerte()
    {
        yield return new WaitForSeconds(1.5f);

        if (fadePanel != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                fadePanel.alpha = t;
                yield return null;
            }
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuMuerte");
    }


}


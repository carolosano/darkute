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
        // Cada corazón = 2 medios
        currentHalves = maxHearts * 2;
    }

    private void Start()
    {
        // Unity 2023+: reemplaza FindObjectOfType por FindFirstObjectByType
        heartsUI = FindFirstObjectByType<HeartsUI>();
        ActualizarHUD();
    }

    /// <summary>
    /// Llamá esto cuando el jugador reciba daño. 
    /// Por diseño actual: cada golpe quita 1 medio corazón, sin importar el "danio" float.
    /// </summary>
    public void TomarDanio(float danio)
    {
        QuitarMediosCorazones(1);
    }

    /// <summary>
    /// Si en algún momento querés quitar N medios (p.ej. 2 medios = 1 corazón), usá este método.
    /// </summary>
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

    /// <summary>
    /// Curar en medios corazones (opcional).
    /// </summary>
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

        // Corazones enteros = currentHalves / 2
        int fullHearts = currentHalves / 2;
        // Hay medio corazón si sobra 1 half
        bool hasHalf = (currentHalves % 2) == 1;

        heartsUI.ActualizarHearts(fullHearts, hasHalf, maxHearts);
    }

    private void Muerte()
    {
        if (logDebug) Debug.Log("[PLAYER] Muerto.");

        var anim = GetComponent<Animator>();

        // Elegí hacia dónde mira al morir. Si guardás la última dirección en el Animator,
        // podés reutilizarla; si no, asumimos que ya venías setenando moveX/moveY.
        float faceX = 1f;
        if (anim != null)
        {
            // Si usás moveX/moveY como “mirada”, aprovechalos:
            var mx = anim.GetFloat("moveX");
            faceX = Mathf.Abs(mx) > 0.001f ? Mathf.Sign(mx) : 1f;

            // Limpiamos posibles triggers previos y disparamos muerte
            anim.ResetTrigger("Hit");
            anim.ResetTrigger("Ataque");
            anim.SetFloat("faceX", faceX);
            anim.SetTrigger("Muerte");
        }

        // Desactivar control/físicas si corresponde
        var controller = GetComponent<PlayerController>();
        if (controller) controller.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Fade + cambio de escena (si ya lo tenés implementado)
        StartCoroutine(FadeOutAndLoadMenuMuerte());
    }



    private System.Collections.IEnumerator FadeOutAndLoadMenuMuerte()
    {
        // Espera que se vea bien la animación (ajustá el tiempo según tu animación)
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


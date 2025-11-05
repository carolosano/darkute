using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenuController : MonoBehaviour
{
    [Header("Escenas destino")]
    [SerializeField] private string menuScene = "Menu";          // nombre exacto de tu escena de Menú
    [SerializeField] private string introHouseScene = "IntroHouse"; // nombre exacto de tu escena de intro jugable

    [Header("Referencias UI")]
    [SerializeField] private Button btnMenu;
    [SerializeField] private Button btnIntroHouse;

    [Header("Opcional: Fade al entrar")]
    [SerializeField] private CanvasGroup fadePanel; 
    [SerializeField] private float fadeInDuration = 0.4f;

    private void Awake()
    {
        
        if (btnMenu == null)    btnMenu = GameObject.Find("BtnMenu")?.GetComponent<Button>();
        if (btnIntroHouse == null) btnIntroHouse = GameObject.Find("BtnIntroHouse")?.GetComponent<Button>();

        if (btnMenu != null)        btnMenu.onClick.AddListener(GoMenu);
        if (btnIntroHouse != null)  btnIntroHouse.onClick.AddListener(GoIntroHouse);
    }

    private void Start()
    {
        
        if (fadePanel != null)
            StartCoroutine(FadeIn());
    }

    public void GoMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void GoIntroHouse()
    {
        SceneManager.LoadScene(introHouseScene);
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private System.Collections.IEnumerator FadeIn()
    {
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = false;
        float t = 0f;
        float start = 1f;
        float end = 0f;
        fadePanel.alpha = start;

        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, end, t / fadeInDuration);
            fadePanel.alpha = a;
            yield return null;
        }

        fadePanel.alpha = end;
        fadePanel.blocksRaycasts = false;
    }
}


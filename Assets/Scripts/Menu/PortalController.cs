using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PortalController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private VideoPlayer videoPlayer; 

    [Header("Siguiente escena")]
    [SerializeField] private string nextScene = "Menu"; 
    [Header("Fallback si el video falla")]
    [SerializeField] private float maxWaitSeconds = 30f;

    [Header("Opcional: permitir saltar")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private bool _done;
    private float _startTime;

    private void Reset()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        _startTime = Time.unscaledTime;

        Debug.Log($"[Menu] nextScene en runtime = '{nextScene}'");

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.Play();
            videoPlayer.SetDirectAudioVolume(0, 0.2f);

        }
        else
        {
            Debug.LogWarning("[Menu] No hay VideoPlayer asignado, cargando siguiente escena.");
            LoadNext();
        }
    }

    private void Update()
    {
        if (_done) return;

        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            Debug.Log("[Menu] Video saltado manualmente.");
            LoadNext();
            return;
        }

        if (Time.unscaledTime - _startTime > maxWaitSeconds)
        {
            Debug.LogWarning("[Menu] Timeout alcanzado, cargando siguiente escena.");
            LoadNext();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (_done) return;
        Debug.Log($"[Menu] Video terminado, cargando '{nextScene}'.");
        LoadNext();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        if (_done) return;
        Debug.LogError($"[Menu] Error en video: {message}. Cargando '{nextScene}'.");
        LoadNext();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    private void LoadNext()
    {
        if (_done) return;
        _done = true;

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("[Menu] No se configuró nextScene.");
            return;
        }

        // Verificá que la escena exista en Build Settings
        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError($"[Menu] La escena '{nextScene}' NO está en Build Settings o el nombre no coincide EXACTO.");
            return;
        }

        Debug.Log($"[Menu] Cargando escena: {nextScene} (desde PortalController). " +
                $"Stack:\n{new System.Diagnostics.StackTrace(true)}");

        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }

}

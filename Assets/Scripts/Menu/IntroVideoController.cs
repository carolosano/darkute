using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideoController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private VideoPlayer videoPlayer; 

    [Header("Siguiente escena")]
    [SerializeField] private string nextScene = "SampleScene"; 
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

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("[IntroVideo] No hay VideoPlayer asignado, cargando siguiente escena.");
            LoadNext();
        }
    }

    private void Update()
    {
        if (_done) return;

        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            Debug.Log("[IntroVideo] Video saltado manualmente.");
            LoadNext();
            return;
        }

        if (Time.unscaledTime - _startTime > maxWaitSeconds)
        {
            Debug.LogWarning("[IntroVideo] Timeout alcanzado, cargando siguiente escena.");
            LoadNext();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (_done) return;
        Debug.Log("[IntroVideo] Video terminado, cargando SampleScene.");
        LoadNext();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        if (_done) return;
        Debug.LogError($"[IntroVideo] Error en video: {message}. Cargando SampleScene.");
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
        SceneManager.LoadScene(nextScene);
    }
}



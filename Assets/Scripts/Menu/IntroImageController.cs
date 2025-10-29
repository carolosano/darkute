using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroImageController : MonoBehaviour
{
    [Header("Duración y escena siguiente")]
    [SerializeField] private float showSeconds = 3f;       
    [SerializeField] private string nextScene = "IntroVideo"; 

    [Header("Opcional: permitir saltar")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private bool _skipped;

    private void Start()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        float t = 0f;
        while (t < showSeconds && !_skipped)
        {
            if (allowSkip && Input.GetKeyDown(skipKey))
            {
                _skipped = true;
                break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}


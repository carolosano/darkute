using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenuOnKey : MonoBehaviour
{
    [SerializeField] private KeyCode key = KeyCode.M;
    [SerializeField] private string sceneName = "Menu";

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}



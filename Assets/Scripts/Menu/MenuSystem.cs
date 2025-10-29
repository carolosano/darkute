using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    public void Jugar()
    {
        Debug.Log("[MENU] Click en Jugar");
        SceneManager.LoadScene("IntroImage"); 
    }

    public void Salir()
    {
        Debug.Log("[MENU] Click en Salir");
        Application.Quit();
    }
}



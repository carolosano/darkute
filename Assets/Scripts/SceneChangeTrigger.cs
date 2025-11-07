using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "IntroDueño"; 
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float delayBeforeLoad = 0f; 

    private bool playerInRange;
    private bool triggered;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void Update()
    {
       
        if (playerInRange && !triggered && Input.GetKeyDown(interactKey))
        {
            triggered = true;

            if (delayBeforeLoad > 0f)
                Invoke(nameof(LoadTargetScene), delayBeforeLoad);
            else
                LoadTargetScene();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneChangeTrigger] sceneToLoad no asignado.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}

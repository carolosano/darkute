using UnityEngine;
using UnityEngine.SceneManagement;
using System;



// este script era para hacer checkpoints durante el juego y que muestre los objetivos que faltan pero al finl no tuve tiempo de i,plementarlo :)
public enum ProgressStage
{
    None = 0,
    CP1_IntroHouse = 1,
    CP2_IntroDueno = 2,
    CP3_BackToSample = 3,
    CP4_MapZone = 4,
    CP5_LastEnemyDown = 5,
    CP6_DoorOpened = 6
}

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    public static event Action<ProgressStage> OnStageChanged;

    [SerializeField] private ProgressStage current = ProgressStage.None;
    private bool visitedIntroDueno;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public ProgressStage Current => current;

    public void SetStage(ProgressStage stage)
    {
        if (stage == current) return;
        current = stage;

        OnStageChanged?.Invoke(current);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string s = scene.name;

  
        if (s == "IntroHouse" && current < ProgressStage.CP1_IntroHouse)
        {
            SetStage(ProgressStage.CP1_IntroHouse);
            return;
        }


        if (s == "IntroDueño" || s == "IntroDueno")
        {
            visitedIntroDueno = true;
            if (current < ProgressStage.CP2_IntroDueno)
                SetStage(ProgressStage.CP2_IntroDueno);
            return;
        }

        
        if (s == "SampleScene" && visitedIntroDueno && current < ProgressStage.CP3_BackToSample)
        {
            SetStage(ProgressStage.CP3_BackToSample);
        }
    }
}

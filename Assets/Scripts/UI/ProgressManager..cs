using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum ProgressStage
{
    None = 0,
    CP1_IntroHouse = 1,     // escena IntroHouse
    CP2_IntroDueno = 2,     // escena IntroDueño
    CP3_BackToSample = 3,   // volver a SampleScene después de IntroDueño
    CP4_MapZone = 4,        // trigger en el mapa
    CP5_LastEnemyDown = 5,  // último enemigo derrotado (dropea llave)
    CP6_DoorOpened = 6      // abre la puerta con la llave
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
        // Debug.Log($"[Progress] Stage => {current}");
        OnStageChanged?.Invoke(current);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string s = scene.name;

        // CP1: al entrar a IntroHouse
        if (s == "IntroHouse" && current < ProgressStage.CP1_IntroHouse)
        {
            SetStage(ProgressStage.CP1_IntroHouse);
            return;
        }

        // CP2: al entrar a IntroDueño
        if (s == "IntroDueño" || s == "IntroDueno")
        {
            visitedIntroDueno = true;
            if (current < ProgressStage.CP2_IntroDueno)
                SetStage(ProgressStage.CP2_IntroDueno);
            return;
        }

        // CP3: volver a SampleScene luego de IntroDueño
        if (s == "SampleScene" && visitedIntroDueno && current < ProgressStage.CP3_BackToSample)
        {
            SetStage(ProgressStage.CP3_BackToSample);
        }
    }
}

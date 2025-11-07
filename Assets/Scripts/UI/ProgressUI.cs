using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [Header("Referencia al Image de la UI")]
    [SerializeField] private Image targetImage;

    [Header("Sprites por checkpoint (índice = stage - 1)")]
    [SerializeField] private Sprite cp1;
    [SerializeField] private Sprite cp2;
    [SerializeField] private Sprite cp3;
    [SerializeField] private Sprite cp4;
    [SerializeField] private Sprite cp5;
    [SerializeField] private Sprite cp6;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        targetImage.enabled = false;
    }

    private void OnEnable()
    {
        ProgressManager.OnStageChanged += HandleStageChanged;
        
        if (ProgressManager.Instance != null)
            HandleStageChanged(ProgressManager.Instance.Current);
    }

    private void OnDisable()
    {
        ProgressManager.OnStageChanged -= HandleStageChanged;
    }

    private void HandleStageChanged(ProgressStage stage)
    {
        Sprite s = null;
        switch (stage)
        {
            case ProgressStage.CP1_IntroHouse:   s = cp1; break;
            case ProgressStage.CP2_IntroDueno:   s = cp2; break;
            case ProgressStage.CP3_BackToSample: s = cp3; break;
            case ProgressStage.CP4_MapZone:      s = cp4; break;
            case ProgressStage.CP5_LastEnemyDown:s = cp5; break;
            case ProgressStage.CP6_DoorOpened:   s = cp6; break;
            default: break;
        }

        if (s != null)
        {
            targetImage.sprite = s;
            targetImage.enabled = true;
        }
        else
        {
            targetImage.enabled = false;
        }
    }
}

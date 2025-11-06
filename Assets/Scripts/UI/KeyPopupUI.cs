using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeyPopupUI : MonoBehaviour
{
    [SerializeField] private Image popupImage;
    [SerializeField] private float showDuration = 1.2f;
    [SerializeField] private float fadeSpeed = 2f;

    private CanvasGroup canvasGroup;
    private Coroutine activeRoutine;

    private void Awake()
    {
        if (popupImage == null)
            popupImage = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Dejar el objeto ACTIVO, pero invisible e in-interactivo
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        // NO desactivar el GameObject acá
        // gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerInventory.OnKeyObtained += HandleKeyObtained;
    }

    private void OnDisable()
    {
        PlayerInventory.OnKeyObtained -= HandleKeyObtained;
    }

    private void HandleKeyObtained(PlayerInventory inv)
    {
        canvasGroup.alpha = 1f; // aparece y se queda
    }


    public void Show()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        // Asegurar visible
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showDuration);

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        // NO desactivar el GameObject, así seguimos suscritos al evento
        // gameObject.SetActive(false);
    }
}



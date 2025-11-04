using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Slots (imágenes) de corazones, en orden izquierda->derecha")]
    [SerializeField] private Image[] heartSlots;

    [Header("Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;

    /// <summary>
    /// Actualiza el HUD.
    /// fullHearts  = cantidad de corazones completos
    /// hasHalf     = true si hay medio corazón extra
    /// totalHearts = cantidad total de slots (p.ej. 4)
    /// </summary>
    public void ActualizarHearts(int fullHearts, bool hasHalf, int totalHearts)
    {
        if (heartSlots == null || heartSlots.Length == 0)
        {
            Debug.LogWarning("[HeartsUI] No hay heartSlots asignados.");
            return;
        }

        if (heartSlots.Length < totalHearts)
        {
            Debug.LogWarning($"[HeartsUI] Hay menos slots ({heartSlots.Length}) que totalHearts ({totalHearts}). " +
                             $"Ajustá el array en el Inspector.");
        }

        // Recorremos cada slot hasta totalHearts (si sobran en el array, los dejamos vacíos).
        for (int i = 0; i < heartSlots.Length; i++)
        {
            var img = heartSlots[i];
            if (img == null) continue;

            if (i < totalHearts)
            {
                // Dentro del total de corazones visibles
                if (i < fullHearts)
                {
                    img.sprite = heartFull;
                }
                else if (i == fullHearts && hasHalf)
                {
                    img.sprite = heartHalf;
                }
                else
                {
                    img.sprite = heartEmpty;
                }

                if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
            }
            else
            {
                // Si hay más slots de los necesarios, los oculto
                img.sprite = heartEmpty;
                if (img.gameObject.activeSelf) img.gameObject.SetActive(false);
            }
        }
    }
}



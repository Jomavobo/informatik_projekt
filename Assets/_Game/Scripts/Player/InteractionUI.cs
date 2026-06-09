using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("Prompt")]
    public TextMeshProUGUI promptText;

    [Header("Hold Ring")]
    public Image ringImage;      // Image mit Fill Method = Radial 360
    public Image crosshairDot;   // Kleiner Punkt in der Mitte

    [Header("Colors")]
    public Color promptActive   = Color.white;
    public Color promptBlocked  = new Color(1f, 0.4f, 0.4f);
    public Color ringColor      = Color.white;

    void Awake()
    {
        Instance = this;
        HidePrompt();
        UpdateHoldRing(0f);
    }

    public void ShowPrompt(string text, bool canInteract)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(true);
        promptText.text  = text;
        promptText.color = canInteract ? promptActive : promptBlocked;
    }

    public void HidePrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
        UpdateHoldRing(0f);
    }

    public void UpdateHoldRing(float progress)
    {
        if (ringImage == null) return;
        ringImage.fillAmount = progress;
        ringImage.gameObject.SetActive(progress > 0f);
    }
}

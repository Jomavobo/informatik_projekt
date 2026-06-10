using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Globales Alert-Banner das bei jedem aktiven Event erscheint.
/// Platziere auf einem Canvas-Panel.
/// </summary>
public class ShipAlertUI : MonoBehaviour
{
    public static ShipAlertUI Instance;

    [Header("References")]
    public GameObject alertPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI timerText;
    public Image           alertBackground;

    [Header("Colors")]
    public Color colorWarning  = new Color(0.9f, 0.6f, 0.1f);
    public Color colorCritical = new Color(0.9f, 0.1f, 0.1f);

    void Awake()
    {
        Instance = this;
        alertPanel?.SetActive(false);
    }

    void Update()
    {
        if (EventSystem.Instance == null || !EventSystem.Instance.eventActive)
        {
            alertPanel?.SetActive(false);
            return;
        }

        alertPanel?.SetActive(true);

        float t = EventSystem.Instance.eventTimeRemaining;
        if (timerText != null)
        {
            timerText.text  = $"{t:0}s";
            timerText.color = t < 10f ? colorCritical : Color.white;
        }
    }

    public void ShowEvent(ShipEventType type)
    {
        if (type == ShipEventType.None)
        {
            alertPanel?.SetActive(false);
            return;
        }

        var e = EventSystem.Instance?.GetCurrentEvent();
        if (e == null) return;

        alertPanel?.SetActive(true);

        if (titleText != null)       titleText.text       = e.title;
        if (descriptionText != null) descriptionText.text = e.description;

        if (alertBackground != null)
            alertBackground.color = type == ShipEventType.AutopilotFailure
                ? colorCritical : colorWarning;
    }
}
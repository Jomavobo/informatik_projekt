using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Brücken-Screen: zeigt Navigationsstatus und Autopilot-Alarm.
/// Bei Autopilot-Ausfall erscheint ein roter Popup-Button.
/// </summary>
public class BridgeScreen : MonoBehaviour
{
    [Header("Normal State")]
    public GameObject normalPanel;
    public TextMeshProUGUI autopilotStatusText;
    public TextMeshProUGUI courseText;

    [Header("Alert State")]
    public GameObject alertPanel;
    public Button     alertButton;
    public TextMeshProUGUI alertText;

    void Start()
    {
        // Event System abonnieren
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.OnEventStarted += OnEventStarted;
            EventSystem.Instance.OnEventEnded   += OnEventEnded;
        }

        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.OnEventStarted -= OnEventStarted;
            EventSystem.Instance.OnEventEnded   -= OnEventEnded;
        }
    }

    void OnEventStarted(ShipEventType type)
    {
        if (type == ShipEventType.AutopilotFailure)
            ShowAlert();
    }

    void OnEventEnded(ShipEventType type)
    {
        if (type == ShipEventType.AutopilotFailure)
            HideAlert();
    }

    void ShowAlert()
    {
        if (alertPanel  != null) alertPanel.SetActive(true);
        if (normalPanel != null) normalPanel.SetActive(false);

        if (autopilotStatusText != null)
        {
            autopilotStatusText.text  = "AUSFALL";
            autopilotStatusText.color = Color.red;
        }
    }

    void HideAlert()
    {
        if (alertPanel  != null) alertPanel.SetActive(false);
        if (normalPanel != null) normalPanel.SetActive(true);

        if (autopilotStatusText != null)
        {
            autopilotStatusText.text  = "AKTIV";
            autopilotStatusText.color = Color.green;
        }
    }

    void UpdateDisplay()
    {
        bool autopilotFailed = EventSystem.Instance != null &&
                               EventSystem.Instance.eventActive &&
                               EventSystem.Instance.activeEvent == ShipEventType.AutopilotFailure;

        if (autopilotFailed) ShowAlert();
        else                 HideAlert();
    }

    // Vom Alert-Button aufgerufen
    public void OnAlertButtonClicked()
    {
        AutopilotMinigame.Instance?.StartMinigame();
    }
}
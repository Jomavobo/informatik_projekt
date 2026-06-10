using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dev Panel zum manuellen Triggern aller Ship Events.
/// Nur im Editor sichtbar (#if UNITY_EDITOR oder via Toggle).
/// </summary>
public class EventDevPanel : MonoBehaviour
{
    public static EventDevPanel Instance;

    [Header("References")]
    public GameObject panel;
    public TextMeshProUGUI statusText;
    public KeyCode toggleKey = KeyCode.F1;

    void Awake()
    {
        Instance = this;
        // Im Build verstecken
        #if !UNITY_EDITOR
        // panel?.SetActive(false); // Auskommentieren zum Testen im Build
        #endif
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            panel?.SetActive(!panel.activeSelf);
    }

    // ── Button Callbacks ──────────────────────────────────────────────────────

    public void TriggerAutopilotFailure() => Trigger(ShipEventType.AutopilotFailure);
    public void TriggerPirateAttack()     => Trigger(ShipEventType.PirateAttack);
    public void TriggerAsteroidField()    => Trigger(ShipEventType.AsteroidField);
    public void TriggerFireOnBoard()      => Trigger(ShipEventType.FireOnBoard);
    public void TriggerPowerOutage()      => Trigger(ShipEventType.PowerOutage);
    public void TriggerDistressSignal()   => Trigger(ShipEventType.DistressSignal);
    public void ResolveCurrentEvent()     => EventSystem.Instance?.CmdResolveEvent();

    void Trigger(ShipEventType type)
    {
        if (EventSystem.Instance == null) { Debug.LogError("EventSystem nicht gefunden"); return; }
        if (EventSystem.Instance.eventActive) { Debug.Log("Event bereits aktiv"); return; }
        EventSystem.Instance.CmdTriggerEvent(type);
    }

    public void UpdateStatus()
    {
        if (statusText == null || EventSystem.Instance == null) return;
        statusText.text = EventSystem.Instance.eventActive
            ? $"AKTIV: {EventSystem.Instance.activeEvent}"
            : "Kein Event aktiv";
    }
}
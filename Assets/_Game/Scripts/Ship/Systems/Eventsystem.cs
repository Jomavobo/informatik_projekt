using Mirror;
using UnityEngine;
using System.Collections.Generic;

public enum ShipEventType
{
    None,
    AutopilotFailure,
    PirateAttack,
    AsteroidField,
    FireOnBoard,
    PowerOutage,
    DistressSignal
}

[System.Serializable]
public class ShipEvent
{
    public ShipEventType type;
    public string title;
    public string description;
    public float duration;
}

public class EventSystem : NetworkBehaviour
{
    public static EventSystem Instance;

    [SyncVar(hook = nameof(OnActiveEventChanged))]
    public ShipEventType activeEvent = ShipEventType.None;

    [SyncVar]
    public float eventTimeRemaining = 0f;

    [SyncVar]
    public bool eventActive = false;

    public List<ShipEvent> events = new List<ShipEvent>()
    {
        new ShipEvent { type = ShipEventType.AutopilotFailure, title = "AUTOPILOT AUSFALL",    description = "Manueller Eingriff erforderlich!",     duration = 30f },
        new ShipEvent { type = ShipEventType.PirateAttack,    title = "PIRATENANGRIFF",        description = "Feindliches Schiff in Sichtweite!",    duration = 60f },
        new ShipEvent { type = ShipEventType.AsteroidField,   title = "ASTEROIDENFELD",        description = "Ausweichmanöver erforderlich!",        duration = 45f },
        new ShipEvent { type = ShipEventType.FireOnBoard,     title = "FEUER AN BORD",         description = "Feuer im Maschinenraum!",              duration = 40f },
        new ShipEvent { type = ShipEventType.PowerOutage,     title = "STROMAUSFALL",          description = "Reaktor neu starten!",                 duration = 50f },
        new ShipEvent { type = ShipEventType.DistressSignal,  title = "NOTSIGNAL EMPFANGEN",   description = "Quelle unbekannt. Vorsicht geboten!", duration = 90f },
    };

    // Events die andere Systeme abonnieren können
    public System.Action<ShipEventType> OnEventStarted;
    public System.Action<ShipEventType> OnEventEnded;

    void Awake() => Instance = this;

    void Update()
    {
        if (!isServer || !eventActive) return;

        eventTimeRemaining -= Time.deltaTime;
        if (eventTimeRemaining <= 0f)
            EndCurrentEvent();
    }

    // ── Server: Event triggern ────────────────────────────────────────────────

    [Server]
    public void TriggerEvent(ShipEventType type)
    {
        if (eventActive) return;

        var e = events.Find(x => x.type == type);
        if (e == null) return;

        activeEvent        = type;
        eventTimeRemaining = e.duration;
        eventActive        = true;
    }

    [Server]
    public void EndCurrentEvent()
    {
        var ended = activeEvent;
        activeEvent    = ShipEventType.None;
        eventActive    = false;
        eventTimeRemaining = 0f;

        RpcOnEventEnded(ended);
    }

    [Server]
    public void ResolveEvent()
    {
        if (!eventActive) return;
        EndCurrentEvent();
    }

    // ── Commands (von Clients aufrufbar) ──────────────────────────────────────

    [Command(requiresAuthority = false)]
    public void CmdTriggerEvent(ShipEventType type) => TriggerEvent(type);

    [Command(requiresAuthority = false)]
    public void CmdResolveEvent() => ResolveEvent();

    // ── RPCs ──────────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcOnEventEnded(ShipEventType type)
    {
        OnEventEnded?.Invoke(type);
    }

    // ── SyncVar Hook ──────────────────────────────────────────────────────────

    void OnActiveEventChanged(ShipEventType oldVal, ShipEventType newVal)
    {
        if (newVal != ShipEventType.None)
            OnEventStarted?.Invoke(newVal);

        EventDevPanel.Instance?.UpdateStatus();
        ShipAlertUI.Instance?.ShowEvent(newVal);
    }

    public ShipEvent GetCurrentEvent()
        => events.Find(x => x.type == activeEvent);
}
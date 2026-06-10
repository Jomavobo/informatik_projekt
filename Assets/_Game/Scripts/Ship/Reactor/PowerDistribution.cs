using Mirror;
using UnityEngine;
using System.Collections.Generic;

public enum ShipSystem
{
    LifeSupport,
    Weapons,
    Shields,
    Warp,
    Workshop,
    Sensors
}

[System.Serializable]
public class PowerAllocation
{
    public ShipSystem system;
    public float allocated;
    public float minimum;
    public float optimal;
}

public class PowerDistribution : NetworkBehaviour
{
    public static PowerDistribution Instance;

    [Header("Allocations")]
    public List<PowerAllocation> allocations = new List<PowerAllocation>()
    {
        new PowerAllocation { system = ShipSystem.LifeSupport, allocated = 10f, minimum = 5f,  optimal = 10f },
        new PowerAllocation { system = ShipSystem.Weapons,     allocated = 20f, minimum = 0f,  optimal = 30f },
        new PowerAllocation { system = ShipSystem.Shields,     allocated = 20f, minimum = 0f,  optimal = 25f },
        new PowerAllocation { system = ShipSystem.Warp,        allocated = 30f, minimum = 0f,  optimal = 40f },
        new PowerAllocation { system = ShipSystem.Workshop,    allocated = 10f, minimum = 0f,  optimal = 15f },
        new PowerAllocation { system = ShipSystem.Sensors,     allocated = 10f, minimum = 0f,  optimal = 10f },
    };

    [SyncVar]
    public float reactorLimit = 100f;

    void Awake()
    {
        Instance = this;
    }

    // Beim Client-Start alle Werte synchronisieren
    public override void OnStartClient()
    {
        PowerDistributionUI.Instance?.UpdateUI();
    }

    void Update()
    {
        if (!isServer) return;
        UpdateReactorLoad();
        UpdateLifeSupport();
    }

    [Server]
    void UpdateReactorLoad()
    {
        float total = 0f;
        foreach (var a in allocations)
            total += a.allocated;

        if (ReactorSystem.Instance != null)
            ReactorSystem.Instance.currentLoad = total;
    }

    [Server]
    void UpdateLifeSupport()
    {
        if (ReactorSystem.Instance == null) return;

        var ls = allocations.Find(a => a.system == ShipSystem.LifeSupport);
        bool active = ls != null && ls.allocated >= ls.minimum;
        ReactorSystem.Instance.SetLifeSupport(active);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetAllocation(ShipSystem system, float amount)
    {
        var alloc = allocations.Find(a => a.system == system);
        if (alloc == null) return;

        alloc.allocated = Mathf.Max(0f, amount);
        RpcUpdateAllocation(system, alloc.allocated);
    }

    [ClientRpc]
    void RpcUpdateAllocation(ShipSystem system, float amount)
    {
        var alloc = allocations.Find(a => a.system == system);
        if (alloc == null) return;
        alloc.allocated = amount;
        PowerDistributionUI.Instance?.UpdateUI();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetReactorLimit(float limitPercent)
    {
        reactorLimit = Mathf.Clamp(limitPercent, 0f, 150f);
        RpcUpdateReactorLimit(reactorLimit);
    }

    [ClientRpc]
    void RpcUpdateReactorLimit(float limit)
    {
        reactorLimit = limit;
        PowerDistributionUI.Instance?.UpdateUI();
    }

    public float GetSystemEfficiency(ShipSystem system)
    {
        var alloc = allocations.Find(a => a.system == system);
        if (alloc == null) return 0f;
        if (alloc.optimal <= 0f) return 1f;
        return Mathf.Clamp01(alloc.allocated / alloc.optimal);
    }

    public bool IsSystemOnline(ShipSystem system)
    {
        var alloc = allocations.Find(a => a.system == system);
        if (alloc == null) return false;
        return alloc.allocated >= alloc.minimum;
    }
}
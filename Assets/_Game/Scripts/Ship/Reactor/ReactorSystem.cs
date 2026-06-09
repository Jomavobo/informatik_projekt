using Mirror;
using UnityEngine;

public class ReactorSystem : NetworkBehaviour
{
    public static ReactorSystem Instance;

    [Header("Reactor Stats")]
    public float maxCapacity = 100f;
    public float overloadTolerancePercent = 0.10f;
    public float overloadGracePeriod = 10f;

    [Header("Life Support")]
    public float lifeSupportTimer = 120f;

    [SyncVar(hook = nameof(OnIntegrityChanged))]
    public float integrity = 100f;

    [SyncVar(hook = nameof(OnLifeSupportChanged))]
    public bool lifeSupportActive = true;

    [SyncVar]
    public float lifeSupportTimeRemaining;

    [SyncVar]
    public float currentLoad = 0f;

    private float overloadTimer = 0f;
    private bool isOverloaded = false;

    void Awake()
    {
        Instance = this;
        lifeSupportTimeRemaining = lifeSupportTimer;
    }

public float MaxAvailableCapacity
{
    get
    {
        float limit = PowerDistribution.Instance != null
            ? PowerDistribution.Instance.reactorLimit / 100f
            : 1f;
        return maxCapacity * (integrity / 100f) * limit;
    }
}

    public float LoadPercent
    {
        get
        {
            if (MaxAvailableCapacity <= 0f) return 1f;
            return currentLoad / MaxAvailableCapacity;
        }
    }

    public bool IsOverloaded => isOverloaded;
    public float OverloadTimer => overloadTimer;
    public float OverloadPercent => Mathf.Clamp01(
        (LoadPercent - 1f) / overloadTolerancePercent);

    void Update()
    {
        if (!isServer) return;

        HandleOverload();
        HandleLifeSupport();
    }

    [Server]
    void HandleOverload()
    {
        // Überschuss in absoluten Einheiten – unabhängig von sinkender Integrität
        float maxBeforeDamage = maxCapacity * (integrity / 100f);
        float toleranceThreshold = maxBeforeDamage * (1f + overloadTolerancePercent);
        float absoluteExcess = currentLoad - toleranceThreshold;

        if (absoluteExcess > 0f)
        {
            isOverloaded = true;
            overloadTimer += Time.deltaTime;

            if (overloadTimer >= overloadGracePeriod)
            {
                // Fixer Schaden pro Sekunde, kein exponentieller Anstieg
                float damage = 2f * Time.deltaTime;
                TakeDamage(damage);
            }
        }
        else
        {
            isOverloaded = false;
            overloadTimer = Mathf.Max(0f, overloadTimer - Time.deltaTime * 2f);
        }
    }

    [Server]
    void HandleLifeSupport()
    {
        if (!lifeSupportActive)
        {
            lifeSupportTimeRemaining -= Time.deltaTime;

            if (lifeSupportTimeRemaining <= 0f)
            {
                lifeSupportTimeRemaining = 0f;
                RpcKillAllPlayers();
            }
        }
        else
        {
            lifeSupportTimeRemaining = Mathf.Min(lifeSupportTimer,
                lifeSupportTimeRemaining + Time.deltaTime * 0.5f);
        }
    }

    [Server]
    public void TakeDamage(float amount)
    {
        integrity = Mathf.Max(0f, integrity - amount);
    }

    [Server]
    public void Repair(float amount)
    {
        integrity = Mathf.Min(100f, integrity + amount);
    }

    [Command(requiresAuthority = false)]
    public void CmdRepair(float amount)
    {
        Repair(amount);
    }

    [Server]
    public void SetLifeSupport(bool active)
    {
        lifeSupportActive = active;
    }

    void OnIntegrityChanged(float oldVal, float newVal)
    {
        ReactorUI.Instance?.UpdateIntegrity(newVal);
    }

    void OnLifeSupportChanged(bool oldVal, bool newVal)
    {
        ReactorUI.Instance?.UpdateLifeSupport(newVal, lifeSupportTimeRemaining);
    }

    [ClientRpc]
    void RpcKillAllPlayers()
    {
        GameManager.Instance?.TriggerGameOver("Lebenserhaltung ausgefallen");
    }

    public override void OnStartClient()
    {
        PowerDistributionUI.Instance?.InitializeListeners();
    }
}
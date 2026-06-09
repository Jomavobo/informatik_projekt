using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PowerDistributionUI : MonoBehaviour
{
    public static PowerDistributionUI Instance;

    [System.Serializable]
    public class SystemSlotUI
    {
        public ShipSystem system;
        public Slider slider;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI efficiencyText;
        public Image statusIndicator;
    }

    [Header("System Slots")]
    public List<SystemSlotUI> systemSlots = new List<SystemSlotUI>();

    [Header("Totals")]
    public TextMeshProUGUI totalAllocatedText;
    public TextMeshProUGUI maxCapacityText;
    public Slider reactorLimitSlider;
    public TextMeshProUGUI reactorLimitText;

    [Header("Colors")]
    public Color onlineColor = Color.green;
    public Color offlineColor = Color.red;
    public Color overloadColor = Color.yellow;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var slot in systemSlots)
        {
            var s = slot;
            s.slider.onValueChanged.AddListener((val) => OnSliderChanged(s, val));
        }

        if (reactorLimitSlider != null)
            reactorLimitSlider.onValueChanged.AddListener(OnReactorLimitChanged);
    }

    // Wird von ReactorSystem.OnStartClient aufgerufen
    public void InitializeListeners()
    {
        UpdateUI();
    }

    void Update()
    {
        // Kontinuierlich updaten damit Totals immer aktuell sind
        UpdateTotals();
    }

    void OnSliderChanged(SystemSlotUI slot, float value)
    {
        if (PowerDistribution.Instance == null) return;
        PowerDistribution.Instance.CmdSetAllocation(slot.system, value);
        UpdateSlot(slot);
    }

    void OnReactorLimitChanged(float value)
    {
        if (PowerDistribution.Instance == null) return;
        PowerDistribution.Instance.CmdSetReactorLimit(value);

        if (reactorLimitText != null)
            reactorLimitText.text = $"Limit: {value:0}%";
    }

    public void UpdateUI()
    {
        foreach (var slot in systemSlots)
            UpdateSlot(slot);

        UpdateTotals();
    }

    void UpdateSlot(SystemSlotUI slot)
    {
        if (PowerDistribution.Instance == null) return;

        var alloc = PowerDistribution.Instance.allocations
            .Find(a => a.system == slot.system);

        if (alloc == null) return;

        if (slot.valueText != null)
            slot.valueText.text = $"{alloc.allocated:0} / {alloc.optimal:0}";

        float efficiency = PowerDistribution.Instance.GetSystemEfficiency(slot.system);
        bool online = PowerDistribution.Instance.IsSystemOnline(slot.system);

        if (slot.efficiencyText != null)
            slot.efficiencyText.text = online ? $"{efficiency * 100f:0}%" : "OFFLINE";

        if (slot.statusIndicator != null)
            slot.statusIndicator.color = online ? onlineColor : offlineColor;
    }

    void UpdateTotals()
    {
        if (PowerDistribution.Instance == null) return;
        if (ReactorSystem.Instance == null) return;

        float totalAllocated = 0f;
        foreach (var alloc in PowerDistribution.Instance.allocations)
            totalAllocated += alloc.allocated;

        float maxCap = ReactorSystem.Instance.MaxAvailableCapacity;

        if (totalAllocatedText != null)
            totalAllocatedText.text = $"Gesamt: {totalAllocated:0}";

        if (maxCapacityText != null)
        {
            maxCapacityText.text = $"Max: {maxCap:0}";
            maxCapacityText.color = totalAllocated > maxCap ? overloadColor : onlineColor;
        }
    }
}
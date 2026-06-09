using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactorUI : MonoBehaviour
{
    public static ReactorUI Instance;

    [Header("Reactor")]
    public Slider integritySlider;
    public Slider loadSlider;
    public TextMeshProUGUI integrityText;
    public TextMeshProUGUI loadText;
    public TextMeshProUGUI overloadTimerText;
    public Image overloadWarning;

    [Header("Life Support")]
    public TextMeshProUGUI lifeSupportTimerText;
    public GameObject lifeSupportWarningPanel;

    [Header("Colors")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (ReactorSystem.Instance == null) return;

        UpdateIntegrity(ReactorSystem.Instance.integrity);
        UpdateLoad();
        UpdateOverload();
        UpdateLifeSupportTimer();
    }

    public void UpdateIntegrity(float newIntegrity)
    {
        if (integritySlider != null)
            integritySlider.value = newIntegrity / 100f;

        if (integrityText != null)
            integrityText.text = $"Integrität: {newIntegrity:0}%";

        if (integritySlider != null)
        {
            var fill = integritySlider.fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = newIntegrity < 25f ? criticalColor
                           : newIntegrity < 50f ? warningColor
                           : normalColor;
        }
    }

    void UpdateLoad()
    {
        if (ReactorSystem.Instance == null) return;

        float load = ReactorSystem.Instance.LoadPercent;

        if (loadSlider != null)
            loadSlider.value = Mathf.Clamp01(load);

        if (loadText != null)
            loadText.text = $"Auslastung: {load * 100f:0}%";

        if (loadSlider != null)
        {
            var fill = loadSlider.fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = load > 1.1f ? criticalColor
                           : load > 1f ? warningColor
                           : normalColor;
        }
    }

    void UpdateOverload()
    {
        if (ReactorSystem.Instance == null) return;

        bool overloaded = ReactorSystem.Instance.IsOverloaded;

        if (overloadWarning != null)
            overloadWarning.gameObject.SetActive(overloaded);

        if (overloadTimerText != null)
        {
            if (overloaded)
            {
                float remaining = ReactorSystem.Instance.overloadGracePeriod
                                - ReactorSystem.Instance.OverloadTimer;
                overloadTimerText.text = remaining > 0f
                    ? $"Überlastung! Schaden in {remaining:0.0}s"
                    : "REAKTORSCHADEN!";
            }
            else
            {
                overloadTimerText.text = "";
            }
        }
    }

    void UpdateLifeSupportTimer()
    {
        if (ReactorSystem.Instance == null) return;

        bool active = ReactorSystem.Instance.lifeSupportActive;

        if (lifeSupportWarningPanel != null)
            lifeSupportWarningPanel.SetActive(!active);

        if (lifeSupportTimerText != null)
        {
            if (!active)
            {
                float t = ReactorSystem.Instance.lifeSupportTimeRemaining;
                lifeSupportTimerText.text = $"SAUERSTOFF: {t:0}s";
                lifeSupportTimerText.color = t < 30f ? criticalColor : warningColor;
            }
            else
            {
                lifeSupportTimerText.text = "Lebenserhaltung aktiv";
                lifeSupportTimerText.color = normalColor;
            }
        }
    }

    public void UpdateLifeSupport(bool active, float timeRemaining)
    {
        if (lifeSupportWarningPanel != null)
            lifeSupportWarningPanel.SetActive(!active);
    }
}
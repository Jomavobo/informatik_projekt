using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactorIntegrityDisplay : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI integrityText;   // Zeigt nur "100%"
    public TextMeshProUGUI statusText;      // Zeigt nur "NORMAL" / "ÜBERLASTUNG" etc.
    public TextMeshProUGUI lifeSupportText; // Zeigt nur "120s"
    public Button repairButton;

    [Header("Thresholds")]
    public float warningThreshold = 50f;
    public float criticalThreshold = 25f;

    [Header("Colors")]
    public Color colorNormal = Color.white;
    public Color colorWarning = new Color(1f, 0.85f, 0.2f);
    public Color colorCritical = new Color(1f, 0.3f, 0.3f);

    void Update()
    {
        if (ReactorSystem.Instance == null) return;

        float integrity = ReactorSystem.Instance.integrity;
        float load = ReactorSystem.Instance.LoadPercent * 100f;
        bool overloaded = ReactorSystem.Instance.IsOverloaded;
        bool lifeSupport = ReactorSystem.Instance.lifeSupportActive;

        Color integrityColor = integrity < criticalThreshold ? colorCritical
                             : integrity < warningThreshold ? colorWarning
                             : colorNormal;

        // Integrität – nur Zahl
        if (integrityText != null)
        {
            integrityText.text = $"{integrity:0}%";
            integrityText.color = integrityColor;
        }

        // Status – kurzer Text, nur wenn nicht normal
        if (statusText != null)
        {
            if (overloaded)
            {
                statusText.text = "ÜBERLASTUNG";
                statusText.color = colorCritical;
            }
            else if (!lifeSupport)
            {
                statusText.text = "KEIN SAUERSTOFF";
                statusText.color = colorCritical;
            }
            else if (load > 90f)
            {
                statusText.text = "HOHE AUSLASTUNG";
                statusText.color = colorWarning;
            }
            else
            {
                statusText.text = "● NORMAL";
                statusText.color = colorNormal;
            }
        }

        // Lebenserhaltung Timer – nur wenn aktiv
        if (lifeSupportText != null)
        {
            if (!lifeSupport)
            {
                float t = ReactorSystem.Instance.lifeSupportTimeRemaining;
                lifeSupportText.text = $"{t:0}s";
                lifeSupportText.color = t < 30f ? colorCritical : colorWarning;
                lifeSupportText.gameObject.SetActive(true);
            }
            else
            {
                lifeSupportText.gameObject.SetActive(false);
            }
        }

        // Repair Button nur anzeigen wenn unter 100%
        if (repairButton != null)
            repairButton.gameObject.SetActive(integrity < 100f);
    }
}
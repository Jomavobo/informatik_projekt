using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Generiert einen vollständigen World Space Screen mit Tabs und Panels.
/// Platziere dieses Script auf dem Monitor-GameObject, konfiguriere es im Inspector,
/// und klicke "Generate Screen UI" um das UI zu bauen.
/// </summary>
public class ShipScreenGenerator : MonoBehaviour
{
    [Header("Screen Settings")]
    public string screenTitle   = "TERMINAL";
    public Vector2 canvasSize   = new Vector2(1108, 876);
    public float   canvasScale  = 1.496129e-05f;

    [Header("Style")]
    public Color colorBackground  = new Color(0.05f, 0.05f, 0.08f, 0.98f);
    public Color colorHeader      = new Color(0.08f, 0.08f, 0.14f, 1f);
    public Color colorAccent      = new Color(0.15f, 0.55f, 0.85f, 1f);
    public Color colorTabActive   = new Color(0.15f, 0.45f, 0.75f, 1f);
    public Color colorTabInactive = new Color(0.12f, 0.12f, 0.18f, 1f);
    public Color colorText        = Color.white;
    public Color colorSubtext     = new Color(0.7f, 0.7f, 0.7f);
    public int   baseFontSize     = 28;

    [Header("Tabs")]
    public List<ScreenTab> tabs = new List<ScreenTab>();

    [System.Serializable]
    public class ScreenTab
    {
        public string tabName    = "Tab";
        public ScreenTabType type = ScreenTabType.Custom;
    }

    public enum ScreenTabType
    {
        Custom,
        ReactorStatus,
        PowerDistribution,
    }

    // Generierter Canvas (Referenz)
    [HideInInspector] public Canvas generatedCanvas;

    #if UNITY_EDITOR
    [ContextMenu("Generate Screen UI")]
    public void GenerateScreenUI()
    {
        // Alten Canvas löschen
        var existing = GetComponentInChildren<Canvas>();
        if (existing != null) DestroyImmediate(existing.gameObject);

        // Canvas erstellen
        var canvasGO = new GameObject("ScreenCanvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = canvasSize;
        rt.localScale = Vector3.one * canvasScale;
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;

        canvasGO.AddComponent<GraphicRaycaster>();

        // WorldSpaceCanvasSetup für Event Camera
        canvasGO.AddComponent<WorldSpaceCanvasSetup>();

        generatedCanvas = canvas;

        // Root Panel
        var root = CreatePanel(canvasGO, "Root",
            Vector2.zero, canvasSize, colorBackground);
        root.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        root.GetComponent<RectTransform>().anchorMax = Vector2.one;
        root.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Header
        float headerH = 80f;
        var header = CreatePanel(root, "Header",
            new Vector2(0, canvasSize.y / 2f - headerH / 2f),
            new Vector2(canvasSize.x, headerH), colorHeader);
        SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -headerH));

        CreateLabel(header, "ScreenTitle", screenTitle,
            Vector2.zero, new Vector2(canvasSize.x, headerH),
            baseFontSize + 4, colorAccent, true, TextAlignmentOptions.MidlineLeft,
            new Vector2(20, 0));

        // Accent Line unter Header
        var line = CreatePanel(root, "AccentLine",
            new Vector2(0, canvasSize.y / 2f - headerH - 2f),
            new Vector2(canvasSize.x, 4f), colorAccent);
        SetAnchors(line, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(headerH + 4f)));

        // Tab-Bereich (nur wenn mehr als ein Tab)
        float tabH    = tabs.Count > 1 ? 60f : 0f;
        float contentY = canvasSize.y / 2f - headerH - 4f - tabH;
        float contentH = canvasSize.y - headerH - 4f - tabH;

        List<GameObject> tabPanels   = new List<GameObject>();
        List<Button>     tabButtons  = new List<Button>();
        List<GameObject> contentPanels = new List<GameObject>();

        if (tabs.Count > 1)
        {
            var tabBar = CreatePanel(root, "TabBar",
                new Vector2(0, contentY - tabH / 2f),
                new Vector2(canvasSize.x, tabH),
                new Color(0.07f, 0.07f, 0.10f));
            SetAnchors(tabBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -(headerH + 4f + tabH)));

            float tabW = canvasSize.x / tabs.Count;
            for (int i = 0; i < tabs.Count; i++)
            {
                float tabX = -canvasSize.x / 2f + tabW * i + tabW / 2f;
                var tabBtn = CreateButton(tabBar, $"Tab_{i}",
                    tabs[i].tabName,
                    new Vector2(tabX, 0),
                    new Vector2(tabW - 4f, tabH - 8f),
                    i == 0 ? colorTabActive : colorTabInactive,
                    baseFontSize - 4);
                tabButtons.Add(tabBtn.GetComponent<Button>());
            }
        }

        // Content Panels
        for (int i = 0; i < Mathf.Max(1, tabs.Count); i++)
        {
            var tabType = tabs.Count > 0 ? tabs[i].type : ScreenTabType.Custom;
            string tabName = tabs.Count > 0 ? tabs[i].tabName : "Content";

            var content = CreatePanel(root, $"Content_{tabName}",
                new Vector2(0, -(headerH + 4f + tabH) / 2f),
                new Vector2(canvasSize.x, contentH),
                Color.clear);
            SetAnchors(content, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -(headerH + 4f + tabH)));
            content.GetComponent<Image>().color = Color.clear;

            // Nur erster Tab sichtbar
            content.SetActive(i == 0);
            contentPanels.Add(content);

            // Tab-spezifischen Inhalt generieren
            switch (tabType)
            {
                case ScreenTabType.ReactorStatus:
                    BuildReactorStatusTab(content, contentH);
                    break;
                case ScreenTabType.PowerDistribution:
                    BuildPowerDistributionTab(content, contentH);
                    break;
                default:
                    CreateLabel(content, "Placeholder",
                        $"[{tabName}]", Vector2.zero,
                        new Vector2(400, 60), baseFontSize,
                        colorSubtext, false);
                    break;
            }
        }

        // Tab-Button Logik verdrahten
        if (tabButtons.Count > 1)
        {
            var switcher = root.AddComponent<TabSwitcher>();
            switcher.tabButtons   = tabButtons;
            switcher.contentPanels = contentPanels;
            switcher.activeColor   = colorTabActive;
            switcher.inactiveColor = colorTabInactive;
            switcher.Init();
        }

        Debug.Log($"Screen '{screenTitle}' mit {Mathf.Max(1, tabs.Count)} Tab(s) generiert.");
        EditorUtility.SetDirty(gameObject);
    }

    // ── Tab-Inhalte ──────────────────────────────────────────────────────────

    void BuildReactorStatusTab(GameObject parent, float height)
    {
        float pad = 40f;
        float w   = canvasSize.x - pad * 2f;

        // Integrität
        CreateLabel(parent, "IntegrityLabel", "INTEGRITÄT",
            new Vector2(0, height / 2f - pad - 30f),
            new Vector2(w, 40f), baseFontSize - 2, colorSubtext, false,
            TextAlignmentOptions.MidlineLeft);

        var integrityText = CreateLabel(parent, "IntegrityValue", "100%",
            new Vector2(0, height / 2f - pad - 80f),
            new Vector2(w, 60f), baseFontSize + 14, colorText, true,
            TextAlignmentOptions.MidlineLeft);

        // Status
        CreateLabel(parent, "StatusLabel", "STATUS",
            new Vector2(0, height / 2f - pad - 160f),
            new Vector2(w, 40f), baseFontSize - 2, colorSubtext, false,
            TextAlignmentOptions.MidlineLeft);

        var statusText = CreateLabel(parent, "StatusValue", "● NORMAL",
            new Vector2(0, height / 2f - pad - 210f),
            new Vector2(w, 50f), baseFontSize, colorText, false,
            TextAlignmentOptions.MidlineLeft);

        // Lebenserhaltung Timer
        var lifeSupportText = CreateLabel(parent, "LifeSupportText", "",
            new Vector2(0, height / 2f - pad - 270f),
            new Vector2(w, 50f), baseFontSize, Color.red, true,
            TextAlignmentOptions.MidlineLeft);
        lifeSupportText.gameObject.SetActive(false);

        // Repair Button
        var repairBtn = CreateButton(parent, "RepairButton",
            "Reaktor reparieren  (+25%)",
            new Vector2(0, -(height / 2f - pad - 60f)),
            new Vector2(w, 70f),
            new Color(0.15f, 0.45f, 0.25f),
            baseFontSize - 2);

        // ReactorIntegrityDisplay zuweisen
        var display = parent.AddComponent<ReactorIntegrityDisplay>();
        display.integrityText   = integrityText;
        display.statusText      = statusText;
        display.lifeSupportText = lifeSupportText;
        display.repairButton    = repairBtn.GetComponent<Button>();

        // ReactorTerminal auf Parent-GameObject finden und Button verdrahten
        var terminal = GetComponent<ReactorTerminal>();
        if (terminal == null) terminal = gameObject.AddComponent<ReactorTerminal>();
        repairBtn.GetComponent<Button>().onClick.AddListener(terminal.StartRepair);
        display.repairButton = repairBtn.GetComponent<Button>();
    }

    void BuildPowerDistributionTab(GameObject parent, float height)
    {
        float pad  = 30f;
        float w    = canvasSize.x - pad * 2f;
        float rowH = (height - pad * 2f - 80f) / 7f; // 6 Systeme + Totals

        string[] systemNames = { "Lebenserhaltung", "Waffen", "Schilde", "Warp", "Werkstatt", "Sensoren" };
        ShipSystem[] systems = {
            ShipSystem.LifeSupport, ShipSystem.Weapons, ShipSystem.Shields,
            ShipSystem.Warp, ShipSystem.Workshop, ShipSystem.Sensors
        };

        var ui = parent.AddComponent<PowerDistributionUI>();
        ui.systemSlots = new System.Collections.Generic.List<PowerDistributionUI.SystemSlotUI>();

        for (int i = 0; i < systems.Length; i++)
        {
            float y = height / 2f - pad - rowH * i - rowH / 2f;

            // Label
            CreateLabel(parent, $"{systems[i]}_Label", systemNames[i],
                new Vector2(-w / 2f + 180f, y),
                new Vector2(280f, rowH - 8f),
                baseFontSize - 6, colorText, false,
                TextAlignmentOptions.MidlineLeft);

            // Status Dot
            var dot = CreatePanel(parent, $"{systems[i]}_Dot",
                new Vector2(-w / 2f + 20f, y),
                new Vector2(20f, 20f), Color.green);

            // Slider
            var slider = CreateSlider(parent, $"{systems[i]}_Slider",
                new Vector2(w * 0.1f, y),
                new Vector2(w * 0.45f, rowH - 16f));
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value    = 20;

            // Value Text
            var valText = CreateLabel(parent, $"{systems[i]}_Value", "20/30",
                new Vector2(w / 2f - 160f, y),
                new Vector2(160f, rowH - 8f),
                baseFontSize - 8, colorSubtext, false,
                TextAlignmentOptions.MidlineRight);

            // Efficiency Text
            var effText = CreateLabel(parent, $"{systems[i]}_Eff", "67%",
                new Vector2(w / 2f - 20f, y),
                new Vector2(100f, rowH - 8f),
                baseFontSize - 8, Color.green, false,
                TextAlignmentOptions.MidlineRight);

            var slot = new PowerDistributionUI.SystemSlotUI
            {
                system         = systems[i],
                slider         = slider,
                valueText      = valText,
                efficiencyText = effText,
                statusIndicator = dot.GetComponent<Image>()
            };
            ui.systemSlots.Add(slot);
        }

        // Totals
        float totalsY = -(height / 2f - pad - 30f);
        var totalText = CreateLabel(parent, "TotalAllocated", "Gesamt: 0",
            new Vector2(-150f, totalsY),
            new Vector2(300f, 50f), baseFontSize - 4, colorText, false);
        var maxText = CreateLabel(parent, "MaxCapacity", "Max: 100",
            new Vector2(150f, totalsY),
            new Vector2(300f, 50f), baseFontSize - 4, colorText, false);

        // Reactor Limit
        float limitY = totalsY - 50f;
        CreateLabel(parent, "LimitLabel", "Reaktor Limit",
            new Vector2(-w / 2f + 180f, limitY),
            new Vector2(280f, 50f), baseFontSize - 6, colorSubtext, false,
            TextAlignmentOptions.MidlineLeft);

        var limitSlider = CreateSlider(parent, "LimitSlider",
            new Vector2(w * 0.1f, limitY),
            new Vector2(w * 0.45f, 40f));
        limitSlider.minValue = 10;
        limitSlider.maxValue = 150;
        limitSlider.value    = 100;

        var limitText = CreateLabel(parent, "LimitText", "Limit: 100%",
            new Vector2(w / 2f - 100f, limitY),
            new Vector2(200f, 50f), baseFontSize - 6, colorText, false,
            TextAlignmentOptions.MidlineRight);

        ui.totalAllocatedText = totalText;
        ui.maxCapacityText    = maxText;
        ui.reactorLimitSlider = limitSlider;
        ui.reactorLimitText   = limitText;
        ui.onlineColor        = Color.green;
        ui.offlineColor       = Color.red;
        ui.overloadColor      = Color.yellow;
    }

    // ── UI Helper Methoden ───────────────────────────────────────────────────

    GameObject CreatePanel(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    TextMeshProUGUI CreateLabel(GameObject parent, string name, string text,
        Vector2 pos, Vector2 size, int fontSize, Color color, bool bold = false,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        Vector2 offset = default)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos + offset;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = alignment;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 pos, Vector2 size, Color color, int fontSize = 24)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var lgo = new GameObject("Label");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;
        var tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }

    Slider CreateSlider(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.20f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.sizeDelta = new Vector2(-10, 0); faRT.anchoredPosition = new Vector2(5, 0);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(0.5f, 1f); fillRT.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = new Color(0.15f, 0.55f, 0.85f);

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one; haRT.sizeDelta = new Vector2(-20, 0);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(24, 0);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = go.AddComponent<Slider>();
        slider.fillRect     = fillRT;
        slider.handleRect   = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction    = Slider.Direction.LeftToRight;
        slider.minValue     = 0f;
        slider.maxValue     = 1f;
        slider.value        = 0.5f;
        return slider;
    }

    void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
    }
    #endif
}
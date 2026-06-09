using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShipUIGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Ship UI")]
    public static void ShowWindow()
    {
        GetWindow<ShipUIGenerator>("Ship UI Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Ship UI Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate All UI", GUILayout.Height(40)))
            GenerateAll();

        GUILayout.Space(5);

        if (GUILayout.Button("Clear Generated UI"))
        {
            GameObject existing = GameObject.Find("GameCanvas");
            if (existing != null) DestroyImmediate(existing);
        }
    }

    void GenerateAll()
    {
        GameObject existing = GameObject.Find("GameCanvas");
        if (existing != null) DestroyImmediate(existing);

        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        GenerateReactorUI(canvasGO);
        GeneratePowerDistributionUI(canvasGO);
        GenerateGameOverUI(canvasGO);

        Debug.Log("Ship UI generiert und zugewiesen.");
        Selection.activeGameObject = canvasGO;
    }

    // ── REACTOR UI ──
    void GenerateReactorUI(GameObject canvas)
    {
        GameObject panel = CreatePanel(canvas, "ReactorUI_Panel",
            new Vector2(-650, 150), new Vector2(350, 360), new Color(0.1f, 0.1f, 0.1f, 0.85f));

        ReactorUI ui = panel.AddComponent<ReactorUI>();

        CreateLabel(panel, "Title", "REAKTOR", new Vector2(0, 155), 18, Color.white, true);

        // Integrity
        CreateLabel(panel, "IntegrityLabel", "Integrität", new Vector2(-80, 115), 13, Color.white);
        ui.integrityText = CreateLabel(panel, "IntegrityText", "100%", new Vector2(100, 115), 13, Color.green);
        ui.integritySlider = CreateSlider(panel, "IntegritySlider", new Vector2(0, 90), new Vector2(300, 20), false);
        ui.integritySlider.interactable = false;

        // Load
        CreateLabel(panel, "LoadLabel", "Auslastung", new Vector2(-80, 55), 13, Color.white);
        ui.loadText = CreateLabel(panel, "LoadText", "0%", new Vector2(100, 55), 13, Color.green);
        ui.loadSlider = CreateSlider(panel, "LoadSlider", new Vector2(0, 30), new Vector2(300, 20), false);
        ui.loadSlider.interactable = false;

        // Overload Timer
        ui.overloadTimerText = CreateLabel(panel, "OverloadTimerText", "", new Vector2(0, -10), 13, Color.yellow);
        ui.overloadTimerText.rectTransform.sizeDelta = new Vector2(320, 30);

        // Life Support Warning
        GameObject lsWarning = CreatePanel(panel, "LifeSupportWarning",
            new Vector2(0, -60), new Vector2(320, 45), new Color(0.8f, 0.1f, 0.1f, 0.9f));
        lsWarning.SetActive(false);
        ui.lifeSupportWarningPanel = lsWarning;
        ui.lifeSupportTimerText = CreateLabel(lsWarning, "LifeSupportText", "SAUERSTOFF: 120s",
            new Vector2(0, 0), 14, Color.white, true);

        // Overload Warning Image
        GameObject owGO = new GameObject("OverloadWarning");
        owGO.transform.SetParent(panel.transform, false);
        RectTransform owRT = owGO.AddComponent<RectTransform>();
        owRT.anchoredPosition = new Vector2(0, -115);
        owRT.sizeDelta = new Vector2(320, 30);
        Image owImg = owGO.AddComponent<Image>();
        owImg.color = new Color(1f, 0.5f, 0f, 0.8f);
        owGO.SetActive(false);
        ui.overloadWarning = owImg;

        // Dev Repair Button
        GameObject repairBtn = CreateButton(panel, "DEV_RepairButton",
            "DEV: Reparieren (+10)", new Vector2(0, -160), new Vector2(220, 35),
            new Color(0.2f, 0.6f, 0.2f));
        repairBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (ReactorSystem.Instance != null)
                ReactorSystem.Instance.Repair(10f);
        });

        // Colors
        ui.normalColor = Color.green;
        ui.warningColor = Color.yellow;
        ui.criticalColor = Color.red;
    }

    // ── POWER DISTRIBUTION UI ──
    void GeneratePowerDistributionUI(GameObject canvas)
    {
        ShipSystem[] systems = new ShipSystem[]
        {
            ShipSystem.LifeSupport,
            ShipSystem.Weapons,
            ShipSystem.Shields,
            ShipSystem.Warp,
            ShipSystem.Workshop,
            ShipSystem.Sensors
        };

        string[] labels = new string[]
        {
            "Lebenserhaltung", "Waffen", "Schilde", "Warp", "Werkstatt", "Sensoren"
        };

        float panelHeight = 100f + systems.Length * 55f;

        GameObject panel = CreatePanel(canvas, "PowerDistribution_Panel",
            new Vector2(650, 0), new Vector2(400, panelHeight), new Color(0.1f, 0.1f, 0.1f, 0.85f));

        PowerDistributionUI ui = panel.AddComponent<PowerDistributionUI>();
        ui.systemSlots = new List<PowerDistributionUI.SystemSlotUI>();

        CreateLabel(panel, "Title", "STROMVERTEILUNG", new Vector2(0, panelHeight / 2 - 25), 18, Color.white, true);

        float startY = panelHeight / 2 - 65f;

        for (int i = 0; i < systems.Length; i++)
        {
            float y = startY - i * 55f;
            var slot = new PowerDistributionUI.SystemSlotUI();
            slot.system = systems[i];

            // Status Indicator
            GameObject indicatorGO = new GameObject($"{systems[i]}_Indicator");
            indicatorGO.transform.SetParent(panel.transform, false);
            RectTransform indRT = indicatorGO.AddComponent<RectTransform>();
            indRT.anchoredPosition = new Vector2(-170, y);
            indRT.sizeDelta = new Vector2(12, 12);
            slot.statusIndicator = indicatorGO.AddComponent<Image>();
            slot.statusIndicator.color = Color.green;

            // Label
            CreateLabel(panel, $"{systems[i]}_Label", labels[i], new Vector2(-80, y), 12, Color.white);

            // Slider
            slot.slider = CreateSlider(panel, $"{systems[i]}_Slider",
                new Vector2(60, y), new Vector2(180, 18), true);
            slot.slider.minValue = 0;
            slot.slider.maxValue = 100;
            slot.slider.value = 20;

            // Value + Efficiency
            slot.valueText = CreateLabel(panel, $"{systems[i]}_Value", "0/0",
                new Vector2(170, y + 8), 11, Color.white);
            slot.efficiencyText = CreateLabel(panel, $"{systems[i]}_Efficiency", "100%",
                new Vector2(170, y - 8), 11, Color.green);

            ui.systemSlots.Add(slot);
        }

        // Totals
        float bottomY = startY - systems.Length * 55f;
        ui.totalAllocatedText = CreateLabel(panel, "TotalAllocated", "Gesamt: 0",
            new Vector2(-70, bottomY), 13, Color.white);
        ui.maxCapacityText = CreateLabel(panel, "MaxCapacity", "Max: 100",
            new Vector2(70, bottomY), 13, Color.green);

        // Reactor Limit
        CreateLabel(panel, "LimitLabel", "Reaktor Limit",
            new Vector2(-100, bottomY - 35), 12, Color.white);
        ui.reactorLimitSlider = CreateSlider(panel, "ReactorLimitSlider",
            new Vector2(40, bottomY - 35), new Vector2(180, 18), true);
        ui.reactorLimitSlider.minValue = 10;
        ui.reactorLimitSlider.maxValue = 150;
        ui.reactorLimitSlider.value = 100;
        ui.reactorLimitText = CreateLabel(panel, "ReactorLimitText", "Limit: 100%",
            new Vector2(160, bottomY - 35), 12, Color.white);

        ui.onlineColor = Color.green;
        ui.offlineColor = Color.red;
        ui.overloadColor = Color.yellow;
    }

    // ── GAME OVER UI ──
    void GenerateGameOverUI(GameObject canvas)
    {
        GameObject panel = CreatePanel(canvas, "GameOver_Panel",
            new Vector2(0, 0), new Vector2(500, 350), new Color(0.05f, 0.05f, 0.05f, 0.95f));
        panel.SetActive(false);

        GameOverUI ui = panel.AddComponent<GameOverUI>();
        ui.panel = panel;

        ui.titleText = CreateLabel(panel, "Title", "SCHIFF VERLOREN",
            new Vector2(0, 120), 36, Color.red, true);
        ui.titleText.rectTransform.sizeDelta = new Vector2(460, 50);

        ui.reasonText = CreateLabel(panel, "Reason", "",
            new Vector2(0, 60), 20, Color.white);
        ui.reasonText.rectTransform.sizeDelta = new Vector2(460, 40);

        CreateLabel(panel, "Subtitle", "Das Schiff und seine Crew sind verloren.",
            new Vector2(0, 10), 14, new Color(0.7f, 0.7f, 0.7f));

        // Restart Button
        GameObject restartBtn = CreateButton(panel, "RestartButton",
            "Neu starten", new Vector2(0, -60), new Vector2(200, 45),
            new Color(0.2f, 0.4f, 0.8f));
        restartBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            GameOverUI.Instance?.Hide();
        });

        // Dev: Trigger Game Over Button (außerhalb des Panels, immer sichtbar)
        GameObject devBtn = CreateButton(canvas, "DEV_GameOverButton",
            "DEV: Game Over", new Vector2(0, -480), new Vector2(200, 35),
            new Color(0.6f, 0.1f, 0.1f));
        devBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            GameManager.Instance?.TriggerGameOver("DEV: Manuell ausgelöst");
        });
    }

    // ── HELPER METHODEN ──

    GameObject CreatePanel(GameObject parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    TextMeshProUGUI CreateLabel(GameObject parent, string name, string text,
        Vector2 position, int fontSize, Color color, bool bold = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200, 30);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    Slider CreateSlider(GameObject parent, string name, Vector2 position, Vector2 size, bool interactable)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.sizeDelta = new Vector2(-10, 0);
        faRT.anchoredPosition = new Vector2(5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0.5f, 1f);
        fillRT.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;

        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        RectTransform haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        // Slider
        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        slider.interactable = interactable;

        return slider;
    }

    GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = color;
        Button btn = go.AddComponent<Button>();

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        btn.targetGraphic = img;

        return go;
    }
}
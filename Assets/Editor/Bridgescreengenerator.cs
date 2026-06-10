using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BridgeScreenGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Bridge Screen UI")]
    public static void ShowWindow()
        => GetWindow<BridgeScreenGenerator>("Bridge Screen Generator");

    [Header("Settings")]
    float canvasScale = 1.496129e-05f;
    Vector2 canvasSize = new Vector2(1108, 876);

    void OnGUI()
    {
        GUILayout.Label("Bridge Screen UI Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("Canvas Scale:");
        canvasScale = EditorGUILayout.FloatField(canvasScale);
        GUILayout.Label("Canvas Size:");
        canvasSize = EditorGUILayout.Vector2Field("", canvasSize);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate", GUILayout.Height(40))) Generate();
        if (GUILayout.Button("Clear"))
        {
            var ex = GameObject.Find("BridgeScreenCanvas");
            if (ex != null) DestroyImmediate(ex);
        }
    }

    void Generate()
    {
        // Brücken-Monitor GameObject finden
        var bridge = Selection.activeGameObject;
        if (bridge == null)
        {
            Debug.LogError("Wähle das Brücken-Monitor GameObject aus bevor du generierst.");
            return;
        }

        // Alten Canvas löschen
        var existingCanvas = bridge.GetComponentInChildren<Canvas>();
        if (existingCanvas != null) DestroyImmediate(existingCanvas.gameObject);

        // Canvas erstellen
        var canvasGO = new GameObject("BridgeScreenCanvas");
        canvasGO.transform.SetParent(bridge.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta     = canvasSize;
        rt.localScale    = Vector3.one * canvasScale;
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<WorldSpaceCanvasSetup>();

        // Root Panel
        var root = MakePanel(canvasGO, "Root", Vector2.zero, canvasSize,
            new Color(0.05f, 0.06f, 0.10f, 0.98f));
        SetFullStretch(root);

        // Header
        float headerH = 80f;
        var header = MakePanel(root, "Header",
            new Vector2(0, canvasSize.y / 2f - headerH / 2f),
            new Vector2(canvasSize.x, headerH),
            new Color(0.07f, 0.09f, 0.15f));
        MakeLabel(header, "Title", "BRÜCKE – NAVIGATIONSSYSTEM",
            Vector2.zero, new Vector2(canvasSize.x, headerH),
            20, new Color(0.4f, 0.7f, 1f), true);

        // Accent Line
        MakePanel(root, "AccentLine",
            new Vector2(0, canvasSize.y / 2f - headerH - 2f),
            new Vector2(canvasSize.x, 3f),
            new Color(0.4f, 0.7f, 1f));

        float contentY = canvasSize.y / 2f - headerH - 3f;
        float contentH = canvasSize.y - headerH - 3f;

        // ── Normal Panel ──────────────────────────────────────────────────────
        var normalPanel = MakePanel(root, "NormalPanel",
            new Vector2(0, -headerH / 2f - 1.5f),
            new Vector2(canvasSize.x, contentH),
            Color.clear);
        normalPanel.GetComponent<Image>().color = Color.clear;

        float pad = 60f;
        float w   = canvasSize.x - pad * 2f;

        // Autopilot Status
        MakeLabel(normalPanel, "AutopilotLabel", "AUTOPILOT",
            new Vector2(-w / 4f, contentH / 2f - 80f),
            new Vector2(300f, 40f), 16,
            new Color(0.6f, 0.6f, 0.6f));

        var autopilotStatus = MakeLabel(normalPanel, "AutopilotStatus", "● AKTIV",
            new Vector2(-w / 4f, contentH / 2f - 130f),
            new Vector2(300f, 50f), 24, Color.green, true);

        // Kurs
        MakeLabel(normalPanel, "CourseLabel", "AKTUELLER KURS",
            new Vector2(w / 4f, contentH / 2f - 80f),
            new Vector2(300f, 40f), 16,
            new Color(0.6f, 0.6f, 0.6f));

        var courseText = MakeLabel(normalPanel, "CourseText", "270° – Station Alpha",
            new Vector2(w / 4f, contentH / 2f - 130f),
            new Vector2(350f, 50f), 18, Color.white);

        // Geschwindigkeit
        MakeLabel(normalPanel, "SpeedLabel", "WARP STATUS",
            new Vector2(-w / 4f, contentH / 2f - 230f),
            new Vector2(300f, 40f), 16,
            new Color(0.6f, 0.6f, 0.6f));
        MakeLabel(normalPanel, "SpeedValue", "WARP AKTIV",
            new Vector2(-w / 4f, contentH / 2f - 280f),
            new Vector2(300f, 50f), 20, new Color(0.4f, 0.8f, 1f));

        // Ankunft
        MakeLabel(normalPanel, "ETALabel", "GESCHÄTZTE ANKUNFT",
            new Vector2(w / 4f, contentH / 2f - 230f),
            new Vector2(350f, 40f), 16,
            new Color(0.6f, 0.6f, 0.6f));
        MakeLabel(normalPanel, "ETAValue", "00:42",
            new Vector2(w / 4f, contentH / 2f - 280f),
            new Vector2(350f, 50f), 28, Color.white, true);

        // Trennlinie
        MakePanel(normalPanel, "Divider",
            new Vector2(0, contentH / 2f - 340f),
            new Vector2(w, 2f),
            new Color(0.2f, 0.2f, 0.3f));

        // Crew Status
        MakeLabel(normalPanel, "CrewLabel", "CREW AN BORD",
            new Vector2(0, contentH / 2f - 390f),
            new Vector2(400f, 40f), 16,
            new Color(0.6f, 0.6f, 0.6f));
        MakeLabel(normalPanel, "CrewValue", "4 / 6",
            new Vector2(0, contentH / 2f - 440f),
            new Vector2(400f, 50f), 28, Color.white, true);

        // ── Alert Panel ───────────────────────────────────────────────────────
        var alertPanel = MakePanel(root, "AlertPanel",
            new Vector2(0, -headerH / 2f - 1.5f),
            new Vector2(canvasSize.x, contentH),
            Color.clear);
        alertPanel.GetComponent<Image>().color = Color.clear;
        alertPanel.SetActive(false);

        // Roter Alarm-Hintergrund
        var alertBg = MakePanel(alertPanel, "AlertBg",
            new Vector2(0, 0),
            new Vector2(canvasSize.x, contentH),
            new Color(0.15f, 0.02f, 0.02f, 0.5f));

        // Alarm-Text
        MakeLabel(alertPanel, "AlertTitle", "⚠ AUTOPILOT AUSFALL",
            new Vector2(0, contentH / 2f - 120f),
            new Vector2(800f, 60f), 32,
            new Color(1f, 0.3f, 0.3f), true);

        MakeLabel(alertPanel, "AlertDesc", "Manuelle Neustartsequenz erforderlich\nZugang zur Brücke und Code-Eingabe notwendig",
            new Vector2(0, contentH / 2f - 200f),
            new Vector2(700f, 80f), 16,
            new Color(0.9f, 0.9f, 0.9f));

        // Pulsierender Alert-Button
        var alertBtn = MakeButton(alertPanel, "AlertButton",
            "⚠  NEUSTARTSEQUENZ EINLEITEN  ⚠",
            new Vector2(0, 0),
            new Vector2(600f, 100f),
            new Color(0.8f, 0.1f, 0.1f));

        // BridgeScreen Script auf Monitor-Objekt
        var bridgeScreen = bridge.GetComponent<BridgeScreen>();
        if (bridgeScreen == null) bridgeScreen = bridge.AddComponent<BridgeScreen>();

        bridgeScreen.normalPanel         = normalPanel;
        bridgeScreen.alertPanel          = alertPanel;
        bridgeScreen.alertButton         = alertBtn.GetComponent<Button>();
        bridgeScreen.autopilotStatusText = autopilotStatus;
        bridgeScreen.courseText          = courseText;

        alertBtn.GetComponent<Button>().onClick.AddListener(bridgeScreen.OnAlertButtonClicked);

        Debug.Log("Bridge Screen UI generiert.");
        Selection.activeGameObject = bridge;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetFullStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    GameObject MakePanel(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    TextMeshProUGUI MakeLabel(GameObject parent, string name, string text,
        Vector2 pos, Vector2 size, int fs, Color color, bool bold = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fs; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    GameObject MakeButton(GameObject parent, string name, string label,
        Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = color;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;

        var lgo = new GameObject("L");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;
        var tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AutopilotMinigameSetup : EditorWindow
{
    [MenuItem("Tools/Generate Autopilot Minigame")]
    public static void ShowWindow()
        => GetWindow<AutopilotMinigameSetup>("Autopilot Minigame");

    void OnGUI()
    {
        GUILayout.Label("Autopilot Minigame Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (GUILayout.Button("Generate", GUILayout.Height(40))) Generate();
        if (GUILayout.Button("Clear"))
        {
            var ex = GameObject.Find("AutopilotMinigame");
            if (ex != null) DestroyImmediate(ex);
        }
    }

    void Generate()
    {
        var existing = GameObject.Find("AutopilotMinigame");
        if (existing != null) DestroyImmediate(existing);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("Kein Canvas gefunden"); return; }

        // Root Panel (Overlay)
        var root = MakePanel(canvas.gameObject, "AutopilotMinigame",
            Vector2.zero, new Vector2(600, 500),
            new Color(0.04f, 0.04f, 0.07f, 0.97f));
        root.SetActive(false);

        var mg = root.AddComponent<AutopilotMinigame>();
        mg.minigamePanel = root;

        // Titel
        MakeLabel(root, "Title", "AUTOPILOT AUSFALL",
            new Vector2(0, 210), new Vector2(560, 44), 24,
            new Color(1f, 0.3f, 0.3f), true);

        MakeLabel(root, "Sub", "Manuelle Neustartsequenz erforderlich",
            new Vector2(0, 170), new Vector2(560, 28), 13,
            new Color(0.7f, 0.7f, 0.7f));

        // Status Text
        var statusTMP = MakeLabel(root, "Status", "Merke dir den Code!",
            new Vector2(0, 135), new Vector2(560, 30), 14, Color.yellow);
        mg.statusText = statusTMP;

        // ── Show Panel (Zahlen anzeigen) ──────────────────────────────────────
        var showPanel = MakePanel(root, "ShowPanel",
            new Vector2(0, 30), new Vector2(500, 120),
            new Color(0.08f, 0.08f, 0.12f));

        var displayDigits = new TextMeshProUGUI[4];
        float[] xPositions = { -150f, -50f, 50f, 150f };
        for (int i = 0; i < 4; i++)
        {
            var digitGO = new GameObject($"Digit_{i}");
            digitGO.transform.SetParent(showPanel.transform, false);
            var drt = digitGO.AddComponent<RectTransform>();
            drt.anchoredPosition = new Vector2(xPositions[i], 0);
            drt.sizeDelta        = new Vector2(80, 100);
            var dtmp = digitGO.AddComponent<TextMeshProUGUI>();
            dtmp.text      = "?";
            dtmp.fontSize  = 72;
            dtmp.color     = new Color(0.2f, 0.8f, 1f);
            dtmp.alignment = TextAlignmentOptions.Center;
            dtmp.fontStyle = FontStyles.Bold;
            displayDigits[i] = dtmp;
        }
        mg.displayDigits = displayDigits;
        mg.showPanel     = showPanel;

        // ── Input Panel (Eingabefelder) ───────────────────────────────────────
        var inputPanel = MakePanel(root, "InputPanel",
            new Vector2(0, 30), new Vector2(500, 120),
            new Color(0.08f, 0.08f, 0.12f));
        inputPanel.SetActive(false);

        var inputFields = new TMP_InputField[4];
        for (int i = 0; i < 4; i++)
        {
            var fieldGO = new GameObject($"InputField_{i}");
            fieldGO.transform.SetParent(inputPanel.transform, false);
            var frt = fieldGO.AddComponent<RectTransform>();
            frt.anchoredPosition = new Vector2(xPositions[i], 0);
            frt.sizeDelta        = new Vector2(80, 90);

            var fbg = fieldGO.AddComponent<Image>();
            fbg.color = new Color(0.12f, 0.12f, 0.18f);

            var field = fieldGO.AddComponent<TMP_InputField>();

            // Text Area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(fieldGO.transform, false);
            var taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
            taRT.sizeDelta = new Vector2(-10, -10);
            var rectMask = textArea.AddComponent<RectMask2D>();

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform, false);
            var trt = textGO.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            var ttmp = textGO.AddComponent<TextMeshProUGUI>();
            ttmp.fontSize  = 56;
            ttmp.color     = Color.white;
            ttmp.alignment = TextAlignmentOptions.Center;
            ttmp.fontStyle = FontStyles.Bold;

            // Placeholder
            var phGO = new GameObject("Placeholder");
            phGO.transform.SetParent(textArea.transform, false);
            var prt = phGO.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.sizeDelta = Vector2.zero;
            var ptmp = phGO.AddComponent<TextMeshProUGUI>();
            ptmp.text      = "_";
            ptmp.fontSize  = 56;
            ptmp.color     = new Color(0.4f, 0.4f, 0.4f);
            ptmp.alignment = TextAlignmentOptions.Center;

            field.textComponent   = ttmp;
            field.placeholder     = ptmp;
            field.textViewport    = taRT;
            field.contentType     = TMP_InputField.ContentType.IntegerNumber;
            field.characterLimit  = 1;
            field.caretColor      = Color.white;

            inputFields[i] = field;
        }
        mg.inputFields = inputFields;
        mg.inputPanel  = inputPanel;

        // Timer
        var timerTMP = MakeLabel(root, "Timer", "15.0s",
            new Vector2(0, -75), new Vector2(200, 44), 32, Color.white, true);
        mg.timerText = timerTMP;

        // Submit Button
        var submitBtn = MakeButton(root, "SubmitBtn", "BESTÄTIGEN",
            new Vector2(0, -140), new Vector2(240, 50),
            new Color(0.15f, 0.45f, 0.75f));
        submitBtn.GetComponent<Button>().onClick.AddListener(() =>
            FindObjectOfType<AutopilotMinigame>()?.Submit());
        mg.submitButton = submitBtn.GetComponent<Button>();

        // Schließen Button
        var closeBtn = MakeButton(root, "CloseBtn", "Abbrechen",
            new Vector2(0, -200), new Vector2(180, 38),
            new Color(0.35f, 0.1f, 0.1f));
        closeBtn.GetComponent<Button>().onClick.AddListener(() =>
            FindObjectOfType<AutopilotMinigame>()?.CloseMinigame());

        Debug.Log("Autopilot Minigame generiert.");
        Selection.activeGameObject = root;
    }

    GameObject MakePanel(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    TextMeshProUGUI MakeLabel(GameObject parent, string name, string text,
        Vector2 pos, Vector2 size, int fs, Color color, bool bold = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
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
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = color;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;

        var lgo = new GameObject("L");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
        var tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 15; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
        return go;
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class EventDevPanelSetup : EditorWindow
{
    [MenuItem("Tools/Generate Event Dev Panel")]
    public static void ShowWindow()
        => GetWindow<EventDevPanelSetup>("Event Dev Panel");

    void OnGUI()
    {
        GUILayout.Label("Event Dev Panel Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (GUILayout.Button("Generate", GUILayout.Height(40))) Generate();
        if (GUILayout.Button("Clear"))
        {
            var ex = GameObject.Find("EventDevPanel");
            if (ex != null) DestroyImmediate(ex);
            var ex2 = GameObject.Find("EventSystemGO");
            if (ex2 != null) DestroyImmediate(ex2);
        }
    }

    void Generate()
    {
        // EventSystem GameObject
        var existing = GameObject.Find("EventSystemGO");
        if (existing == null)
        {
            var esGO = new GameObject("EventSystemGO");
            esGO.AddComponent<EventSystem>();
            var ni = esGO.AddComponent<Mirror.NetworkIdentity>();
            Debug.Log("EventSystem GameObject erstellt");
        }

        // Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Kein Canvas gefunden");
            return;
        }

        var exPanel = GameObject.Find("EventDevPanel");
        if (exPanel != null) DestroyImmediate(exPanel);

        // Root
        var root = MakePanel(canvas.gameObject, "EventDevPanel",
            new Vector2(-600, -300), new Vector2(320, 420),
            new Color(0.05f, 0.05f, 0.08f, 0.95f));

        var devPanel = root.AddComponent<EventDevPanel>();
        devPanel.panel = root;

        // Titel
        MakeLabel(root, "Title", "DEV – EVENTS",
            new Vector2(0, 180), new Vector2(300, 36), 16, Color.cyan, true);

        // Status
        var status = MakeLabel(root, "Status", "Kein Event aktiv",
            new Vector2(0, 145), new Vector2(300, 28), 11,
            new Color(0.7f, 0.7f, 0.7f));
        devPanel.statusText = status;

        // Separator
        MakePanel(root, "Sep", new Vector2(0, 122), new Vector2(280, 2),
            new Color(0.3f, 0.3f, 0.3f));

        // Event Buttons
        (string label, string method, Color color)[] buttons =
        {
            ("Autopilot Ausfall",  "TriggerAutopilotFailure", new Color(0.8f, 0.1f, 0.1f)),
            ("Piratenangriff",     "TriggerPirateAttack",     new Color(0.7f, 0.2f, 0.0f)),
            ("Asteroidenfeld",     "TriggerAsteroidField",    new Color(0.5f, 0.4f, 0.1f)),
            ("Feuer an Bord",      "TriggerFireOnBoard",      new Color(0.8f, 0.3f, 0.0f)),
            ("Stromausfall",       "TriggerPowerOutage",      new Color(0.3f, 0.2f, 0.5f)),
            ("Notsignal",          "TriggerDistressSignal",   new Color(0.1f, 0.4f, 0.6f)),
        };

        float startY = 90f;
        for (int i = 0; i < buttons.Length; i++)
        {
            var (label, method, color) = buttons[i];
            float y = startY - i * 44f;
            var btn = MakeButton(root, $"Btn_{i}", label,
                new Vector2(0, y), new Vector2(280, 36), color);

            var devPanelRef = devPanel;
            var methodName  = method;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                var dp = FindObjectOfType<EventDevPanel>();
                if (dp != null)
                    dp.GetType().GetMethod(methodName)?.Invoke(dp, null);
            });
        }

        // Resolve Button
        var resolveBtn = MakeButton(root, "ResolveBtn", "✓ Event beenden",
            new Vector2(0, startY - buttons.Length * 44f - 10f),
            new Vector2(280, 36), new Color(0.1f, 0.5f, 0.2f));
        resolveBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            var dp = FindObjectOfType<EventDevPanel>();
            dp?.ResolveCurrentEvent();
        });

        // Toggle Hint
        MakeLabel(root, "Hint", "F1 zum Ein/Ausblenden",
            new Vector2(0, -185), new Vector2(300, 24), 10,
            new Color(0.5f, 0.5f, 0.5f));

        // Standardmäßig ausgeblendet
        root.SetActive(false);

        Debug.Log("Event Dev Panel generiert. F1 zum Öffnen.");
        Selection.activeGameObject = root;
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
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = color;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;

        var lgo = new GameObject("L");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
        var tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 13; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
        return go;
    }
}
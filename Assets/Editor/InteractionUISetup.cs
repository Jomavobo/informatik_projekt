using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class InteractionUISetup : EditorWindow
{
    [MenuItem("Tools/Generate Interaction UI")]
    public static void ShowWindow()
    {
        GetWindow<InteractionUISetup>("Interaction UI");
    }

    void OnGUI()
    {
        GUILayout.Label("Interaction UI Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate", GUILayout.Height(40)))
            Generate();

        if (GUILayout.Button("Clear"))
        {
            var existing = GameObject.Find("InteractionUI");
            if (existing != null) DestroyImmediate(existing);
        }
    }

    void Generate()
    {
        var existing = GameObject.Find("InteractionUI");
        if (existing != null) DestroyImmediate(existing);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Kein Canvas gefunden.");
            return;
        }

        var root = new GameObject("InteractionUI");
        root.transform.SetParent(canvas.transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;

        var ui = root.AddComponent<InteractionUI>();

        // ── Crosshair Dot ──
        var dot = new GameObject("CrosshairDot");
        dot.transform.SetParent(root.transform, false);
        var dotRT = dot.AddComponent<RectTransform>();
        dotRT.anchoredPosition = Vector2.zero;
        dotRT.sizeDelta = new Vector2(6, 6);
        var dotImg = dot.AddComponent<Image>();
        dotImg.color = new Color(1f, 1f, 1f, 0.8f);
        ui.crosshairDot = dotImg;

        // ── Hold Ring ──
        var ring = new GameObject("HoldRing");
        ring.transform.SetParent(root.transform, false);
        var ringRT = ring.AddComponent<RectTransform>();
        ringRT.anchoredPosition = Vector2.zero;
        ringRT.sizeDelta = new Vector2(48, 48);
        var ringImg = ring.AddComponent<Image>();
        ringImg.color = new Color(1f, 1f, 1f, 0.9f);
        ringImg.type = Image.Type.Filled;
        ringImg.fillMethod = Image.FillMethod.Radial360;
        ringImg.fillOrigin = (int)Image.Origin360.Top;
        ringImg.fillClockwise = true;
        ringImg.fillAmount = 0f;
        ringImg.gameObject.SetActive(false);
        ui.ringImage = ringImg;

        // ── Prompt Text ──
        var prompt = new GameObject("PromptText");
        prompt.transform.SetParent(root.transform, false);
        var promptRT = prompt.AddComponent<RectTransform>();
        promptRT.anchoredPosition = new Vector2(0, -60);
        promptRT.sizeDelta = new Vector2(400, 36);
        var promptTMP = prompt.AddComponent<TextMeshProUGUI>();
        promptTMP.text = "";
        promptTMP.fontSize = 16;
        promptTMP.color = Color.white;
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.fontStyle = FontStyles.Bold;
        prompt.SetActive(false);
        ui.promptText = promptTMP;

        // Farben
        ui.promptActive  = Color.white;
        ui.promptBlocked = new Color(1f, 0.4f, 0.4f);
        ui.ringColor     = Color.white;

        Debug.Log("Interaction UI generiert.");
        Selection.activeGameObject = root;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipePuzzleSetup : MonoBehaviour
{
    [Header("Settings")]
    public int   gridSize  = 7;
    public float timeLimit = 90f;
    public KeyCode openKey = KeyCode.P;

    PipePuzzle    puzzle;
    PipePuzzleUI  ui;

    void Start()
    {
        BuildUI();
        WireEvents();
        Debug.Log($"PipePuzzleSetup bereit. '{openKey}' drücken zum Öffnen.");
    }

    void Update()
    {
        if (Input.GetKeyDown(openKey)) Open();
    }

public void Open()
{
    puzzle.StartPuzzle();
    ui.Show();
    
    // Cursor freigeben
    var player = FindLocalPlayer();
    player?.GetComponent<PlayerMovement>()?.LockCursor(false);
}

    // ── UI komplett zur Laufzeit bauen ───────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        // Puzzle Logic
        var logicGO = new GameObject("PipePuzzleLogic");
        puzzle = logicGO.AddComponent<PipePuzzle>();
        puzzle.gridSize  = gridSize;
        puzzle.timeLimit = timeLimit;

        // Dunkles Overlay (ganzer Bildschirm)
        var overlayGO = new GameObject("PipePuzzleOverlay");
        overlayGO.transform.SetParent(canvas.transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        overlayGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

        // Haupt-Panel
        var panel = MakePanel(overlayGO, "PuzzlePanel", Vector2.zero,
            new Vector2(560, 660), new Color(0.07f, 0.07f, 0.11f, 0.98f));

        // Titel
        MakeLabel(panel, "Title", "REAKTOR REPARATUR",
            new Vector2(0, 295), new Vector2(520, 42), 22, Color.white, true);

        // Untertitel
        MakeLabel(panel, "Sub", "Verbinde Eingang (blau) mit Ausgang (orange) — klicke Kacheln zum Drehen",
            new Vector2(0, 258), new Vector2(520, 26), 11, new Color(0.65f, 0.65f, 0.65f));

        // Timer
        var timerTMP = MakeLabel(panel, "Timer", "60.0s",
            new Vector2(0, 218), new Vector2(180, 46), 32, Color.white, true);

        // Status
        var statusTMP = MakeLabel(panel, "Status", "",
            new Vector2(0, 178), new Vector2(520, 28), 13, Color.white);

        // Grid Container
        var gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(panel.transform, false);
        var gridRT = gridGO.AddComponent<RectTransform>();
        gridRT.anchoredPosition = new Vector2(0, -25f);
        gridRT.sizeDelta = new Vector2(420, 420);

        // Schließen Button
        var closeGO = MakeButton(panel, "CloseBtn", "Schließen",
            new Vector2(0, -300), new Vector2(180, 42),
            new Color(0.45f, 0.12f, 0.12f));
        closeGO.GetComponent<Button>().onClick.AddListener(() => puzzle.Close());

        // PipePuzzleUI Script auf Overlay
        ui = overlayGO.AddComponent<PipePuzzleUI>();
        ui.overlayPanel  = overlayGO;
        ui.gridContainer = gridRT;
        ui.timerText     = timerTMP;
        ui.statusText    = statusTMP;
        ui.tileSize      = 74f;
        ui.tileSpacing   = 5f;
        ui.pipeWidth     = 16f;

        overlayGO.SetActive(false);
    }

    void WireEvents()
    {
        puzzle.OnSolved += () =>
        {
            ui.ShowSolved();
            ReactorSystem.Instance?.CmdRepair(20f);
            Invoke(nameof(ClosePuzzle), 2f);
        };

        puzzle.OnFailed += () =>
        {
            if (ui.statusText != null)
            {
                ui.statusText.text  = "ZEIT ABGELAUFEN!";
                ui.statusText.color = Color.red;
            }
            Invoke(nameof(ClosePuzzle), 2f);
        };

        puzzle.OnTick += t => ui.UpdateTimer(t);
    }

    void ClosePuzzle()
{
    puzzle.Close();
    
    // Cursor wieder sperren
    var player = FindLocalPlayer();
    player?.GetComponent<PlayerMovement>()?.LockCursor(true);
}
GameObject FindLocalPlayer()
{
    foreach (var player in FindObjectsOfType<PlayerMovement>())
        if (player.isLocalPlayer) return player.gameObject;
    return null;
}

    // ── UI Helpers ───────────────────────────────────────────────────────────
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
        tmp.text      = text;
        tmp.fontSize  = fs;
        tmp.color     = color;
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
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var lgo = new GameObject("L");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;
        var tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 14;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }
}
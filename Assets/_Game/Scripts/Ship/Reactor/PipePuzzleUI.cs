using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipePuzzleUI : MonoBehaviour
{
    public static PipePuzzleUI Instance;

    public GameObject     overlayPanel;
    public RectTransform  gridContainer;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;

    public float tileSize    = 72f;
    public float tileSpacing = 5f;
    public float pipeWidth   = 16f; // Breite der Rohr-Segmente

    private Image[,]      tileBgs;
    private GameObject[,] tileRoots;
    private int           n;

    // Farben
    static readonly Color C_Bg         = new Color(0.15f, 0.15f, 0.20f);
    static readonly Color C_BgStart    = new Color(0.10f, 0.35f, 0.70f);
    static readonly Color C_BgEnd      = new Color(0.70f, 0.35f, 0.10f);
    static readonly Color C_Pipe       = new Color(0.85f, 0.85f, 0.90f);
    static readonly Color C_PipeSolved = new Color(0.15f, 0.90f, 0.40f);
    static readonly Color C_BgSolved   = new Color(0.08f, 0.35f, 0.18f);

    void Awake()    { Instance = this; }
    void OnEnable() { Instance = this; }

    public void Show()
    {
        Instance = this;
        if (overlayPanel == null) { Debug.LogError("PipePuzzleUI: overlayPanel null"); return; }
        overlayPanel.SetActive(true);
        Build();
        RefreshAll();
        if (statusText != null) statusText.text = "Verbinde Eingang → Ausgang";
    }

    public void Hide()
    {
        if (overlayPanel != null) overlayPanel.SetActive(false);
    }

    // ── Grid bauen ───────────────────────────────────────────────────────────
    void Build()
    {
        if (PipePuzzle.Instance == null) return;
        n = PipePuzzle.Instance.gridSize;

        foreach (Transform c in gridContainer) Destroy(c.gameObject);

        tileBgs   = new Image[n, n];
        tileRoots = new GameObject[n, n];

        float step  = tileSize + tileSpacing;
        float total = n * step - tileSpacing;
        gridContainer.sizeDelta = new Vector2(total, total);

        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
            {
                float px = x * step - total / 2f + tileSize / 2f;
                float py = y * step - total / 2f + tileSize / 2f;
                MakeTile(x, y, new Vector2(px, py));
            }
    }

    void MakeTile(int x, int y, Vector2 pos)
    {
        var go = new GameObject($"T{x}{y}");
        go.transform.SetParent(gridContainer, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(tileSize, tileSize);
        rt.anchoredPosition = pos;

        // Hintergrund
        var bg = go.AddComponent<Image>();
        bg.color = C_Bg;

        // Klick
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor     = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;
        int cx = x, cy = y;
        btn.onClick.AddListener(() => PipePuzzle.Instance?.RotateTile(cx, cy));

        tileBgs[x, y]   = bg;
        tileRoots[x, y] = go;
    }

    // ── Alle Kacheln aktualisieren ───────────────────────────────────────────
    public void RefreshAll()
    {
        if (PipePuzzle.Instance?.Grid == null) return;
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                RefreshTile(x, y);
    }

    public void RefreshTile(int x, int y)
    {
        if (tileRoots == null || PipePuzzle.Instance?.Grid == null) return;
        var tile = PipePuzzle.Instance.Grid[x, y];
        if (tile == null) return;

        var root = tileRoots[x, y];

        // Hintergrundfarbe
        bool isStart = new Vector2Int(x, y) == PipePuzzle.Instance.Start;
        bool isEnd   = new Vector2Int(x, y) == PipePuzzle.Instance.End;
        tileBgs[x, y].color = isStart ? C_BgStart : isEnd ? C_BgEnd : C_Bg;

        // Rohr-Segmente neu zeichnen
        // Alle alten Rohr-Kinder löschen
        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Destroy(root.transform.GetChild(i).gameObject);

        DrawPipe(root, tile.Connections, C_Pipe);
    }

    // ── Rohr zeichnen ────────────────────────────────────────────────────────
    // Zeichnet Rechtecke die das Rohr-Muster darstellen
    void DrawPipe(GameObject parent, Dir connections, Color color)
    {
        float half   = tileSize / 2f;
        float pw     = pipeWidth;
        float halfPW = pw / 2f;

        // Mitte (immer wenn mehr als eine Verbindung)
        int connCount = CountBits((int)connections);
        if (connCount >= 1)
            MakeRect(parent, "Center", Vector2.zero, new Vector2(pw, pw), color);

        // Segmente pro Richtung
        if ((connections & Dir.Up) != 0)
            MakeRect(parent, "Up", new Vector2(0, half / 2f), new Vector2(pw, half), color);
        if ((connections & Dir.Down) != 0)
            MakeRect(parent, "Down", new Vector2(0, -half / 2f), new Vector2(pw, half), color);
        if ((connections & Dir.Right) != 0)
            MakeRect(parent, "Right", new Vector2(half / 2f, 0), new Vector2(half, pw), color);
        if ((connections & Dir.Left) != 0)
            MakeRect(parent, "Left", new Vector2(-half / 2f, 0), new Vector2(half, pw), color);
    }

    void MakeRect(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
    }

    int CountBits(int v)
    {
        int c = 0;
        while (v != 0) { c += v & 1; v >>= 1; }
        return c;
    }

    // ── Gelöst ───────────────────────────────────────────────────────────────
    public void ShowSolved()
    {
        if (tileRoots == null) return;
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
            {
                if (tileBgs[x, y] != null)
                    tileBgs[x, y].color = C_BgSolved;

                // Rohre grün färben
                if (tileRoots[x, y] != null)
                    foreach (Transform child in tileRoots[x, y].transform)
                    {
                        var img = child.GetComponent<Image>();
                        if (img != null) img.color = C_PipeSolved;
                    }
            }

        if (statusText != null)
        {
            statusText.text  = "VERBINDUNG HERGESTELLT!";
            statusText.color = C_PipeSolved;
        }
    }

    // ── Timer ────────────────────────────────────────────────────────────────
    public void UpdateTimer(float t)
    {
        if (timerText == null) return;
        timerText.text  = $"{Mathf.Max(0f, t):0.0}s";
        timerText.color = t < 15f ? Color.red : t < 30f ? Color.yellow : Color.white;
    }
}
using UnityEngine;
using System.Collections.Generic;

// Verbindungsrichtungen als Bitfeld
[System.Flags]
public enum Dir { None = 0, Up = 1, Right = 2, Down = 4, Left = 8 }

public static class DirUtils
{
    public static Dir Opposite(Dir d)
    {
        switch (d)
        {
            case Dir.Up:    return Dir.Down;
            case Dir.Down:  return Dir.Up;
            case Dir.Left:  return Dir.Right;
            case Dir.Right: return Dir.Left;
            default:        return Dir.None;
        }
    }

    public static Vector2Int ToVec(Dir d)
    {
        switch (d)
        {
            case Dir.Up:    return Vector2Int.up;
            case Dir.Down:  return Vector2Int.down;
            case Dir.Left:  return Vector2Int.left;
            case Dir.Right: return Vector2Int.right;
            default:        return Vector2Int.zero;
        }
    }

    public static Dir FromVec(Vector2Int v)
    {
        if (v == Vector2Int.up)    return Dir.Up;
        if (v == Vector2Int.down)  return Dir.Down;
        if (v == Vector2Int.left)  return Dir.Left;
        if (v == Vector2Int.right) return Dir.Right;
        return Dir.None;
    }

    // Rotiert eine Richtung um 90° im Uhrzeigersinn
    public static Dir RotateCW(Dir d)
    {
        switch (d)
        {
            case Dir.Up:    return Dir.Right;
            case Dir.Right: return Dir.Down;
            case Dir.Down:  return Dir.Left;
            case Dir.Left:  return Dir.Up;
            default:        return Dir.None;
        }
    }

    // Rotiert ein Verbindungs-Bitfeld um steps * 90° im Uhrzeigersinn
    public static Dir RotateConn(Dir conn, int steps)
    {
        steps = ((steps % 4) + 4) % 4;
        Dir result = conn;
        for (int i = 0; i < steps; i++)
        {
            Dir next = Dir.None;
            foreach (Dir d in new[] { Dir.Up, Dir.Right, Dir.Down, Dir.Left })
                if ((result & d) != 0) next |= RotateCW(d);
            result = next;
        }
        return result;
    }
}

public class PipeTile
{
    // Basisverbindungen bei rotation=0
    public readonly Dir baseConn;
    public int rotation; // 0-3

    public PipeTile(Dir baseConn, int rotation = 0)
    {
        this.baseConn = baseConn;
        this.rotation = ((rotation % 4) + 4) % 4;
    }

    public Dir Connections => DirUtils.RotateConn(baseConn, rotation);

    public void RotateCW() => rotation = (rotation + 1) % 4;

    // Verbindet diese Kachel mit Nachbar in Richtung d?
    public bool ConnectsTo(Dir d, PipeTile neighbor)
    {
        if (neighbor == null) return false;
        return (Connections & d) != 0 && (neighbor.Connections & DirUtils.Opposite(d)) != 0;
    }

    // Fabrik-Methoden
    public static PipeTile End(int rotation)      => new PipeTile(Dir.Up, rotation);
    public static PipeTile Straight(int rotation) => new PipeTile(Dir.Up | Dir.Down, rotation);
    public static PipeTile Curve(int rotation)    => new PipeTile(Dir.Up | Dir.Right, rotation);
    public static PipeTile T(int rotation)        => new PipeTile(Dir.Up | Dir.Right | Dir.Down, rotation);
    public static PipeTile Cross()                => new PipeTile(Dir.Up | Dir.Right | Dir.Down | Dir.Left, 0);
}

public class PipePuzzle : MonoBehaviour
{
    public static PipePuzzle Instance;

    [Header("Settings")]
    public int gridSize  = 7;
    public float timeLimit = 90f;

    public PipeTile[,] Grid   { get; private set; }
    public Vector2Int  Start  { get; private set; }
    public Vector2Int  End    { get; private set; }
    public bool IsActive      { get; private set; }
    public bool IsSolved      { get; private set; }
    public float TimeLeft     { get; private set; }

    public event System.Action          OnSolved;
    public event System.Action          OnFailed;
    public event System.Action<float>   OnTick;

    void Awake() => Instance = this;

    void Update()
    {
        if (!IsActive || IsSolved) return;
        TimeLeft -= Time.deltaTime;
        OnTick?.Invoke(TimeLeft);
        if (TimeLeft <= 0f) { TimeLeft = 0f; IsActive = false; OnFailed?.Invoke(); }
    }

    public void StartPuzzle()
    {
        Build();
        TimeLeft = timeLimit;
        IsActive = true;
        IsSolved = false;
    }

    public void Close()
    {
        IsActive = false;
        PipePuzzleUI.Instance?.Hide();
    }

    public void RotateTile(int x, int y)
    {
        if (!IsActive || IsSolved) return;
        Grid[x, y].RotateCW();
        PipePuzzleUI.Instance?.RefreshTile(x, y);
        if (CheckSolved()) { IsSolved = true; IsActive = false; OnSolved?.Invoke(); }
    }

    // ── Puzzle bauen ──────────────────────────────────────────────────────────
    void Build()
    {
        Grid  = new PipeTile[gridSize, gridSize];
        Start = new Vector2Int(0,           gridSize / 2);
        End   = new Vector2Int(gridSize - 1, gridSize / 2);

        // 1. Lösungspfad
        var path = FindPath();

        // 2. Kacheln entlang Pfad setzen (korrekte Verbindungen, rotation=0 als Basis)
        for (int i = 0; i < path.Count; i++)
        {
            var pos  = path[i];
            var prev = i > 0              ? path[i - 1] : pos;
            var next = i < path.Count - 1 ? path[i + 1] : pos;

            Grid[pos.x, pos.y] = MakeTile(pos, prev, next, i == 0, i == path.Count - 1);
        }

        // 3. Restliche Felder füllen
        Dir[] fillerBases = { Dir.Up | Dir.Down, Dir.Up | Dir.Right, Dir.Up | Dir.Right | Dir.Down };
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                if (Grid[x, y] == null)
                    Grid[x, y] = new PipeTile(fillerBases[Random.Range(0, fillerBases.Length)], Random.Range(0, 4));

        // 4. Alle Kacheln zufällig rotieren (Puzzle entsteht)
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
            {
                int r = Random.Range(0, 4);
                for (int k = 0; k < r; k++) Grid[x, y].RotateCW();
            }
    }

    PipeTile MakeTile(Vector2Int pos, Vector2Int prev, Vector2Int next, bool isStart, bool isEnd)
    {
        if (isStart)
        {
            // End-Kachel: öffnet sich Richtung next
            Dir toNext = DirUtils.FromVec(next - pos);
            return new PipeTile(Dir.Up, RotationForDir(toNext));
        }
        if (isEnd)
        {
            // End-Kachel: öffnet sich Richtung prev
            Dir toPrev = DirUtils.FromVec(prev - pos);
            return new PipeTile(Dir.Up, RotationForDir(toPrev));
        }

        Dir fromDir = DirUtils.FromVec(prev - pos); // kommt aus dieser Richtung
        Dir toDir   = DirUtils.FromVec(next - pos); // geht in diese Richtung

        Dir needed = fromDir | toDir; // welche Verbindungen wir brauchen

        // Gerade: gegenüberliegende Verbindungen
        if (fromDir == DirUtils.Opposite(toDir))
        {
            // Gerade Linie: rotation 0 = Up|Down, rotation 1 = Right|Left
            bool horizontal = (needed & Dir.Left) != 0 || (needed & Dir.Right) != 0;
            return PipeTile.Straight(horizontal ? 1 : 0);
        }
        else
        {
            // Kurve: finde rotation so dass baseConn (Up|Right) nach Rotation == needed
            for (int r = 0; r < 4; r++)
                if (DirUtils.RotateConn(Dir.Up | Dir.Right, r) == needed)
                    return PipeTile.Curve(r);
            return PipeTile.Curve(0); // fallback
        }
    }

    // Rotation so dass Dir.Up nach Rotation == targetDir
    int RotationForDir(Dir targetDir)
    {
        for (int r = 0; r < 4; r++)
            if (DirUtils.RotateConn(Dir.Up, r) == targetDir)
                return r;
        return 0;
    }

    // ── Pfad finden – garantiert gewunden ───────────────────────────────────
    // Mindestlänge: 70% aller Felder müssen belegt sein
    int MinPathLength => Mathf.RoundToInt(gridSize * gridSize * 0.70f);

    List<Vector2Int> FindPath()
    {
        // Mehrere Versuche, nehme den längsten der die Mindestlänge erfüllt
        List<Vector2Int> best = null;

        for (int attempt = 0; attempt < 500; attempt++)
        {
            var path    = new List<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            if (DFS(Start, path, visited) && path.Count >= MinPathLength)
            {
                if (best == null || path.Count > best.Count)
                    best = new List<Vector2Int>(path);
                if (best.Count >= gridSize * gridSize - 2)
                    break; // Fast perfekt, reicht
            }
        }

        if (best != null && best.Count >= MinPathLength)
            return best;

        // Fallback: gerader Pfad
        Debug.LogWarning("PipePuzzle: Fallback auf geraden Pfad");
        var fb = new List<Vector2Int>();
        for (int x = 0; x <= gridSize - 1; x++) fb.Add(new Vector2Int(x, Start.y));
        return fb;
    }

    bool DFS(Vector2Int pos, List<Vector2Int> path, HashSet<Vector2Int> visited)
    {
        path.Add(pos);
        visited.Add(pos);
        if (pos == End) return true;

        // Richtungen: stark zufällig, KEIN Bias Richtung Ziel
        // Das erzwingt gewundene Pfade
        Dir[] allDirs = { Dir.Right, Dir.Up, Dir.Down, Dir.Left };
        Shuffle(allDirs);

        // Wenn wir noch weit vom Ziel weg sind, bevorzuge Richtungen weg vom Ziel
        // Das erzwingt Umwege
        float distToEnd = Vector2Int.Distance(pos, End);
        bool forceWinding = distToEnd > 2f && visited.Count < MinPathLength - gridSize;

        if (forceWinding)
        {
            // Sortiere: bevorzuge Richtungen WEG vom Ziel
            System.Array.Sort(allDirs, (a, b) =>
                Vector2Int.Distance(pos + DirUtils.ToVec(b), End)
                .CompareTo(Vector2Int.Distance(pos + DirUtils.ToVec(a), End)));
        }

        foreach (var d in allDirs)
        {
            var next = pos + DirUtils.ToVec(d);
            if (InGrid(next) && !visited.Contains(next))
                if (DFS(next, path, visited)) return true;
        }

        path.RemoveAt(path.Count - 1);
        visited.Remove(pos);
        return false;
    }

    // ── Gewinn-Check (Floodfill) ─────────────────────────────────────────────
    bool CheckSolved()
    {
        var visited = new HashSet<Vector2Int>();
        return Flood(Start, visited);
    }

    bool Flood(Vector2Int pos, HashSet<Vector2Int> visited)
    {
        if (!InGrid(pos) || visited.Contains(pos)) return false;
        visited.Add(pos);
        if (pos == End) return true;

        var tile = Grid[pos.x, pos.y];
        Dir conn = tile.Connections;

        foreach (Dir d in new[] { Dir.Up, Dir.Right, Dir.Down, Dir.Left })
        {
            if ((conn & d) == 0) continue;
            var npos = pos + DirUtils.ToVec(d);
            if (!InGrid(npos)) continue;
            var neighbor = Grid[npos.x, npos.y];
            if ((neighbor.Connections & DirUtils.Opposite(d)) == 0) continue;
            if (Flood(npos, visited)) return true;
        }
        return false;
    }

    bool InGrid(Vector2Int p) => p.x >= 0 && p.x < gridSize && p.y >= 0 && p.y < gridSize;

    void Shuffle<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        { int j = Random.Range(0, i + 1); var t = arr[i]; arr[i] = arr[j]; arr[j] = t; }
    }
}
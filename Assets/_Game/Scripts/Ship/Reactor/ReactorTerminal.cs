using Mirror;
using UnityEngine;

public class ReactorTerminal : NetworkBehaviour
{
    [Header("Settings")]
    public float repairAmount = 25f;

    [Header("Optional")]
    public UnityEngine.UI.Button repairButton;

    [Header("Proximity")]
    public float viewDistance = 8f; // Distanz bis zu der andere Spieler den Screen sehen können

    [SyncVar(hook = nameof(OnInUseChanged))]
    private bool isInUse = false;

    private NetworkConnectionToClient currentUserConn;

    // ── Vom Button aufgerufen ─────────────────────────────────────────────────
    public void StartRepair()
    {
        if (isInUse)
        {
            Debug.Log("Terminal wird bereits benutzt");
            return;
        }

        if (ReactorSystem.Instance != null && ReactorSystem.Instance.integrity >= 100f)
        {
            Debug.Log("Reaktor bereits bei 100%");
            return;
        }

        CmdRequestRepair();
    }

    [Command(requiresAuthority = false)]
    void CmdRequestRepair(NetworkConnectionToClient sender = null)
    {
        if (isInUse) return;

        var conn = sender ?? connectionToClient;
        if (conn == null) return;

        isInUse         = true;
        currentUserConn = conn;

        RpcOpenPuzzle(currentUserConn);
    }

    [TargetRpc]
    void RpcOpenPuzzle(NetworkConnectionToClient target)
    {
        RegisterEvents();
        PipePuzzle.Instance.StartPuzzle();
        PipePuzzleUI.Instance.Show();
    }

    // ── Events ───────────────────────────────────────────────────────────────

    void RegisterEvents()
    {
        PipePuzzle.Instance.OnSolved -= OnSolved;
        PipePuzzle.Instance.OnFailed -= OnFailed;
        PipePuzzle.Instance.OnSolved += OnSolved;
        PipePuzzle.Instance.OnFailed += OnFailed;
    }

    void UnregisterEvents()
    {
        if (PipePuzzle.Instance == null) return;
        PipePuzzle.Instance.OnSolved -= OnSolved;
        PipePuzzle.Instance.OnFailed -= OnFailed;
    }

    void OnSolved()
    {
        CmdRepair();
    }

    void OnFailed()
    {
        UnregisterEvents();
        CmdReleaseLock();
    }

    // ── Server ───────────────────────────────────────────────────────────────

    [Command(requiresAuthority = false)]
    void CmdRepair()
    {
        if (ReactorSystem.Instance != null)
            ReactorSystem.Instance.Repair(repairAmount);

        bool complete = ReactorSystem.Instance == null || ReactorSystem.Instance.integrity >= 100f;
        RpcOnRepairDone(currentUserConn, complete);
    }

    [TargetRpc]
    void RpcOnRepairDone(NetworkConnectionToClient target, bool complete)
    {
        if (complete)
        {
            UnregisterEvents();
            PipePuzzle.Instance?.Close();
            CmdReleaseLock();
        }
        else
        {
            PipePuzzleUI.Instance?.ShowSolved();
            Invoke(nameof(StartNextRound), 1.5f);
        }
    }

    void StartNextRound()
    {
        PipePuzzle.Instance.StartPuzzle();
        PipePuzzleUI.Instance.Show();
    }

    [Command(requiresAuthority = false)]
    void CmdReleaseLock()
    {
        isInUse         = false;
        currentUserConn = null;
    }

    // ── SyncVar Hook ─────────────────────────────────────────────────────────

    void OnInUseChanged(bool oldVal, bool newVal)
    {
        if (repairButton != null)
            repairButton.interactable = !newVal;
    }

    // ── ESC verlassen ────────────────────────────────────────────────────────

    void Update()
    {
        if (!isInUse) return;
        if (PipePuzzle.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnregisterEvents();
            PipePuzzle.Instance.Close();
            CmdReleaseLock();
        }
    }
}
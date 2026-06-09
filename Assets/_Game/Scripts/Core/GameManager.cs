using Mirror;
using UnityEngine;

public enum GameState
{
    Lobby,
    Travelling,
    Warping,
    Encounter,
    Docked,
    GameOver
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SyncVar(hook = nameof(OnGameStateChanged))]
    public GameState currentState = GameState.Lobby;

    [SyncVar]
    public string gameOverReason = "";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── State Transitions ──

    [Server]
    public void SetState(GameState newState)
    {
        currentState = newState;
    }

    [Server]
    public void TriggerGameOver(string reason)
    {
        gameOverReason = reason;
        SetState(GameState.GameOver);
    }

    void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"GameState: {oldState} -> {newState}");

        switch (newState)
        {
            case GameState.GameOver:
                HandleGameOver();
                break;
            case GameState.Encounter:
                HandleEncounterStart();
                break;
            case GameState.Warping:
                HandleWarpStart();
                break;
        }
    }

    void HandleGameOver()
    {
        GameOverUI.Instance?.Show(gameOverReason);
    }

    void HandleEncounterStart()
    {
        // Später: EncounterManager triggern
    }

    void HandleWarpStart()
    {
        // Später: WarpSystem triggern
    }

    // ── Convenience Checks ──
    public bool IsInCombat => currentState == GameState.Encounter;
    public bool IsWarping => currentState == GameState.Warping;
    public bool IsGameOver => currentState == GameState.GameOver;
}
using UnityEngine;

public static class StateManager
{
    public static BaseState CurrentState;

    public delegate void GameStateChangedHandler(BaseState newState);
    public static event GameStateChangedHandler OnGameStateChanged;

    public static void SetState(BaseState newState)
    {
        if (CurrentState is GameOverState
            && (newState is FightState || newState is GameState))
            return;

        // Exit current state
        CurrentState?.OnStateExit();

        // Enter new state
        CurrentState = newState;
        CurrentState?.OnStateEnter();

        OnGameStateChanged?.Invoke(CurrentState);
    }
}

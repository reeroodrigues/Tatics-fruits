using System;
using DefaultNamespace.New_GameplayCore;

namespace New_GameplayCore.GameState
{
    public class GameStateMachine : IGameStateMachine
    {
        public DefaultNamespace.New_GameplayCore.GameState Current { get; private set; } = DefaultNamespace.New_GameplayCore.GameState.Boot;
        public event Action<DefaultNamespace.New_GameplayCore.GameState> OnStateChanged;

        public void SetState(DefaultNamespace.New_GameplayCore.GameState state)
        {
            if (Current == state) return;
            Current = state;
            OnStateChanged?.Invoke(state);
        }

        public event Action<DefaultNamespace.New_GameplayCore.GameState> OnStateChange;
    }
}
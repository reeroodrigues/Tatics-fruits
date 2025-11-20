using System;
using DefaultNamespace.New_GameplayCore;

namespace New_GameplayCore
{
    public interface IGameController
    {
        bool IsPlaying { get; }
        event Action OnEnterPreRound;
        event Action OnExitPreRound;
        event Action<EndCause> OnLevelEnded;
        void StartLevel(LevelConfigSO cfg, DeckConfigSo deckCfg);
        void UpdateTick(float deltaTime);
        void OnCardSelected(CardInstance card);
        void BeginPlayFromPreRound(New_GameplayCore.PreRoundModel model);
        void BackToLevelSelect();
        void OnSwapAllRequested();
        void OnSwapRandomRequested();
    }
}
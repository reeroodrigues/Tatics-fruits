namespace New_GameplayCore.Services
{
    public interface IPreRoundPresenter
    {
        PreRoundModel BuildModel(LevelConfigSO cfg, IDeckService deck, IHighScoreService highscores);

        void OnStartClicked();
        void OnBackClicked();
        void OnToggleUseFixedSeed(bool value);
    }
}
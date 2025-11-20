namespace New_GameplayCore.Services
{
    public interface ILevelProgressService
    {
        int CurrentIndex { get; }
        LevelConfigSO Current(LevelSetSO set);
        void RecordResult(LevelConfigSO cfg, int totalScore, int stars);
        bool CanAdvance(LevelConfigSO cfg, int totalScore, float unlockPct = 0.75f);
        void Advance(LevelSetSO set);
        void Replay();
        void Save();
        void Load();
    }
}
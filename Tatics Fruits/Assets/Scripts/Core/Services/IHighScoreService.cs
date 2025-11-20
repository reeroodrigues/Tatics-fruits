namespace New_GameplayCore.Services
{
    public interface IHighScoreService
    {
        int GetBest(string levelId);
        bool TryReportScore(string levelId, int score);
        event System.Action<string, int> OnHighScoreChanged;
    }
}
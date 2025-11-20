using System;
using System.Collections.Generic;
using New_GameplayCore.Services;
using UnityEngine;

[Serializable]
public class HighScoreData
{
    public Dictionary<string, int> scores = new();
}

public class JsonHighScoreService : IHighScoreService
{
    private const string FILE_NAME = "highscores.json";

    private HighScoreData _data;

    public event Action<string, int> OnHighScoreChanged;

    public JsonHighScoreService()
    {
        if (!JsonDataService.TryLoad(FILE_NAME, out _data))
            _data = new HighScoreData();
    }

    public int GetBest(string levelId)
    {
        if (_data.scores.TryGetValue(levelId, out var best))
            return best;
        return 0;
    }

    public bool TryReportScore(string levelId, int score)
    {
        int currentBest = GetBest(levelId);
        if (score > currentBest)
        {
            _data.scores[levelId] = score;
            JsonDataService.Save(FILE_NAME, _data);
#if UNITY_EDITOR
            Debug.Log($"[HighScore] Novo recorde salvo: {levelId} = {score}");
#endif
            OnHighScoreChanged?.Invoke(levelId, score);
            return true;
        }

        return false;
    }
}
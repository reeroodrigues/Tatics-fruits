using System;
using System.Collections.Generic;
using New_GameplayCore.Services;
using UnityEngine;
using Core.SaveSystem;

[Serializable]
public class LevelHighScoresData : ISaveData
{
    public Dictionary<string, int> scores = new();

    public string GetFileName() => "level_highscores.json";

    public void OnBeforeSave()
    {
    }

    public void OnAfterLoad()
    {
        if (scores == null)
            scores = new Dictionary<string, int>();
    }
}

public class JsonHighScoreService : IHighScoreService
{
    private LevelHighScoresData _data;

    public event Action<string, int> OnHighScoreChanged;

    public JsonHighScoreService()
    {
        _data = SaveManager.Instance.Load<LevelHighScoresData>();
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
            SaveManager.Instance.Save(_data);
#if UNITY_EDITOR
            Debug.Log($"[HighScore] Novo recorde salvo: {levelId} = {score}");
#endif
            OnHighScoreChanged?.Invoke(levelId, score);
            return true;
        }

        return false;
    }
}
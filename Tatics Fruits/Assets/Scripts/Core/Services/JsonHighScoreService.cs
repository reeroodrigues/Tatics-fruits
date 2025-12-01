using System;
using System.Collections.Generic;
using Core.SaveSystem;
using New_GameplayCore.Services;
using UnityEngine;

namespace Core.Services
{
    [Serializable]
    public class LevelHighScoresData : ISaveData
    {
        public Dictionary<string, int> Scores = new();

        public string GetFileName() => "level_highscores.json";

        public void OnBeforeSave()
        {
        }

        public void OnAfterLoad()
        {
            if (Scores == null)
                Scores = new Dictionary<string, int>();
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
            if (_data.Scores.TryGetValue(levelId, out var best))
                return best;
            return 0;
        }

        public bool TryReportScore(string levelId, int score)
        {
            var currentBest = GetBest(levelId);
            if (score > currentBest)
            {
                _data.Scores[levelId] = score;
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
}
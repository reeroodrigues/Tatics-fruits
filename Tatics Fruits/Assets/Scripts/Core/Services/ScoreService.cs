using System;
using UnityEngine;

namespace New_GameplayCore.Services
{
    public class ScoreService : IScoreService
    {
        private readonly LevelConfigSO _cfg;
        private int _total;
        private int _currentCombo;
        private int _bestCombo;

        public int Total => _total;
        public int CurrentCombo => _currentCombo;
        public int BestCombo => _bestCombo;
        
        public event Action<int, int> OnScoreChanged;

        public ScoreService(LevelConfigSO cfg)
        {
            _cfg = cfg;
        }

        public void AddPairScore(CardInstance A, CardInstance B, float multiplier, out int pointsAdded)
        {
            var basePoints = _cfg.scorePerPairBase + A.Value + B.Value;
            pointsAdded = basePoints;
            _total += pointsAdded;
            OnScoreChanged?.Invoke(_total, pointsAdded);
        }

        public void ResetCombo()
        {
            _currentCombo = 0;
        }

        public void RegisterCombo()
        {
            _currentCombo++;
            _bestCombo = Mathf.Max(_bestCombo, _currentCombo);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace New_GameplayCore.Services
{
    [System.Serializable]
    public struct DefeatModel
    {
        public string levelId;
        public int totalScore;
        public int targetScore;
        public int starsEarned;
        public int bestBefore;
        public bool newRecord;
        public int timeLeftSeconds;
    }

    public class DefeatPresenter
    {
        private readonly LevelConfigSO _cfg;
        private readonly IScoreService _score;
        private readonly ITimeManager _time;
        private readonly IHighScoreService _hs;
        private readonly PlayerProfileService _profileService;

        public event Action<DefeatModel> OnModelReady;
        public event Action OnReplay;
        public event Action OnMenu;
        
        public void ClickReplay() => OnReplay?.Invoke();
        public void ClickMenu() => OnMenu?.Invoke();

        public DefeatPresenter(LevelConfigSO cfg,
            IScoreService score,
            ITimeManager time,
            IHighScoreService hs,
            PlayerProfileService profileService)
        {
            _cfg = cfg;
            _score = score;
            _time = time;
            _hs = hs;
            _profileService = profileService;
        }

        public void Build()
        {
            var id = string.IsNullOrEmpty(_cfg.levelId) ? _cfg.name : _cfg.levelId;
            var total = _score.Total;
            var newRecord = _hs.TryReportScore(id, total);

            var pct = Mathf.Clamp01(total / (float)_cfg.targetScore);
            var stars = 0;
            if (pct >= _cfg.star1Threshold) stars = 1;
            if (pct >= _cfg.star2Threshold) stars = 2;
            if (pct >= _cfg.star3Threshold) stars = 3;

            var levelId = _cfg.levelId;
            if (string.IsNullOrEmpty(levelId))
                levelId = _cfg.name;
            
            var best = _profileService.GetBestScore(levelId);

            OnModelReady?.Invoke(new DefeatModel
            {
                levelId = id,
                totalScore = total,
                targetScore = _cfg.targetScore,
                starsEarned = stars,
                bestBefore = best,
                newRecord = newRecord,
                timeLeftSeconds = _time.TimeLeftSeconds
            });
        }
    }
}
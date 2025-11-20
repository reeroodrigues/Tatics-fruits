using System;
using UnityEngine;

namespace New_GameplayCore.Services
{
    public class VictoryPresenter
    {
        private readonly LevelConfigSO _cfg;
        private readonly IScoreService _score;
        private readonly ITimeManager  _time;
        private readonly IHighScoreService _hs;
        private readonly ILevelProgressService _progress;
        private readonly LevelSetSO _levelSet;

        public Action<VictoryModel> OnModelReady;
        private readonly PlayerProfileService _profileService;
        public event Action OnNext;
        public event Action OnReplay;

        public VictoryPresenter(
            LevelConfigSO cfg,
            ScoreService score,
            ITimeManager time,
            IHighScoreService hs,
            PlayerProfileService profileService,
            ILevelProgressService progress,
            LevelSetSO levelSet)
        {
            _cfg = cfg;
            _score = score;
            _time = time;
            _hs = hs;
            _profileService = profileService;
            _progress = progress;
            _levelSet = levelSet;
        }

        public void Build()
        {
            var total = _score.Total;
            var target = _cfg.targetScore;
            
            var pct = Mathf.Clamp01(total/(float)target);
            var stars = 0;
            if(pct >= _cfg.star1Threshold) stars = 1;
            if(pct >= _cfg.star2Threshold) stars = 2;
            if(pct >= _cfg.star3Threshold || total >= target) stars = 3;

            var newRecord = _hs.TryReportScore(string.IsNullOrEmpty(_cfg.levelId) ? _cfg.name : _cfg.levelId, total);
            _progress.RecordResult(_cfg, total, stars);

            var canNext = _progress.CanAdvance(_cfg, total, 0.75f);

            OnModelReady?.Invoke(new VictoryModel
            {
                totalScore = total,
                targetScore = target,
                bestBefore = _profileService.GetBestScore(_cfg.levelId),
                newRecord = newRecord,
                starsEarned = stars,
                canGoNext = canNext,
                levelIndex = _progress.CurrentIndex
            });
        }
        
        public void ClickNext() => OnNext?.Invoke();
        public void ClickReplay() => OnReplay?.Invoke();
    }
}
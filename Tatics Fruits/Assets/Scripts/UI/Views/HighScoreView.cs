using System;
using New_GameplayCore.Services;
using TMPro;
using UnityEngine;

namespace New_GameplayCore.Views
{
    public class HighScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameControllerInitializer bootstrap;

        private PlayerProfileService _profile;
        private IScoreService _score;
        private string _levelId;

        private void Start()
        {
            _profile = bootstrap.Profile;
            _score   = bootstrap.Score;

            _levelId = string.IsNullOrEmpty(bootstrap.LevelConfig.levelId)
                ? bootstrap.LevelConfig.name
                : bootstrap.LevelConfig.levelId;

            RefreshLabel(); 
            _score.OnScoreChanged += OnScoreChanged;
        }

        private void OnDestroy()
        {
            if (_score != null)
                _score.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int total, int delta)
        {
            var bestSaved = _profile.GetBestScore(_levelId);
            SetLabel(Mathf.Max(bestSaved, total));
        }

        private void RefreshLabel()
        {
            SetLabel(_profile.GetBestScore(_levelId));
        }

        private void SetLabel(int value)
        {
            if (!label) return;
            label.text = value.ToString();
        }
    }
}
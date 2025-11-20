using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class HUDView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button swapAllButton;
        [SerializeField] private Button swapOneButton;
        [SerializeField] private RectTransform timeDeltaAnchor;
        [SerializeField] private TimeDeltaToast timeDeltaToast;

        private ITimeManager _time;
        private IScoreService _score;
        private ISwapService _swap;

        public void Initialize(ITimeManager time, IScoreService score, ISwapService swap)
        {
            _time = time;
            _score = score;
            _swap = swap;

            _time.OnTimeChanged += UpdateTimer;
            _time.OnTimeDelta += ShowTimeDelta;
            _score.OnScoreChanged += UpdateScore;
            
            swapAllButton.onClick.AddListener(OnSwapAll);
            swapOneButton.onClick.AddListener(OnSwapOne);

            UpdateTimer(_time.TimeLeftSeconds);
            UpdateScore(_score.Total, 0);
        }

        private void OnDestroy()
        {
            if (_time != null)
                _time.OnTimeChanged -= UpdateTimer;
            
            if (_time != null)
                _time.OnTimeDelta -= ShowTimeDelta;

            if (_score != null)
                _score.OnScoreChanged -= UpdateScore;
        }

        private void ShowTimeDelta(int delta)
        {
            if(!timeDeltaToast || !timeDeltaAnchor)
                return;
            
            var toast = Instantiate(timeDeltaToast,timeDeltaAnchor);
            toast.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            toast.Play(delta);
        }

        private void UpdateTimer(int value)
        {
            timerText.text = value.ToString("00");
            if (value <= 10)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }

        private void UpdateScore(int total, int delta)
        {
            scoreText.text = total.ToString("N0");
        }

        private void OnSwapAll() => _swap.TrySwapAll();
        private void OnSwapOne() => _swap.TrySwapRandom();
    }
}
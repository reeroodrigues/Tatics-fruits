using MoreMountains.Feedbacks;
using New_GameplayCore;
using New_GameplayCore.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
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

        [Header("Clock Shake")]
        [SerializeField] private GameObject clockObject;
        [SerializeField] private int shakeThreshold = 10;

        private ITimeManager _time;
        private IScoreService _score;
        private ISwapService _swap;
        private MMScaleShaker _clockShaker;
        private bool _isShaking = false;

        public void Initialize(ITimeManager time, IScoreService score, ISwapService swap)
        {
            _time = time;
            _score = score;
            _swap = swap;

            if (clockObject != null)
                _clockShaker = clockObject.GetComponent<MMScaleShaker>();

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

            if (_clockShaker != null && _isShaking)
                _clockShaker.Stop();
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
            if (value <= shakeThreshold && value > 0)
            {
                timerText.color = Color.red;

                if (!_isShaking && _clockShaker != null)
                {
                    _clockShaker.Play();
                    _isShaking = true;
                }
                else
                {
                    timerText.color = Color.white;

                    if (_isShaking && _clockShaker != null)
                    {
                        _clockShaker.Stop();
                        _isShaking = false;
                    }
                }
            }
        }

        private void UpdateScore(int total, int delta)
        {
            scoreText.text = total.ToString("N0");
        }

        private void OnSwapAll() => _swap.TrySwapAll();
        private void OnSwapOne() => _swap.TrySwapRandom();
    }
}
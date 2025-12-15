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
        
        [Header("Shake Settings")]
        [SerializeField] private float idleShakeSpeed = 10f;
        [SerializeField] private float idleShakeRange = 0.05f;
        [SerializeField] private float dangerShakeSpeed = 30f;
        [SerializeField] private float dangerShakeRange = 0.2f;

        [Header("Score Feedback")]
        [SerializeField] private MMF_Player scoreFeedback;

        [Header("Combo Feedback")] 
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private MMF_Player comboIncreaseFeedback;
        [SerializeField] private MMF_Player comboLostFeedback;
        
        [Header("Swap Feedbacks")]
        [SerializeField] private MMF_Player swapAllFeedback;
        [SerializeField] private MMF_Player swapOneFeedback;

        [Header("Time Feedback")]
        [SerializeField] private MMF_Player timeBonusFeedback;

        private ITimeManager _time;
        private IScoreService _score;
        private ISwapService _swap;
        private IComboTracker _comboTracker;
        private MMScaleShaker _clockShaker;
        private bool _isInDangerZone = false;
        private int _lastCombo = 0;

        public void Initialize(ITimeManager time, IScoreService score, ISwapService swap)
        {
            _time = time;
            _score = score;
            _swap = swap;

            if (clockObject != null)
            {
                _clockShaker = clockObject.GetComponent<MMScaleShaker>();
                if (_clockShaker != null)
                {
                    SetIdleShake();
                    _clockShaker.Play();
                }
            }

            if (_comboTracker != null)
                _comboTracker.OnComboChanged += UpdateCombo;

            _time.OnTimeChanged += UpdateTimer;
            _time.OnTimeDelta += ShowTimeDelta;
            _score.OnScoreChanged += UpdateScore;
            _time.OnTimeDelta += OnTimeDelta;
            
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

            if (_clockShaker != null)
                _clockShaker.Stop();
        }

        private void OnTimeDelta(int delta)
        {
            if (delta > 0 && timeBonusFeedback != null)
                timeBonusFeedback?.PlayFeedbacks();
        }

        private void UpdateCombo(int comboCount)
        {
            if (comboCount > 0)
            {
                comboText.text = $"COMBO X {comboCount}";
                comboText.gameObject.SetActive(true);
                
                if(comboCount > _lastCombo)
                    comboIncreaseFeedback?.PlayFeedbacks();
            }
            else
            {
                comboText.gameObject.SetActive(false);
                if(_lastCombo > 0)
                    comboLostFeedback?.PlayFeedbacks();
            }
            
            _lastCombo = comboCount;
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

                if (!_isInDangerZone && _clockShaker != null)
                {
                    SetDangerShake();
                    _isInDangerZone = true;
                }
            }
            else
            {
                timerText.color = Color.white;

                if (_isInDangerZone && _clockShaker != null)
                {
                    SetIdleShake();
                    _isInDangerZone = false;
                }
            }
        }

        private void SetIdleShake()
        {
            _clockShaker.ShakeSpeed = idleShakeSpeed;
            _clockShaker.ShakeRange = idleShakeRange;
        }

        private void SetDangerShake()
        {
            _clockShaker.ShakeSpeed = dangerShakeSpeed;
            _clockShaker.ShakeRange = dangerShakeRange;
        }

        private void UpdateScore(int total, int delta)
        {
            scoreText.text = total.ToString("N0");

            if (delta > 0 && scoreFeedback != null)
            {
                scoreFeedback.PlayFeedbacks();
            }
        }

        private void OnSwapAll()
        {
            swapAllFeedback?.PlayFeedbacks();
            _swap.TrySwapAll();
        }

        private void OnSwapOne()
        { 
            swapOneFeedback?.PlayFeedbacks();
            _swap.TrySwapRandom();
        } 
    }
}

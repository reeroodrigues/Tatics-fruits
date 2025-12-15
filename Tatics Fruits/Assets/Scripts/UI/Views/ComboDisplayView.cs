using Core.ScriptableObjects;
using Core.Services;
using DG.Tweening;
using New_GameplayCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class ComboDisplayView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject comboContainer;
        [SerializeField] private TextMeshProUGUI comboCountText;
        [SerializeField] private TextMeshProUGUI comboTierText;
        [SerializeField] private Image comboProgressBar;
        [SerializeField] private Image comboBackground;
        
        [Header("Bar Fill Settings")]
        [SerializeField] private float fillAmountPerCombo = 0.2f;
        [SerializeField] private float fillAnimationDuration = 0.3f;
        [SerializeField] private Ease fillEase = Ease.OutCubic;
        [SerializeField] private Color barFillColor = Color.green;
        [SerializeField] private Color barDrainColor = Color.yellow;
        
        [Header("Animation Settings")]
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private float tierChangePunchScale = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float tierTextFadeInDuration = 0.4f;
        
        [Header("Glow Settings")]
        [SerializeField] private Image glowImage;
        [SerializeField] private float glowPulseDuration = 0.5f;
        
        private IComboTracker _comboTracker;
        private LevelConfigSO _levelConfig;
        private ComboTierConfigSo _tierConfig;
        private Tween _currentAnimation;
        private Tween _barFillTween;
        private float _targetFillAmount;
        private int _previousCombo;

        public void Initialize(IComboTracker comboTracker, LevelConfigSO levelConfig)
        {
            _comboTracker = comboTracker;
            _levelConfig = levelConfig;
            
            _comboTracker.OnComboChanged += OnComboChanged;
            _comboTracker.OnComboTierChanged += OnComboTierChanged;
            
            if (comboProgressBar != null)
            {
                comboProgressBar.fillAmount = 0f;
                comboProgressBar.color = barDrainColor;
            }
            else
            {
                Debug.LogError("[ComboDisplayView] comboProgressBar is NULL! Please assign it in the Inspector.");
            }
            
            if (comboTierText != null)
            {
                comboTierText.alpha = 0f;
            }
            
            _targetFillAmount = 0f;
            _previousCombo = 0;
            
            if (comboContainer != null)
            {
                Debug.Log($"[ComboDisplayView] Initialize - Setting comboContainer '{comboContainer.name}' to inactive");
                comboContainer.SetActive(false);
            }
            else
            {
                Debug.LogError("[ComboDisplayView] comboContainer is NULL! Please assign it in the Inspector.");
            }
        }

        private void OnDestroy()
        {
            if (_comboTracker != null)
            {
                _comboTracker.OnComboChanged -= OnComboChanged;
                _comboTracker.OnComboTierChanged -= OnComboTierChanged;
            }
            
            _currentAnimation?.Kill();
            _barFillTween?.Kill();
        }

        private void Update()
        {
            if (_comboTracker == null || _comboTracker.CurrentCombo <= 0)
                return;
            
            if (comboProgressBar != null && _levelConfig != null)
            {
                float timeProgress = _comboTracker.RemainingWindowsMs / _levelConfig.comboWindows;
                float drainedFill = _targetFillAmount * timeProgress;
                
                comboProgressBar.fillAmount = Mathf.Lerp(
                    comboProgressBar.fillAmount,
                    drainedFill,
                    Time.deltaTime * 8f
                );
                
                if (timeProgress < 0.5f)
                {
                    comboProgressBar.color = Color.Lerp(Color.red, barDrainColor, timeProgress * 2f);
                }
                else
                {
                    comboProgressBar.color = barDrainColor;
                }
            }
        }

        private void OnComboChanged(int comboCount)
        {
            Debug.Log($"[ComboDisplayView] OnComboChanged called: {comboCount}");
    
            if (comboCount <= 0)
            {
                HideCombo();
                _targetFillAmount = 0f;
                _previousCombo = 0;
                return;
            }
            
            if (comboContainer != null)
                comboContainer.SetActive(true);
    
            if (comboCountText != null)
                comboCountText.text = $"x{comboCount}";
            
            AnimateComboPunch();
            
            FillBarTowardNextTier(comboCount);
    
            _previousCombo = comboCount;
        }
        
        private void FillBarTowardNextTier(int currentCombo)
        {
            var currentTier = _comboTracker.CurrentTier;
            var nextTier = GetNextTier(currentCombo);
    
            if (nextTier == null)
            {
                _targetFillAmount = 1f;
            }
            else
            {
                int tierStart = currentTier?.minComboCount ?? 0;
                int tierRange = nextTier.minComboCount - tierStart;
                int progressInTier = currentCombo - tierStart;
        
                _targetFillAmount = Mathf.Clamp01((float)progressInTier / tierRange);
            }
            
            _barFillTween?.Kill();
            _barFillTween = comboProgressBar.DOFillAmount(_targetFillAmount, fillAnimationDuration)
                .SetEase(fillEase);
        }

        private void OnComboTierChanged(int comboCount, ComboTierConfigSo.ComboTier tier)
        {
            if (tier == null || comboTierText == null) return;
            
            comboTierText.text = tier.tierName;
            comboTierText.color = tier.tierColor;
            
            comboTierText.transform.DOKill();
            comboTierText.transform.localScale = Vector3.one;
            comboTierText.transform.DOPunchScale(
                Vector3.one * tierChangePunchScale, 
                punchDuration, 
                5, 
                1f
            );
            
            comboTierText.DOFade(1f, tierTextFadeInDuration);
        }

        private void FillBarIncremental()
        {
            if (comboProgressBar == null)
            {
                Debug.LogWarning("[ComboDisplayView] comboProgressBar is NULL in FillBarIncremental!");
                return;
            }
            
            _barFillTween?.Kill();
            
            _targetFillAmount = Mathf.Min(1f, _targetFillAmount + fillAmountPerCombo);
            
            Debug.Log($"[ComboDisplayView] FillBarIncremental - Target: {_targetFillAmount}, Current: {comboProgressBar.fillAmount}");
            
            comboProgressBar.color = barFillColor;
            _barFillTween = comboProgressBar.DOFillAmount(_targetFillAmount, fillAnimationDuration)
                .SetEase(fillEase)
                .OnComplete(() =>
                {
                    if (_comboTracker != null && _comboTracker.CurrentCombo > 0)
                    {
                        comboProgressBar.color = barDrainColor;
                    }
                });
            
            if (comboProgressBar != null)
            {
                comboProgressBar.transform.DOKill();
                comboProgressBar.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
            }
        }

        private void AnimateComboPunch()
        {
            _currentAnimation?.Kill();
            
            transform.localScale = Vector3.one;
            _currentAnimation = transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f)
                .SetEase(Ease.OutElastic);
            
            if (glowImage != null)
            {
                glowImage.DOKill();
                glowImage.color = new Color(1, 1, 1, 0);
                glowImage.DOFade(0.8f, glowPulseDuration * 0.5f)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }

        private void AnimateTierChange(ComboTierConfigSo.ComboTier tier)
        {
            _currentAnimation?.Kill();
            
            transform.localScale = Vector3.one;
            _currentAnimation = transform.DOPunchScale(Vector3.one * tierChangePunchScale, punchDuration, 1, 0.5f)
                .SetEase(Ease.OutBounce);
            
            if (comboTierText != null)
            {
                comboTierText.transform.DOKill();
                comboTierText.transform.localScale = Vector3.one;
                comboTierText.transform.DOPunchScale(Vector3.one * 0.5f, punchDuration * 1.5f, 3, 0.8f);
            }
            
            if (glowImage != null)
            {
                glowImage.DOKill();
                glowImage.color = tier.tierColor;
                glowImage.DOFade(1f, glowPulseDuration * 0.3f)
                    .SetLoops(4, LoopType.Yoyo);
            }
        }

        private void HideCombo()
        {
            _currentAnimation?.Kill();
            _barFillTween?.Kill();
            
            if (comboTierText != null)
            {
                comboTierText.DOKill();
                comboTierText.DOFade(0f, fadeOutDuration * 0.5f);
            }
            
            if (comboProgressBar != null)
            {
                comboProgressBar.DOKill();
                comboProgressBar.DOFillAmount(0f, fadeOutDuration);
            }
            
            _currentAnimation = comboContainer.transform.DOScale(0f, fadeOutDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => 
                {
                    comboContainer.SetActive(false);
                    if (comboProgressBar != null)
                    {
                        comboProgressBar.fillAmount = 0f;
                        comboProgressBar.color = barDrainColor;
                    }
                });
        }
        
        private ComboTierConfigSo.ComboTier GetNextTier(int currentCombo)
        {
            if (_tierConfig == null) return null;
    
            foreach (var tier in _tierConfig.tiers)
            {
                if (tier.minComboCount > currentCombo)
                    return tier;
            }
            return null;
        }
    }
}

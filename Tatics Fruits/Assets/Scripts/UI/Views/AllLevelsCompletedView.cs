using System;
using DG.Tweening;
using Gameplay.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.FantasyRPG;

namespace UI.Views
{
    public class AllLevelsCompletedView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button menuButton;

        [Header("Effects")]
        [SerializeField] private ParticleSystem[] celebrationParticles;
        [SerializeField] private UIParticleSystem[] uiCelebrationParticles;
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private float panelAnimationDuration = 0.8f;
        [SerializeField] private float particleDelay = 0.3f;

        public void Initialize(System.Action onMenuClick)
        {
            if(titleText)
                titleText.text = Localizer.Instance.Tr("levels_completed_title");

            if (messageText)
                messageText.text = Localizer.Instance.Tr("levels_completed_subtitle");
            
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() =>
            {
                Managers.AnalyticsManager.Instance?.TrackButtonClicked("all_levels_completed_menu");
                onMenuClick();
            });

            Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, panelAnimationDuration).SetEase(Ease.OutQuad);
            }

            if (panelTransform)
            {
                panelTransform.localScale = Vector3.zero;
                panelTransform.DOScale(1f, panelAnimationDuration).SetEase(Ease.OutBack).OnComplete(PlayParticles);
            }
            else
            {
                DOVirtual.DelayedCall(particleDelay, PlayParticles);
            }
        }

        private void PlayParticles()
        {
            Debug.Log("[AllLevelsCompletedView] Playing particles!");
            
            if(celebrationParticles != null && celebrationParticles.Length > 0)
            {
                foreach (var particleSystem in celebrationParticles)
                {
                    if(particleSystem != null)
                    {
                        Debug.Log($"[AllLevelsCompletedView] Playing ParticleSystem: {particleSystem.name}");
                        particleSystem.Play();
                    }
                }
            }
            
            if(uiCelebrationParticles != null && uiCelebrationParticles.Length > 0)
            {
                foreach (var uiParticle in uiCelebrationParticles)
                {
                    if(uiParticle != null)
                    {
                        Debug.Log($"[AllLevelsCompletedView] Playing UIParticleSystem: {uiParticle.name}");
                        uiParticle.StartParticleEmission();
                    }
                }
            }
        }

        private void OnDisable()
        {
            canvasGroup?.DOKill();
            panelTransform?.DOKill();
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Core.SaveSystem;

namespace UI.Views
{
    public class LgpdView : MonoBehaviour
    {
        [Header("Root / Panel")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Dim / Fundo escuro")]
        [SerializeField] private CanvasGroup dimCanvasGroup;

        [Header("Conteúdo")]
        [SerializeField] private TextMeshProUGUI lgpdText;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Botão Aceitar")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private CanvasGroup acceptButtonCanvasGroup;
        [SerializeField] private RectTransform acceptButtonRect;

        [Header("Configurações de Fade / Animação")]
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private float dimTargetAlpha = 0.6f;
        [SerializeField] private float panelScaleFrom = 0.9f;
        [SerializeField] private float panelSlideOffsetY = -20f;
        [SerializeField] private float buttonSlideOffsetY = -40f;
        [SerializeField] private float buttonFadeDuration = 0.25f;

        private bool _hasReachedEnd = false;
        private bool _isTransitioning = false;

        private PlayerProfileData _profile;
        private Vector2 _panelOriginalPos;
        private Vector2 _buttonOriginalPos;

        private void Awake()
        {
            _profile = SaveManager.Instance.Load<PlayerProfileData>();
        }

        private void Start()
        {
            if (_profile != null && _profile.hasAcceptedLGPD)
            {
                if (rootPanel != null)
                    rootPanel.SetActive(false);

                enabled = false;
                return;
            }

            if (rootPanel != null)
                rootPanel.SetActive(true);
            
            if (panelRect != null)
                _panelOriginalPos = panelRect.anchoredPosition;
            if (acceptButtonRect != null)
                _buttonOriginalPos = acceptButtonRect.anchoredPosition;

            PrepareInitialVisualState();
            SetupScroll();
            FadeIn();
        }

        private void PrepareInitialVisualState()
        {
            if (dimCanvasGroup != null)
            {
                dimCanvasGroup.alpha = 0f;
                dimCanvasGroup.blocksRaycasts = true;
                dimCanvasGroup.interactable = false;
            }
            
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }

            if (panelRect != null)
            {
                panelRect.localScale = Vector3.one * panelScaleFrom;
                panelRect.anchoredPosition = _panelOriginalPos + new Vector2(0f, panelSlideOffsetY);
            }
            
            if (acceptButton != null)
            {
                acceptButton.gameObject.SetActive(false);
            }

            if (acceptButtonCanvasGroup != null)
            {
                acceptButtonCanvasGroup.alpha = 0f;
            }

            if (acceptButtonRect != null)
            {
                acceptButtonRect.anchoredPosition = _buttonOriginalPos + new Vector2(0f, buttonSlideOffsetY);
            }
        }

        private void SetupScroll()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void FadeIn()
        {
            _isTransitioning = true;
            
            if (dimCanvasGroup != null)
            {
                dimCanvasGroup.DOFade(dimTargetAlpha, fadeDuration)
                    .SetEase(Ease.OutSine);
            }
            
            Sequence seq = DOTween.Sequence();

            if (panelCanvasGroup != null)
            {
                seq.Join(panelCanvasGroup.DOFade(1f, fadeDuration));
            }

            if (panelRect != null)
            {
                seq.Join(panelRect.DOScale(1f, fadeDuration).SetEase(Ease.OutBack));
                seq.Join(panelRect.DOAnchorPos(_panelOriginalPos, fadeDuration).SetEase(Ease.OutSine));
            }

            seq.OnComplete(() =>
            {
                _isTransitioning = false;

                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.blocksRaycasts = true;
                    panelCanvasGroup.interactable = true;
                }
            });
        }

        private void FadeOut()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = false;
            }

            Sequence seq = DOTween.Sequence();
            
            if (panelCanvasGroup != null)
            {
                seq.Join(panelCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine));
            }

            if (panelRect != null)
            {
                seq.Join(panelRect.DOScale(panelScaleFrom, fadeDuration).SetEase(Ease.InSine));
                seq.Join(panelRect.DOAnchorPos(_panelOriginalPos + new Vector2(0f, panelSlideOffsetY), fadeDuration)
                    .SetEase(Ease.InSine));
            }
            
            if (dimCanvasGroup != null)
            {
                seq.Join(dimCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine));
            }

            seq.OnComplete(() =>
            {
                if (rootPanel != null)
                    rootPanel.SetActive(false);

                _isTransitioning = false;
                enabled = false;
            });
        }

        private void OnScrollValueChanged(Vector2 pos)
        {
            if (_isTransitioning) return;
            
            if (!_hasReachedEnd && scrollRect != null && scrollRect.verticalNormalizedPosition <= 0.001f)
            {
                _hasReachedEnd = true;
                ShowAcceptButton();
            }
        }

        private void ShowAcceptButton()
        {
            if (acceptButton == null || acceptButtonRect == null || acceptButtonCanvasGroup == null)
                return;

            acceptButton.gameObject.SetActive(true);
            acceptButtonCanvasGroup.alpha = 0f;
            acceptButtonRect.anchoredPosition = _buttonOriginalPos + new Vector2(0f, buttonSlideOffsetY);

            Sequence seq = DOTween.Sequence();
            seq.Join(acceptButtonCanvasGroup.DOFade(1f, buttonFadeDuration).SetEase(Ease.OutSine));
            seq.Join(acceptButtonRect.DOAnchorPos(_buttonOriginalPos, buttonFadeDuration).SetEase(Ease.OutSine));
        }

        public void AcceptLgpd()
        {
            if (_isTransitioning) return;

            if (_profile == null)
                _profile = SaveManager.Instance.Load<PlayerProfileData>();

            _profile.hasAcceptedLGPD = true;
            SaveManager.Instance.Save(_profile);

            FadeOut();
        }

        private void OnDestroy()
        {
            if (panelRect != null) DOTween.Kill(panelRect);
            if (panelCanvasGroup != null) DOTween.Kill(panelCanvasGroup);
            if (dimCanvasGroup != null) DOTween.Kill(dimCanvasGroup);
            if (acceptButtonRect != null) DOTween.Kill(acceptButtonRect);
            if (acceptButtonCanvasGroup != null) DOTween.Kill(acceptButtonCanvasGroup);
        }
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CreditsPopup : MonoBehaviour
{
    [Header("Panel/Canvas")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image dimBackground;
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Behavior")]
    [SerializeField] private bool destroyOnClose = true;

    public bool IsOpen { get; private set; }

    private Sequence _animationTween;
    private bool _animating;

    private void Awake()
    {
        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        if (dimBackground)
        {
            dimBackground.enabled = false;
            dimBackground.raycastTarget = false;
            
            var dimButton = dimBackground.GetComponent<Button>();
            if (dimButton != null)
            {
                dimButton.onClick.RemoveAllListeners();
                dimButton.onClick.AddListener(Hide);
                dimBackground.raycastTarget = true;
            }
        }

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panel)
        {
            panel.localScale = Vector3.zero;
        }

        gameObject.SetActive(false);
        IsOpen = false;
        _animating = false;
    }

    private void OnDisable()
    {
        _animationTween?.Kill();
        _animationTween = null;
        _animating = false;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (dimBackground)
        {
            dimBackground.enabled = false;
        }

        IsOpen = false;
    }

    public void Show()
    {
        if (_animating) return;
        _animating = true;
        IsOpen = true;

        gameObject.SetActive(true);

        if (dimBackground)
        {
            dimBackground.enabled = true;
        }

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (panel)
        {
            panel.localScale = Vector3.zero;
        }

        _animationTween?.Kill();
        _animationTween = DOTween.Sequence()
            .Append(canvasGroup ? canvasGroup.DOFade(1f, fadeDuration) : null)
            .Join(panel ? panel.DOScale(1f, scaleDuration).SetEase(Ease.OutBack) : null)
            .OnComplete(() =>
            {
                if (canvasGroup)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                _animationTween = null;
                _animating = false;
            });
    }

    public void Hide()
    {
        if (_animating || !IsOpen) return;
        _animating = true;
        IsOpen = false;

        _animationTween?.Kill();

        if (canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        _animationTween = DOTween.Sequence()
            .Append(panel ? panel.DOScale(0f, scaleDuration * 0.85f).SetEase(Ease.InBack) : null)
            .Join(canvasGroup ? canvasGroup.DOFade(0f, fadeDuration * 0.8f) : null)
            .OnComplete(() =>
            {
                if (dimBackground)
                {
                    dimBackground.enabled = false;
                }
                gameObject.SetActive(false);
                _animationTween = null;
                _animating = false;

                if (destroyOnClose)
                {
                    Destroy(gameObject);
                }
            });
    }

    public void Toggle()
    {
        if (_animating) return;
        if (IsOpen) Hide();
        else Show();
    }
}

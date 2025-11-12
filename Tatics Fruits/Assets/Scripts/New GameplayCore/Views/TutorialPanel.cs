using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class TutorialPanel : MonoBehaviour
    {
        [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image slideImage;
    [SerializeField] private TextMeshProUGUI slideText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button closeButton;

    [Header("Slides")]
    [SerializeField] private List<TutorialSlideSo> slides = new List<TutorialSlideSo>();

    private int _currentIndex;
    private bool _isShowing;

    public event Action OnTutorialFinished;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        SetupButtons();
        HideImmediate();
    }

    private void SetupButtons()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);

        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(FinishTutorial);
    }

    public void Show()
    {
        if (slides == null || slides.Count == 0)
        {
            FinishTutorial();
            return;
        }
        
        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        _isShowing = true;
        _currentIndex = 0;
        UpdateSlide();
    }

    public void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        _isShowing = false;
    }

    private void UpdateSlide()
    {
        if (_currentIndex < 0 || _currentIndex >= slides.Count)
            return;

        var slide = slides[_currentIndex];

        if (slideImage != null)
            slideImage.sprite = slide.image;

        if (slideText != null)
        {
            var localized = slideText.GetComponent<LocalizedText>();
            if (localized != null && !string.IsNullOrEmpty(slide.localizationKey))
            {
                localized.key = slide.localizationKey;
                localized.fallback = slide.description;
                localized.Refresh();
            }
            else
            {
                slideText.text = slide.description;    
            }
            
        }
        
        if (prevButton != null)
            prevButton.gameObject.SetActive(_currentIndex > 0);

        if (nextButton != null)
            nextButton.gameObject.SetActive(_currentIndex < slides.Count - 1);

        if (closeButton != null)
            closeButton.gameObject.SetActive(_currentIndex == slides.Count - 1);
    }

    private void OnNextClicked()
    {
        if (!_isShowing) return;
        if (_currentIndex < slides.Count - 1)
        {
            _currentIndex++;
            UpdateSlide();
        }
    }

    private void OnPrevClicked()
    {
        if (!_isShowing) return;
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateSlide();
        }
    }

    private void FinishTutorial()
    {
        if (!_isShowing) return;

        HideImmediate();
        OnTutorialFinished?.Invoke();
    }
    }
}

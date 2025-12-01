using System;
using System.Collections;
using Gameplay.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Views
{
    public class StartSceneView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button guestButton;
        [SerializeField] private Button googleButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Toggle checkTerms;
        [SerializeField] private TextMeshProUGUI titleTermsText;
        [SerializeField] private TextMeshProUGUI descriptionTermsText;
        [SerializeField] private GameObject guestPanel;
        [SerializeField] private GameObject termsPanel;
        [SerializeField] private GameObject helpPanel;

        [Header("Guest Panel")] 
        [SerializeField] private Button guestYesButton;
        [SerializeField] private Button guestNoButton;

        [Header("Navigation")] 
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [Header("Terms Feedback")]
        [SerializeField] private Color termsWarningColor = Color.red;
        [SerializeField] private float termsBlinkDuration = 0.15f;
        
        [Header("Loading")]
        [SerializeField] private float minimumLoadingTime = 2f;
        [SerializeField] private float progressBarSpeed = 3f;
        [SerializeField] private string loadingTextKey = "loading";

        private Color _termsOriginalColor;
        private bool _isBlinkingTerms;
        private Camera _uiCamera;
        private Localizer _localizer;

        private void Awake()
        {
            if (titleTermsText != null)
                _termsOriginalColor = titleTermsText.color;
            
            if(googleButton != null)
                googleButton.onClick.AddListener(OnGoogleButtonClicked);

            if (guestPanel != null)
                guestPanel.SetActive(false);

            if (termsPanel != null)
                termsPanel.SetActive(false);
            
            if(guestButton != null)
                guestButton.onClick.AddListener(OnGuestButtonClicked);
            
            if(guestYesButton != null)
                guestYesButton.onClick.AddListener(OnGuestYesClicked);

            if (guestNoButton != null)
                guestNoButton.onClick.AddListener(OnGuestNoClicked);
            
            if(helpButton != null)
                helpButton.onClick.AddListener(OnHelpClicked);

            if (titleTermsText != null && titleTermsText.canvas != null)
            {
                var canvas = titleTermsText.canvas;
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera;
            }
        }

        private void OnDestroy()
        {
            if(guestButton != null)
                guestButton.onClick.RemoveAllListeners();
            
            if(guestYesButton != null)
                guestYesButton.onClick.RemoveAllListeners();
            
            if(guestNoButton != null)
                guestNoButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            DetectTermsTextClick();
        }

        private void DetectTermsTextClick()
        {
            if (titleTermsText == null)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            var mousePosition = Input.mousePosition;
            
            var isOverText = TMP_TextUtilities.IsIntersectingRectTransform(
                titleTermsText.rectTransform,
                mousePosition,
                _uiCamera
            );

            if (isOverText)
            {
                OpenTermsPanel();
            }
        }

        private void OpenTermsPanel()
        {
            if(termsPanel != null)
                termsPanel.SetActive(true);
            
            descriptionTermsText.text = _localizer.Tr("descriptionTerms_text");
        }

        public void CloseTermsPanel()
        {
            if(termsPanel != null)
                termsPanel.SetActive(false);
        }
        
        private void OnHelpClicked()
        {
            if(helpPanel != null)
                helpPanel.SetActive(true);
        }

        public void CloseHelpPanel()
        {
            if(helpPanel != null)
                helpPanel.SetActive(false);
        }
        
        private void OnGoogleButtonClicked()
        {
            if(checkTerms == null)
                return;

            if (!checkTerms.isOn)
            {
                FlashTermsTitleOnce();
                return;
            }
            
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnGuestButtonClicked()
        {
            if(checkTerms == null)
                return;

            if (!checkTerms.isOn)
            {
                FlashTermsTitleOnce();
                return;
            }
            
            if(guestPanel != null)
                guestPanel.SetActive(true);
        }

        private void OnGuestNoClicked()
        {
            if(guestPanel != null)
                guestPanel.SetActive(false);
        }

        private void OnGuestYesClicked()
        {
            if (guestPanel != null)
                guestPanel.SetActive(false);

            ProceedAsGuest();
        }

        private void ProceedAsGuest()
        {
            if(string.IsNullOrEmpty(mainMenuSceneName))
                return;
            
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void FlashTermsTitleOnce()
        {
            if(_isBlinkingTerms || titleTermsText == null)
                return;

            StartCoroutine(FlashTermsCoroutine());
        }

        private IEnumerator FlashTermsCoroutine()
        {
            _isBlinkingTerms = true;

            titleTermsText.color = termsWarningColor;
            yield return new WaitForSeconds(termsBlinkDuration);
            
            titleTermsText.color = _termsOriginalColor;
            _isBlinkingTerms = false;
        }
    }
}
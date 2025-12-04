using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ads
{
    public class AdDebugPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button showAdButton;
        [SerializeField] private Button toggleTestModeButton;
        [SerializeField] private TextMeshProUGUI testModeText;
        [SerializeField] private GameObject panel;

        private bool _isVisible = true;

        private void Start()
        {
            if (showAdButton != null)
            {
                showAdButton.onClick.AddListener(OnShowAdButtonClicked);
            }

            if (toggleTestModeButton != null)
            {
                toggleTestModeButton.onClick.AddListener(OnToggleTestMode);
                UpdateTestModeText();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                TogglePanelVisibility();
            }

            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
            if (InterstitialAdManager.Instance == null)
            {
                if (statusText != null)
                {
                    statusText.text = "Status: Ad Manager not found!";
                    statusText.color = Color.red;
                }
                return;
            }

            float timeUntilNextAd = InterstitialAdManager.Instance.GetMatchesUntilNextAd();
            
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeUntilNextAd / 60f);
                int seconds = Mathf.FloorToInt(timeUntilNextAd % 60f);
                timerText.text = $"Next ad in: {minutes:00}:{seconds:00}";
            }

            if (statusText != null)
            {
                statusText.text = timeUntilNextAd <= 0 ? "Status: Ready to show ad" : "Status: Counting down...";
                statusText.color = timeUntilNextAd <= 0 ? Color.green : Color.yellow;
            }
        }

        private void OnShowAdButtonClicked()
        {
            if (InterstitialAdManager.Instance != null)
            {
                Debug.Log("[AdDebugPanel] Manual ad trigger requested");
                InterstitialAdManager.Instance.ShowInterstitialAd();
            }
            else
            {
                Debug.LogError("[AdDebugPanel] InterstitialAdManager not found!");
            }
        }

        private void OnToggleTestMode()
        {
            if (InterstitialAdManager.Instance != null)
            {
                InterstitialAdManager.Instance.ToggleTestMode();
                UpdateTestModeText();
            }
        }

        private void UpdateTestModeText()
        {
            if (testModeText != null && InterstitialAdManager.Instance != null)
            {
                bool isTestMode = InterstitialAdManager.Instance.IsTestModeEnabled();
                testModeText.text = isTestMode ? "Test Mode: ON (30s)" : "Test Mode: OFF (3min)";
                testModeText.color = isTestMode ? Color.cyan : Color.white;
            }
        }

        private void TogglePanelVisibility()
        {
            _isVisible = !_isVisible;
            if (panel != null)
            {
                panel.SetActive(_isVisible);
            }
        }
    }
}

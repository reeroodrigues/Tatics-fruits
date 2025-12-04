using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ads
{
    public class AdDebugUI : MonoBehaviour
    {
        private GameObject _panel;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _testModeText;
        private TextMeshProUGUI _bannerStatusText;
        private Button _showAdButton;
        private Button _toggleTestModeButton;
        private Button _toggleBannerButton;
        private bool _isVisible = true;

        private void Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            CreateDebugUI();
#else
            Debug.Log("[AdDebugUI] Debug UI disabled in production build");
            enabled = false;
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F1))
            {
                TogglePanelVisibility();
            }

            UpdateStatusDisplay();
#endif
        }

        private void CreateDebugUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[AdDebugUI] No Canvas found in scene. Cannot create debug UI.");
                return;
            }

            _panel = new GameObject("AdDebugPanel");
            _panel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(10, -10);
            panelRect.sizeDelta = new Vector2(350, 250);

            Image panelBg = _panel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);

            VerticalLayoutGroup layout = _panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateTitle();
            CreateStatusText();
            CreateTimerText();
            CreateTestModeText();
            CreateBannerStatusText();
            CreateShowAdButton();
            CreateToggleTestModeButton();
            CreateToggleBannerButton();
            CreateHelpText();
        }

        private void CreateToggleBannerButton()
        {
            GameObject buttonObj = new GameObject("ToggleBannerButton");
            buttonObj.transform.SetParent(_panel.transform, false);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.2f, 0.4f, 0.6f, 1f);
            
            _toggleBannerButton = buttonObj.AddComponent<Button>();
            _toggleBannerButton.onClick.AddListener(OnToggleBanner);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "TOGGLE BANNER";
            buttonText.fontSize = 14;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            LayoutElement le = buttonObj.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
        }

        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_panel.transform, false);
            
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "AD DEBUG PANEL";
            title.fontSize = 18;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.cyan;
            
            LayoutElement le = titleObj.AddComponent<LayoutElement>();
            le.preferredHeight = 25;
        }

        private void CreateStatusText()
        {
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(_panel.transform, false);
            
            _statusText = statusObj.AddComponent<TextMeshProUGUI>();
            _statusText.text = "Status: Initializing...";
            _statusText.fontSize = 14;
            _statusText.alignment = TextAlignmentOptions.Left;
            _statusText.color = Color.white;
            
            LayoutElement le = statusObj.AddComponent<LayoutElement>();
            le.preferredHeight = 20;
        }

        private void CreateTimerText()
        {
            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(_panel.transform, false);
            
            _timerText = timerObj.AddComponent<TextMeshProUGUI>();
            _timerText.text = "Next ad in: 00:00";
            _timerText.fontSize = 14;
            _timerText.alignment = TextAlignmentOptions.Left;
            _timerText.color = Color.yellow;
            
            LayoutElement le = timerObj.AddComponent<LayoutElement>();
            le.preferredHeight = 20;
        }

        private void CreateTestModeText()
        {
            GameObject testModeObj = new GameObject("TestModeText");
            testModeObj.transform.SetParent(_panel.transform, false);
            
            _testModeText = testModeObj.AddComponent<TextMeshProUGUI>();
            _testModeText.text = "Test Mode: OFF";
            _testModeText.fontSize = 14;
            _testModeText.alignment = TextAlignmentOptions.Left;
            _testModeText.color = Color.white;
            
            LayoutElement le = testModeObj.AddComponent<LayoutElement>();
            le.preferredHeight = 20;
        }

        private void CreateBannerStatusText()
        {
            GameObject bannerStatusObj = new GameObject("BannerStatusText");
            bannerStatusObj.transform.SetParent(_panel.transform, false);
            
            _bannerStatusText = bannerStatusObj.AddComponent<TextMeshProUGUI>();
            _bannerStatusText.text = "Banner: Not Showing";
            _bannerStatusText.fontSize = 14;
            _bannerStatusText.alignment = TextAlignmentOptions.Left;
            _bannerStatusText.color = Color.white;
            
            LayoutElement le = bannerStatusObj.AddComponent<LayoutElement>();
            le.preferredHeight = 20;
        }

        private void CreateShowAdButton()
        {
            GameObject buttonObj = new GameObject("ShowAdButton");
            buttonObj.transform.SetParent(_panel.transform, false);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            _showAdButton = buttonObj.AddComponent<Button>();
            _showAdButton.onClick.AddListener(OnShowAdButtonClicked);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "SHOW AD NOW";
            buttonText.fontSize = 14;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            LayoutElement le = buttonObj.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
        }

        private void CreateToggleTestModeButton()
        {
            GameObject buttonObj = new GameObject("ToggleTestModeButton");
            buttonObj.transform.SetParent(_panel.transform, false);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.6f, 0.4f, 0.2f, 1f);
            
            _toggleTestModeButton = buttonObj.AddComponent<Button>();
            _toggleTestModeButton.onClick.AddListener(OnToggleTestMode);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "TOGGLE TEST MODE";
            buttonText.fontSize = 14;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            LayoutElement le = buttonObj.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
        }

        private void CreateHelpText()
        {
            GameObject helpObj = new GameObject("HelpText");
            helpObj.transform.SetParent(_panel.transform, false);
            
            TextMeshProUGUI helpText = helpObj.AddComponent<TextMeshProUGUI>();
            helpText.text = "Press F1 to toggle panel";
            helpText.fontSize = 10;
            helpText.alignment = TextAlignmentOptions.Center;
            helpText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            LayoutElement le = helpObj.AddComponent<LayoutElement>();
            le.preferredHeight = 15;
        }

        private void UpdateStatusDisplay()
        {
            if (_statusText == null || _timerText == null || _testModeText == null || _bannerStatusText == null) return;

            if (InterstitialAdManager.Instance == null)
            {
                _statusText.text = "Status: Ad Manager not found!";
                _statusText.color = Color.red;
                return;
            }

            float timeUntilNextAd = InterstitialAdManager.Instance.GetMatchesUntilNextAd();
            
            int minutes = Mathf.FloorToInt(timeUntilNextAd / 60f);
            int seconds = Mathf.FloorToInt(timeUntilNextAd % 60f);
            _timerText.text = $"Next ad in: {minutes:00}:{seconds:00}";

            _statusText.text = timeUntilNextAd <= 0 ? "Status: Ready to show ad" : "Status: Counting down...";
            _statusText.color = timeUntilNextAd <= 0 ? Color.green : Color.yellow;

            bool isTestMode = InterstitialAdManager.Instance.IsTestModeEnabled();
            _testModeText.text = isTestMode ? "Test Mode: ON (30s)" : "Test Mode: OFF (3min)";
            _testModeText.color = isTestMode ? Color.cyan : Color.white;

            if (BannerAdManager.Instance != null)
            {
                bool isBannerShowing = BannerAdManager.Instance.IsBannerShowing();
                _bannerStatusText.text = isBannerShowing ? "Banner: ✅ Showing" : "Banner: ❌ Hidden";
                _bannerStatusText.color = isBannerShowing ? Color.green : Color.gray;
            }
        }

        private void OnShowAdButtonClicked()
        {
            if (InterstitialAdManager.Instance != null)
            {
                Debug.Log("[AdDebugUI] Manual ad trigger requested");
                InterstitialAdManager.Instance.ForceShowAd();
            }
            else
            {
                Debug.LogError("[AdDebugUI] InterstitialAdManager not found!");
            }
        }

        private void OnToggleTestMode()
        {
            if (InterstitialAdManager.Instance != null)
            {
                InterstitialAdManager.Instance.ToggleTestMode();
            }
        }

        private void OnToggleBanner()
        {
            if (BannerAdManager.Instance == null)
            {
                Debug.LogError("[AdDebugUI] BannerAdManager not found!");
                return;
            }

            if (BannerAdManager.Instance.IsBannerShowing())
            {
                Debug.Log("[AdDebugUI] Hiding banner");
                BannerAdManager.Instance.HideBanner();
            }
            else
            {
                Debug.Log("[AdDebugUI] Showing banner");
                BannerAdManager.Instance.ShowBanner();
            }
        }

        private void TogglePanelVisibility()
        {
            _isVisible = !_isVisible;
            if (_panel != null)
            {
                _panel.SetActive(_isVisible);
            }
        }
    }
}

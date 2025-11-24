using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Views
{
    public class LoadingManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        
        [Header("Tips")]
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private string[] tipTranslationKeys;
        [SerializeField] private float tipChangeInterval = 3.5f;
        
        [Header("Settings")]
        [SerializeField] private float minimumLoadingTime = 2f;
        [SerializeField] private float progressBarSpeed = 3f;
        [SerializeField] private string loadingTextKey = "loading";

        private const float FAKE_PROGRESS_MAX = 0.95f;
        
        private static string sceneToLoad;
        private float _displayedProgress = 0f;
        private float _targetProgress = 0f;

        public static void LoadScene(string sceneName)
        {
            sceneToLoad = sceneName;
            SceneManager.LoadScene("Loading");
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                sceneToLoad = "Gameplay Scene";
            }

            ShowBannerIfAvailable();

            StartCoroutine(LoadSceneAsync());
            StartCoroutine(RotateTips());
        }

        private void ShowBannerIfAvailable()
        {
            if (Ads.BannerAdManager.Instance != null)
            {
                Ads.BannerAdManager.Instance.ShowBanner();
            }
        }

        private IEnumerator LoadSceneAsync()
        {
            var startTime = Time.time;

            var operation = SceneManager.LoadSceneAsync(sceneToLoad);
            operation.allowSceneActivation = false;

            bool canComplete = false;

            while (!operation.isDone)
            {
                var elapsed = Time.time - startTime;
                var realProgress = Mathf.Clamp01(operation.progress / 0.9f);

                if (realProgress >= 0.9f && elapsed >= minimumLoadingTime)
                {
                    canComplete = true;
                    _targetProgress = 1f;
                }
                else
                {
                    var timeBasedProgress = elapsed / minimumLoadingTime;
                    _targetProgress = Mathf.Clamp01(timeBasedProgress) * FAKE_PROGRESS_MAX;
                }
                
                _displayedProgress = Mathf.Lerp(_displayedProgress, _targetProgress, Time.deltaTime * progressBarSpeed);

                if (progressBar != null)
                {
                    progressBar.value = _displayedProgress;
                }

                if (loadingText != null)
                {
                    var loadingLabel = Localizer.Instance.Tr(loadingTextKey, "Loading");
                    loadingText.text = $"{loadingLabel}... {Mathf.RoundToInt(_displayedProgress * 100)}%";
                }

                if (canComplete && _displayedProgress >= 0.99f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        private IEnumerator RotateTips()
        {
            while (true)
            {
                ShowRandomTip();
                yield return new WaitForSeconds(tipChangeInterval);
            }
        }

        private void ShowRandomTip()
        {
            if (tipText == null || tipTranslationKeys == null || tipTranslationKeys.Length == 0)
                return;
                
            var randomKey = tipTranslationKeys[Random.Range(0, tipTranslationKeys.Length)];
            tipText.text = Localizer.Instance.Tr(randomKey, randomKey);
        }
    }
}

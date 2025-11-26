using UnityEngine;

namespace Core
{
    public class ApplicationManager : MonoBehaviour
    {
        private const int TARGET_FRAME_RATE = 60;
        
        private static ApplicationManager _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeApplication();
        }

        private void InitializeApplication()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;
            
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Input.multiTouchEnabled = false;

#if !UNITY_EDITOR
            Debug.unityLogger.logEnabled = Debug.isDebugBuild;
#endif

#if UNITY_ANDROID
            if (SystemInfo.processorFrequency > 2000)
            {
                QualitySettings.vSyncCount = 0;
            }
#endif
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                AudioListener.pause = true;
            }
            else
            {
                AudioListener.pause = false;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Time.timeScale = 0f;
            }
            else
            {
                if (!InterstitialAdIsShowing())
                {
                    Time.timeScale = 1f;
                }
            }
        }

        private bool InterstitialAdIsShowing()
        {
            var adManager = Ads.InterstitialAdManager.Instance;
            return adManager != null && adManager.gameObject.activeInHierarchy;
        }
    }
}

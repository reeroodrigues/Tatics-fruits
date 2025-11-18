using UnityEngine;

namespace Ads
{
    public class AdInitializer : MonoBehaviour
    {
        [SerializeField] private AdProviderType providerType = AdProviderType.AdMob;

        private void Awake()
        {
            if (providerType == AdProviderType.AdMob)
            {
                SetupGoogleMobileAdsInitializer();
            }
        }

        private void Start()
        {
            if (providerType == AdProviderType.AdMob)
            {
                StartCoroutine(InitializeAdsAfterDelay());
            }
            else
            {
                InitializeAds();
            }
        }

        private void SetupGoogleMobileAdsInitializer()
        {
            if (GetComponent<GoogleMobileAdsInitializer>() == null)
            {
                gameObject.AddComponent<GoogleMobileAdsInitializer>();
            }
        }

        private System.Collections.IEnumerator InitializeAdsAfterDelay()
        {
            yield return new WaitForSeconds(1f);

            while (!GoogleMobileAdsInitializer.IsInitialized())
            {
                yield return new WaitForSeconds(0.5f);
            }

            InitializeAds();
        }

        private void InitializeAds()
        {
            IInterstitialAdProvider provider = providerType switch
            {
                AdProviderType.TestProvider => CreateTestProvider(),
                AdProviderType.UnityAds => new UnityAdsInterstitialProvider(),
                AdProviderType.AdMob => new AdMobInterstitialProvider(),
                _ => null
            };

            if (provider != null && InterstitialAdManager.Instance != null)
            {
                InterstitialAdManager.Instance.SetAdProvider(provider);
                Debug.Log($"[AdInitializer] Ad provider initialized: {providerType}");
            }
            else
            {
                Debug.LogError("[AdInitializer] Failed to initialize ad provider or InterstitialAdManager not found.");
            }
        }

        private IInterstitialAdProvider CreateTestProvider()
        {
            TestAdProvider testProvider = gameObject.AddComponent<TestAdProvider>();
            return testProvider;
        }

        private enum AdProviderType
        {
            TestProvider,
            UnityAds,
            AdMob
        }
    }
}

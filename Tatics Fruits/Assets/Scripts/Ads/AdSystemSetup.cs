using UnityEngine;

namespace Ads
{
    public class AdSystemSetup : MonoBehaviour
    {
        [Header("Setup Instructions")]
        [Tooltip("This script helps you set up the ad system. Add this to a GameObject in your scene.")]
        [SerializeField] private bool autoSetup = true;

        [Header("Components (Auto-filled)")]
        [SerializeField] private InterstitialAdManager interstitialAdManager;
        [SerializeField] private BannerAdManager bannerAdManager;
        [SerializeField] private AdInitializer adInitializer;
        [SerializeField] private AdDebugUI debugUI;

        private void Awake()
        {
            if (autoSetup)
            {
                SetupComponents();
            }
        }

        private void SetupComponents()
        {
            interstitialAdManager = GetComponent<InterstitialAdManager>();
            if (interstitialAdManager == null)
            {
                interstitialAdManager = gameObject.AddComponent<InterstitialAdManager>();
                Debug.Log("[AdSystemSetup] Added InterstitialAdManager component");
            }

            bannerAdManager = GetComponent<BannerAdManager>();
            if (bannerAdManager == null)
            {
                bannerAdManager = gameObject.AddComponent<BannerAdManager>();
                Debug.Log("[AdSystemSetup] Added BannerAdManager component");
            }

            adInitializer = GetComponent<AdInitializer>();
            if (adInitializer == null)
            {
                adInitializer = gameObject.AddComponent<AdInitializer>();
                Debug.Log("[AdSystemSetup] Added AdInitializer component");
            }

            debugUI = GetComponent<AdDebugUI>();
            if (debugUI == null)
            {
                debugUI = gameObject.AddComponent<AdDebugUI>();
                Debug.Log("[AdSystemSetup] Added AdDebugUI component");
            }

            Debug.Log("[AdSystemSetup] Ad system setup complete! Check the Inspector to configure your ad provider.");
        }

        private void OnValidate()
        {
            interstitialAdManager = GetComponent<InterstitialAdManager>();
            bannerAdManager = GetComponent<BannerAdManager>();
            adInitializer = GetComponent<AdInitializer>();
            debugUI = GetComponent<AdDebugUI>();
        }
    }
}

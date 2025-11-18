using UnityEngine;

namespace Ads
{
    public class AdSystemSetup : MonoBehaviour
    {
        [Header("Setup Instructions")]
        [Tooltip("This script helps you set up the ad system. Add this to a GameObject in your scene.")]
        [SerializeField] private bool autoSetup = true;

        [Header("Components (Auto-filled)")]
        [SerializeField] private InterstitialAdManager adManager;
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
            adManager = GetComponent<InterstitialAdManager>();
            if (adManager == null)
            {
                adManager = gameObject.AddComponent<InterstitialAdManager>();
                Debug.Log("[AdSystemSetup] Added InterstitialAdManager component");
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
            adManager = GetComponent<InterstitialAdManager>();
            adInitializer = GetComponent<AdInitializer>();
            debugUI = GetComponent<AdDebugUI>();
        }
    }
}

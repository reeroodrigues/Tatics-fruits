using UnityEngine;
using UnityEditor;

namespace Ads.Editor
{
    [InitializeOnLoad]
    public class BannerAdTester
    {
        static BannerAdTester()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EnsureAdSystemExists();
            }
        }

        private static void EnsureAdSystemExists()
        {
            var adSystem = GameObject.Find("AdSystem");
            
            if (adSystem == null)
            {
                Debug.LogWarning("[BannerAdTester] AdSystem not found in scene. Creating temporary AdSystem for testing...");
                CreateTestAdSystem();
            }
        }

        private static void CreateTestAdSystem()
        {
            var adSystemGO = new GameObject("AdSystem");
            Object.DontDestroyOnLoad(adSystemGO);

            var bannerManager = adSystemGO.AddComponent<BannerAdManager>();
            var adInitializer = adSystemGO.AddComponent<AdInitializer>();

            SetBannerManagerDefaults(bannerManager);

            Debug.Log("[BannerAdTester] ✅ Created temporary AdSystem for testing. Banner ads should now work!");
        }

        private static void SetBannerManagerDefaults(BannerAdManager manager)
        {
            var serializedObject = new SerializedObject(manager);
            
            serializedObject.FindProperty("showBannerOnStart").boolValue = true;
            serializedObject.FindProperty("enableBanners").boolValue = true;
            
            var scenesArray = serializedObject.FindProperty("scenesWithBanners");
            scenesArray.ClearArray();
            scenesArray.InsertArrayElementAtIndex(0);
            scenesArray.GetArrayElementAtIndex(0).stringValue = "MainMenu";
            scenesArray.InsertArrayElementAtIndex(1);
            scenesArray.GetArrayElementAtIndex(1).stringValue = "Loading";
            
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Ads/Force Create AdSystem")]
        private static void ForceCreateAdSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[BannerAdTester] This tool only works in Play Mode!");
                return;
            }

            var existing = GameObject.Find("AdSystem");
            if (existing != null)
            {
                Debug.LogWarning("[BannerAdTester] Destroying existing AdSystem...");
                Object.DestroyImmediate(existing);
            }

            CreateTestAdSystem();
        }

        [MenuItem("Tools/Ads/Log Ad System Status")]
        private static void LogAdSystemStatus()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[BannerAdTester] This tool only works in Play Mode!");
                return;
            }

            var adSystem = GameObject.Find("AdSystem");
            
            if (adSystem == null)
            {
                Debug.LogError("[BannerAdTester] ❌ AdSystem NOT FOUND in scene!");
                Debug.Log("[BannerAdTester] Use 'Tools > Ads > Force Create AdSystem' to create one.");
                return;
            }

            Debug.Log($"[BannerAdTester] ✅ AdSystem found: {adSystem.name}");
            
            var bannerManager = adSystem.GetComponent<BannerAdManager>();
            if (bannerManager != null)
            {
                Debug.Log($"[BannerAdTester] BannerAdManager: Found");
                Debug.Log($"[BannerAdTester] - IsBannerShowing: {bannerManager.IsBannerShowing()}");
            }
            else
            {
                Debug.LogError("[BannerAdTester] ❌ BannerAdManager component NOT FOUND!");
            }

            var adInitializer = adSystem.GetComponent<AdInitializer>();
            if (adInitializer != null)
            {
                Debug.Log($"[BannerAdTester] AdInitializer: Found");
            }
            else
            {
                Debug.LogError("[BannerAdTester] ❌ AdInitializer component NOT FOUND!");
            }

            var googleAdsInit = adSystem.GetComponent<GoogleMobileAdsInitializer>();
            if (googleAdsInit != null)
            {
                Debug.Log($"[BannerAdTester] GoogleMobileAdsInitializer: Found");
                Debug.Log($"[BannerAdTester] - IsInitialized: {GoogleMobileAdsInitializer.IsInitialized()}");
            }
            else
            {
                Debug.LogWarning("[BannerAdTester] ⚠️ GoogleMobileAdsInitializer component NOT FOUND (will be added automatically)");
            }
        }
    }
}

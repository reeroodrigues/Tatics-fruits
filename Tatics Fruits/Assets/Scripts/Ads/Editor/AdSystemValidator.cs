using UnityEditor;
using UnityEngine;

namespace Ads.Editor
{
    [InitializeOnLoad]
    public class AdSystemValidator
    {
        static AdSystemValidator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                ValidateAdSystem();
            }
        }

        private static void ValidateAdSystem()
        {
            InterstitialAdManager interstitialManager = Object.FindFirstObjectByType<InterstitialAdManager>();
            BannerAdManager bannerManager = Object.FindFirstObjectByType<BannerAdManager>();
            
            if (interstitialManager == null && bannerManager == null)
            {
                Debug.LogWarning("[AdSystemValidator] No Ad Managers found in scene!");
                return;
            }

            if (interstitialManager != null)
            {
                AdDebugUI debugUI = interstitialManager.GetComponent<AdDebugUI>();
                if (debugUI == null)
                {
                    Debug.LogWarning($"[AdSystemValidator] No AdDebugUI found on {interstitialManager.gameObject.name}. You won't see the debug panel!");
                }

                AdInitializer initializer = interstitialManager.GetComponent<AdInitializer>();
                if (initializer == null)
                {
                    Debug.LogWarning($"[AdSystemValidator] No AdInitializer found on {interstitialManager.gameObject.name}. Ads won't initialize!");
                }
            }

            if (bannerManager != null)
            {
                Debug.Log("[AdSystemValidator] ✅ BannerAdManager found - Banner ads ready!");
            }
        }
    }

    public class AdSystemQuickFix
    {
        [MenuItem("Tools/Ads/Fix Ad System Setup")]
        public static void FixAdSystemSetup()
        {
            InterstitialAdManager interstitialManager = Object.FindFirstObjectByType<InterstitialAdManager>();
            BannerAdManager bannerManager = Object.FindFirstObjectByType<BannerAdManager>();
            
            GameObject targetObject = null;
            
            if (interstitialManager != null)
            {
                targetObject = interstitialManager.gameObject;
            }
            else if (bannerManager != null)
            {
                targetObject = bannerManager.gameObject;
            }
            
            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("Ad System Fix", 
                    "No Ad Manager found in the current scene!\n\nPlease add InterstitialAdManager or BannerAdManager to a GameObject first.", 
                    "OK");
                return;
            }

            int componentsAdded = 0;

            if (targetObject.GetComponent<InterstitialAdManager>() == null)
            {
                targetObject.AddComponent<InterstitialAdManager>();
                componentsAdded++;
                Debug.Log($"[AdSystemQuickFix] Added InterstitialAdManager to {targetObject.name}");
            }

            if (targetObject.GetComponent<BannerAdManager>() == null)
            {
                targetObject.AddComponent<BannerAdManager>();
                componentsAdded++;
                Debug.Log($"[AdSystemQuickFix] Added BannerAdManager to {targetObject.name}");
            }

            if (targetObject.GetComponent<AdInitializer>() == null)
            {
                targetObject.AddComponent<AdInitializer>();
                componentsAdded++;
                Debug.Log($"[AdSystemQuickFix] Added AdInitializer to {targetObject.name}");
            }

            if (targetObject.GetComponent<AdDebugUI>() == null)
            {
                targetObject.AddComponent<AdDebugUI>();
                componentsAdded++;
                Debug.Log($"[AdSystemQuickFix] Added AdDebugUI to {targetObject.name}");
            }

            if (componentsAdded > 0)
            {
                EditorUtility.DisplayDialog("Ad System Fixed!", 
                    $"Added {componentsAdded} missing component(s) to {targetObject.name}.\n\n" +
                    "✅ Your ad system is now ready!\n\n" +
                    "Both interstitial and banner ads are configured.\n\n" +
                    "Press Play to see the debug panel in the top-left corner.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Ad System Check", 
                    $"All components are already present on {targetObject.name}!\n\n" +
                    "✅ InterstitialAdManager\n" +
                    "✅ BannerAdManager\n" +
                    "✅ AdInitializer\n" +
                    "✅ AdDebugUI\n\n" +
                    "Your ad system is ready to use!", 
                    "OK");
            }
        }

        [MenuItem("Tools/Ads/Enable Test Mode")]
        public static void EnableTestMode()
        {
            InterstitialAdManager manager = Object.FindFirstObjectByType<InterstitialAdManager>();
            
            if (manager == null)
            {
                EditorUtility.DisplayDialog("Test Mode", 
                    "No InterstitialAdManager found in scene!", 
                    "OK");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            SerializedProperty testModeProp = so.FindProperty("testMode");
            
            if (testModeProp != null)
            {
                bool currentValue = testModeProp.boolValue;
                testModeProp.boolValue = !currentValue;
                so.ApplyModifiedProperties();
                
                string status = testModeProp.boolValue ? "ENABLED (30 second intervals)" : "DISABLED (3 minute intervals)";
                Debug.Log($"[AdSystemQuickFix] Test Mode {status}");
                
                EditorUtility.DisplayDialog("Test Mode", 
                    $"Test Mode is now: {status}", 
                    "OK");
            }
        }
    }
}

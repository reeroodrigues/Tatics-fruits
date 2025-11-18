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
            InterstitialAdManager manager = Object.FindFirstObjectByType<InterstitialAdManager>();
            
            if (manager == null)
            {
                Debug.LogWarning("[AdSystemValidator] No InterstitialAdManager found in scene!");
                return;
            }

            AdDebugUI debugUI = manager.GetComponent<AdDebugUI>();
            if (debugUI == null)
            {
                Debug.LogWarning($"[AdSystemValidator] No AdDebugUI found on {manager.gameObject.name}. You won't see the debug panel!");
                Debug.LogWarning($"[AdSystemValidator] Add the AdDebugUI component to {manager.gameObject.name} to see debug controls.");
            }

            AdInitializer initializer = manager.GetComponent<AdInitializer>();
            if (initializer == null)
            {
                Debug.LogWarning($"[AdSystemValidator] No AdInitializer found on {manager.gameObject.name}. Ads won't initialize!");
            }
        }
    }

    public class AdSystemQuickFix
    {
        [MenuItem("Tools/Ads/Fix Ad System Setup")]
        public static void FixAdSystemSetup()
        {
            InterstitialAdManager manager = Object.FindFirstObjectByType<InterstitialAdManager>();
            
            if (manager == null)
            {
                EditorUtility.DisplayDialog("Ad System Fix", 
                    "No InterstitialAdManager found in the current scene!\n\nPlease add an InterstitialAdManager to a GameObject first.", 
                    "OK");
                return;
            }

            GameObject go = manager.gameObject;
            int componentsAdded = 0;

            if (go.GetComponent<AdInitializer>() == null)
            {
                go.AddComponent<AdInitializer>();
                componentsAdded++;
                Debug.Log($"[AdSystemQuickFix] Added AdInitializer to {go.name}");
            }

            if (go.GetComponent<AdDebugUI>() == null)
            {
                go.AddComponent<AdDebugUI>();
                componentsAdded++;
                Debug.Log($"[AdSystemQuickFix] Added AdDebugUI to {go.name}");
            }

            if (componentsAdded > 0)
            {
                EditorUtility.DisplayDialog("Ad System Fixed!", 
                    $"Added {componentsAdded} missing component(s) to {go.name}.\n\n" +
                    "✅ Your ad system is now ready!\n\n" +
                    "Press Play to see the debug panel in the top-left corner.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Ad System Check", 
                    $"All components are already present on {go.name}!\n\n" +
                    "✅ InterstitialAdManager\n" +
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

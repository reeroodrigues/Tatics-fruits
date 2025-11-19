using UnityEditor;
using UnityEngine;

namespace Ads.Editor
{
    public class GooglePlaySetupHelper : EditorWindow
    {
        private const string BANNER_AD_UNIT_ID = "ca-app-pub-8609532543875876/5682573689";
        private const string APP_ID = "ca-app-pub-8609532543875876~3807758387";
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Ads/Google Play Setup Checker")]
        public static void ShowWindow()
        {
            GetWindow<GooglePlaySetupHelper>("Google Play Setup");
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("Google Play Internal Testing Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawAdConfiguration();
            EditorGUILayout.Space();
            
            DrawPlayerSettings();
            EditorGUILayout.Space();
            
            DrawBuildSettings();
            EditorGUILayout.Space();
            
            DrawQuickActions();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawAdConfiguration()
        {
            GUILayout.Label("Ad Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Banner Ad Unit ID: " + BANNER_AD_UNIT_ID + "\n" +
                "App ID: " + APP_ID,
                MessageType.Info);
            
            var adSystem = FindFirstObjectByType<AdSystemSetup>();
            if (adSystem != null)
            {
                EditorGUILayout.HelpBox("✅ AdSystem found in scene", MessageType.Info);
                
                var interstitialManager = adSystem.GetComponent<InterstitialAdManager>();
                if (interstitialManager != null)
                {
                    var enableAds = new SerializedObject(interstitialManager).FindProperty("enableAds");
                    if (!enableAds.boolValue)
                    {
                        EditorGUILayout.HelpBox("✅ Interstitial ads are DISABLED (correct for internal testing)", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("⚠️ Interstitial ads are ENABLED. Recommended to disable for internal testing.", MessageType.Warning);
                        if (GUILayout.Button("Disable Interstitial Ads"))
                        {
                            enableAds.boolValue = false;
                            enableAds.serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(interstitialManager);
                        }
                    }
                }
                
                var bannerManager = adSystem.GetComponent<BannerAdManager>();
                if (bannerManager != null)
                {
                    var enableBanners = new SerializedObject(bannerManager).FindProperty("enableBanners");
                    if (enableBanners.boolValue)
                    {
                        EditorGUILayout.HelpBox("✅ Banner ads are ENABLED", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("⚠️ Banner ads are DISABLED", MessageType.Warning);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("❌ AdSystem not found in current scene. Open MainMenu scene.", MessageType.Error);
            }
        }
        
        private void DrawPlayerSettings()
        {
            GUILayout.Label("Player Settings (Android)", EditorStyles.boldLabel);
            
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            EditorGUILayout.LabelField("Package Name:", packageName);
            
            if (string.IsNullOrEmpty(packageName) || packageName == "com.DefaultCompany.DefaultProject")
            {
                EditorGUILayout.HelpBox("⚠️ Package name not set or using default. Click button to open Player Settings.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Package name is set", MessageType.Info);
            }
            
            EditorGUILayout.LabelField("Version:", PlayerSettings.bundleVersion);
            EditorGUILayout.LabelField("Version Code:", PlayerSettings.Android.bundleVersionCode.ToString());
            
            int minSDK = (int)PlayerSettings.Android.minSdkVersion;
            int targetSDK = (int)PlayerSettings.Android.targetSdkVersion;
            
            EditorGUILayout.LabelField("Min API Level:", minSDK.ToString());
            if (minSDK < 24)
            {
                EditorGUILayout.HelpBox("⚠️ Minimum API level should be 24 or higher for Google Play", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Minimum API level is acceptable", MessageType.Info);
            }
            
            EditorGUILayout.LabelField("Target API Level:", targetSDK.ToString());
            if (targetSDK < 33)
            {
                EditorGUILayout.HelpBox("⚠️ Target API level should be 33 or higher for Google Play", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Target API level meets Google Play requirements", MessageType.Info);
            }
            
            EditorGUILayout.LabelField("Scripting Backend:", PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString());
            
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                EditorGUILayout.HelpBox("⚠️ Scripting backend should be IL2CPP for ARM64 support", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Scripting backend is IL2CPP", MessageType.Info);
            }
            
            var targetArchitectures = PlayerSettings.Android.targetArchitectures;
            bool hasARM64 = (targetArchitectures & AndroidArchitecture.ARM64) != 0;
            
            if (!hasARM64)
            {
                EditorGUILayout.HelpBox("❌ ARM64 architecture is not enabled. Required for Google Play!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ ARM64 architecture is enabled", MessageType.Info);
            }
            
            if (GUILayout.Button("Open Player Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
        }
        
        private void DrawBuildSettings()
        {
            GUILayout.Label("Build Settings", EditorStyles.boldLabel);
            
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorGUILayout.HelpBox("❌ Current platform is not Android. Switch to Android platform.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Platform is set to Android", MessageType.Info);
            }
            
            if (EditorUserBuildSettings.buildAppBundle)
            {
                EditorGUILayout.HelpBox("✅ Build App Bundle is ENABLED (required for Google Play)", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Build App Bundle is DISABLED. Enable it for Google Play submission.", MessageType.Warning);
            }
            
            if (GUILayout.Button("Open Build Settings"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
        }
        
        private void DrawQuickActions()
        {
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("View Full Setup Guide"))
            {
                var guide = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/Ads/GOOGLE_PLAY_INTERNAL_TESTING_SETUP.md");
                if (guide != null)
                {
                    Selection.activeObject = guide;
                    EditorGUIUtility.PingObject(guide);
                }
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "Ready to build?\n\n" +
                "1. Verify all checkmarks above are green\n" +
                "2. Open Build Settings\n" +
                "3. Enable 'Build App Bundle'\n" +
                "4. Click 'Build'\n" +
                "5. Upload .aab to Google Play Console",
                MessageType.Info);
        }
    }
}

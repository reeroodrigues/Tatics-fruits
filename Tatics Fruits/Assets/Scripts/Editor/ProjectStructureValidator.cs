using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ProjectStructureValidator
    {
        [MenuItem("Tools/Validate Project Structure")]
        public static void ValidateStructure()
        {
            Debug.Log("=== PROJECT STRUCTURE VALIDATION ===");

            string[] requiredFolders = new[]
            {
                "Assets/Scripts",
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Materials",
                "Assets/Resources"
            };

            int missingCount = 0;
            foreach (string folder in requiredFolders)
            {
                if (AssetDatabase.IsValidFolder(folder))
                {
                    Debug.Log($"✅ {folder}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Missing: {folder}");
                    missingCount++;
                }
            }

            if (missingCount == 0)
            {
                Debug.Log("✅ All required project folders are present");
            }
            else
            {
                Debug.LogWarning($"⚠️ {missingCount} required folders are missing");
            }

            ValidateScenes();
            ValidateInputActions();

            Debug.Log("====================================");
        }

        private static void ValidateScenes()
        {
            Debug.Log("\n=== SCENE VALIDATION ===");

            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError("❌ No scenes in build settings!");
                return;
            }

            int enabledScenes = 0;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes++;
                    Debug.Log($"✅ Scene {enabledScenes}: {scene.path}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Disabled: {scene.path}");
                }
            }

            Debug.Log($"Total enabled scenes: {enabledScenes}");
        }

        private static void ValidateInputActions()
        {
            Debug.Log("\n=== INPUT SYSTEM VALIDATION ===");

            string inputActionsPath = "Assets/InputSystem_Actions.inputactions";
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(inputActionsPath) != null)
            {
                Debug.Log($"✅ Input actions found: {inputActionsPath}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Input actions not found at: {inputActionsPath}");
            }
        }

        [MenuItem("Tools/Clean Project")]
        public static void CleanProject()
        {
            Debug.Log("=== CLEANING PROJECT ===");

            AssetDatabase.DeleteAsset("Library/ShaderCache");
            AssetDatabase.DeleteAsset("Temp");

            Debug.Log("✅ Cleaned shader cache and temp files");
            Debug.Log("Tip: Close Unity and delete the Library folder for a complete clean");

            AssetDatabase.Refresh();
        }
    }
}

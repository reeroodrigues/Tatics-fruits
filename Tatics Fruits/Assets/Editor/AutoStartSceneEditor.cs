#if  UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class AutoStartSceneEditor
    {
        private static string _previousScene = "";
        private static bool _sceneLoad = false;

        private const string START_SCENE_PATH = "Assets/Scenes/StartScene.unity";

        static AutoStartSceneEditor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                _previousScene = EditorSceneManager.GetActiveScene().path;

                if (_previousScene != START_SCENE_PATH)
                    _sceneLoad = EditorSceneManager.OpenScene(START_SCENE_PATH).IsValid();
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (_sceneLoad && !string.IsNullOrEmpty(_previousScene))
                    EditorSceneManager.OpenScene(_previousScene);
            }
        }
    }
}
#endif
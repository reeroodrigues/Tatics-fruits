using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameSessionTracker : MonoBehaviour
    {
        private static GameSessionTracker _instance;

        public static GameSessionTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new  GameObject("GameSessionTracker");
                    _instance = go.AddComponent<GameSessionTracker>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        public static bool HasPlayedGameplayThisSession { get; private set; }
        public static bool JustReturnedFromGameplay { get; private set; }

        private string _previousScene;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Gameplay Scene")
            {
                HasPlayedGameplayThisSession = true;
                JustReturnedFromGameplay = false;
            }
            else if(scene.name == "MainMenu" && _previousScene == "Gameplay Scene")
            {
                JustReturnedFromGameplay = true;
            }
            else if(scene.name == "MainMenu")
            {
                JustReturnedFromGameplay = false;
            }

            _previousScene = scene.name;
        }

        public static void ResetReturnedFlag()
        {
            JustReturnedFromGameplay = false;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
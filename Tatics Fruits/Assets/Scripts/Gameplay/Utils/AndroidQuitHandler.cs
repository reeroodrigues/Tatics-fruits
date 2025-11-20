using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AndroidQuitHandler : MonoBehaviour
{
    private static AndroidQuitHandler _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void Update()
    {
        bool backPressed = false;

        // Sistema antigo (Input Manager)
        if (Input.GetKeyDown(KeyCode.Escape))
            backPressed = true;

        // Sistema novo (Input System)
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            backPressed = true;
        #endif

        if (backPressed)
        {
            Debug.Log("Back button pressionado - encerrando o jogo");
            Application.Quit();

            // Se quiser **minimizar** em vez de fechar:
            // using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            // using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            // {
            //     activity.Call<bool>("moveTaskToBack", true);
            // }
        }
    }
#endif
}
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Core
{
    public class PermissionsManager : MonoBehaviour
    {
        private static PermissionsManager _instance;

        public static PermissionsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[PermissionsManager]");
                    _instance = go.AddComponent<PermissionsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

#if UNITY_ANDROID
        public void RequestInternetPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                DebugLogger.Log("[PermissionsManager] Requesting external storage permission for save data");
            }
        }

        public bool HasInternetPermission()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
#endif
    }
}

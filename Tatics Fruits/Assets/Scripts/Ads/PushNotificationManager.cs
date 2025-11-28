using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.PushNotifications;
using UnityEngine;
using System;

namespace Ads
{
    public class PushNotificationManager : MonoBehaviour
    {
        async void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            Debug.Log("[PushNotification] Starting initialization...");
            
            try
            {
                Debug.Log("[PushNotification] Initializing Unity Services...");
                await UnityServices.InitializeAsync();
                Debug.Log("[PushNotification] Unity Services initialized successfully!");

                Debug.Log("[PushNotification] Starting Analytics data collection...");
                AnalyticsService.Instance.StartDataCollection();
                Debug.Log("[PushNotification] Analytics started!");

                Debug.Log("[PushNotification] Registering for push notifications...");
                string pushToken = await PushNotificationsService.Instance.RegisterForPushNotificationsAsync();
                Debug.Log($"[PushNotification] SUCCESS! Token: {pushToken}");

                PushNotificationsService.Instance.OnRemoteNotificationReceived += OnNotificationReceived;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PushNotification] FAILED: {e.Message}");
                Debug.LogError($"[PushNotification] Exception Type: {e.GetType().Name}");
                
                if (e.InnerException != null)
                {
                    Debug.LogError($"[PushNotification] Inner Exception: {e.InnerException.Message}");
                    Debug.LogError($"[PushNotification] Inner Exception Type: {e.InnerException.GetType().Name}");
                }
                
                Debug.LogError($"[PushNotification] Full Stack Trace: {e}");
            }
#else
            Debug.Log("Push notifications only work on Android and iOS devices.");
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        void OnNotificationReceived(System.Collections.Generic.IDictionary<string, object> notificationData)
        {
            Debug.Log("[PushNotification] Received a notification!");
            foreach (var entry in notificationData)
            {
                Debug.Log($"[PushNotification] Data: {entry.Key} = {entry.Value}");
            }
        }
#endif

        void OnDestroy()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (PushNotificationsService.Instance != null)
            {
                PushNotificationsService.Instance.OnRemoteNotificationReceived -= OnNotificationReceived;
            }
#endif
        }
    }
}

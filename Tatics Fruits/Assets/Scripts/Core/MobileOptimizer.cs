using UnityEngine;

namespace Core
{
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSync = true;

        [Header("Memory Settings")]
        [SerializeField] private bool unloadUnusedAssets = true;
        [SerializeField] private float unloadInterval = 30f;

        private float _unloadTimer;

        private void Awake()
        {
            if (enableAutoOptimization)
            {
                ApplyMobileOptimizations();
            }
        }

        private void ApplyMobileOptimizations()
        {
            Application.targetFrameRate = targetFrameRate;

            if (disableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_ANDROID
            QualitySettings.antiAliasing = 0;
            
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            
            if (SystemInfo.systemMemorySize < 3072)
            {
                QualitySettings.globalTextureMipmapLimit = 1;
                DebugLogger.Log("[MobileOptimizer] Low memory device detected, reducing texture quality");
            }
#endif

            DebugLogger.Log($"[MobileOptimizer] Mobile optimizations applied. Target FPS: {targetFrameRate}");
        }

        private void Update()
        {
            if (unloadUnusedAssets)
            {
                _unloadTimer += Time.deltaTime;
                if (_unloadTimer >= unloadInterval)
                {
                    _unloadTimer = 0f;
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
        }

        [ContextMenu("Force Unload Unused Assets")]
        public void ForceUnloadAssets()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            DebugLogger.Log("[MobileOptimizer] Manually unloaded unused assets");
        }

        [ContextMenu("Print System Info")]
        public void PrintSystemInfo()
        {
            DebugLogger.Log("=== DEVICE INFORMATION ===");
            DebugLogger.Log($"Device Model: {SystemInfo.deviceModel}");
            DebugLogger.Log($"Device Type: {SystemInfo.deviceType}");
            DebugLogger.Log($"OS: {SystemInfo.operatingSystem}");
            DebugLogger.Log($"Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
            DebugLogger.Log($"System Memory: {SystemInfo.systemMemorySize} MB");
            DebugLogger.Log($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB");
            DebugLogger.Log($"Graphics Device: {SystemInfo.graphicsDeviceName}");
            DebugLogger.Log($"Max Texture Size: {SystemInfo.maxTextureSize}");
            DebugLogger.Log($"Screen Resolution: {Screen.width}x{Screen.height} @ {Screen.dpi} DPI");
            DebugLogger.Log($"Safe Area: {Screen.safeArea}");
            DebugLogger.Log("========================");
        }
    }
}

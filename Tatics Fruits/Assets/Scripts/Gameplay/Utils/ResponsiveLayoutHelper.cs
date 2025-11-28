using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Utils
{
    public class ResponsiveLayoutHelper : MonoBehaviour
    {
        [Header("Device Type Detection")]
        [SerializeField] private DeviceType currentDeviceType = DeviceType.Phone;
        
        [Header("Layout Adjustments")]
        [SerializeField] private LayoutProfile phoneLayout;
        [SerializeField] private LayoutProfile tabletLayout;
        [SerializeField] private LayoutProfile landscapeLayout;
        
        [Header("Auto-Apply")]
        [SerializeField] private bool applyOnStart = true;
        
        private RectTransform rectTransform;
        
        public enum DeviceType
        {
            Phone,
            Tablet,
            Landscape
        }
        
        [System.Serializable]
        public class LayoutProfile
        {
            public bool enabled = true;
            public Vector2 sizeDelta = Vector2.zero;
            public Vector2 anchoredPosition = Vector2.zero;
            public Vector2 scale = Vector2.one;
            public bool modifyAnchors = false;
            public Vector2 anchorMin = Vector2.zero;
            public Vector2 anchorMax = Vector2.one;
        }
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Start()
        {
            if (applyOnStart)
            {
                DetectAndApplyLayout();
            }
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
                return;
        }
#endif
        
        public void DetectAndApplyLayout()
        {
            currentDeviceType = DetectDeviceType();
            ApplyLayout(currentDeviceType);
        }
        
        private DeviceType DetectDeviceType()
        {
            float aspect = (float)Screen.width / Screen.height;
            
            if (aspect > 1f)
            {
                return DeviceType.Landscape;
            }
            
            float diagonalInches = GetScreenDiagonalInches();
            
            if (diagonalInches >= 6.5f)
            {
                return DeviceType.Tablet;
            }
            
            return DeviceType.Phone;
        }
        
        private float GetScreenDiagonalInches()
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
            float widthInches = Screen.width / dpi;
            float heightInches = Screen.height / dpi;
            return Mathf.Sqrt(widthInches * widthInches + heightInches * heightInches);
        }
        
        private void ApplyLayout(DeviceType deviceType)
        {
            LayoutProfile profile = null;
            
            switch (deviceType)
            {
                case DeviceType.Phone:
                    profile = phoneLayout;
                    break;
                case DeviceType.Tablet:
                    profile = tabletLayout;
                    break;
                case DeviceType.Landscape:
                    profile = landscapeLayout;
                    break;
            }
            
            if (profile == null || !profile.enabled)
                return;
            
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (profile.modifyAnchors)
            {
                rectTransform.anchorMin = profile.anchorMin;
                rectTransform.anchorMax = profile.anchorMax;
            }
            
            rectTransform.sizeDelta = profile.sizeDelta;
            rectTransform.anchoredPosition = profile.anchoredPosition;
            rectTransform.localScale = profile.scale;
            
            Debug.Log($"[ResponsiveLayout] Applied {deviceType} layout to {gameObject.name}");
        }
        
        public void ForceApplyLayout(DeviceType deviceType)
        {
            currentDeviceType = deviceType;
            ApplyLayout(deviceType);
        }
    }
}

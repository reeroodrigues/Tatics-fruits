using UnityEngine;
using TMPro;

namespace Gameplay.Utils
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ResponsiveTextScaler : MonoBehaviour
    {
        [Header("Font Size Profiles")]
        [SerializeField] private float phoneFontSize = 24f;
        [SerializeField] private float tabletFontSize = 32f;
        [SerializeField] private float landscapeFontSize = 20f;
        
        [Header("Auto Size Settings")]
        [SerializeField] private bool useAutoSize = true;
        [SerializeField] private float autoSizeMin = 18f;
        [SerializeField] private float autoSizeMax = 48f;
        
        [Header("Device Detection")]
        [SerializeField] private float tabletDiagonalThreshold = 6.5f;
        
        private TextMeshProUGUI textComponent;
        
        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            ApplyResponsiveFontSize();
        }
        
        private void Start()
        {
            ApplyResponsiveFontSize();
        }
        
        private void ApplyResponsiveFontSize()
        {
            if (textComponent == null)
                return;
            
            float aspect = (float)Screen.width / Screen.height;
            bool isLandscape = aspect > 1f;
            bool isTablet = GetScreenDiagonalInches() >= tabletDiagonalThreshold;
            
            float targetFontSize;
            
            if (isLandscape)
            {
                targetFontSize = landscapeFontSize;
            }
            else if (isTablet)
            {
                targetFontSize = tabletFontSize;
            }
            else
            {
                targetFontSize = phoneFontSize;
            }
            
            if (useAutoSize)
            {
                textComponent.enableAutoSizing = true;
                textComponent.fontSizeMin = autoSizeMin;
                textComponent.fontSizeMax = Mathf.Max(autoSizeMax, targetFontSize);
                textComponent.fontSize = targetFontSize;
            }
            else
            {
                textComponent.enableAutoSizing = false;
                textComponent.fontSize = targetFontSize;
            }
        }
        
        private float GetScreenDiagonalInches()
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
            float widthInches = Screen.width / dpi;
            float heightInches = Screen.height / dpi;
            return Mathf.Sqrt(widthInches * widthInches + heightInches * heightInches);
        }
        
        public void RefreshFontSize()
        {
            ApplyResponsiveFontSize();
        }
    }
}

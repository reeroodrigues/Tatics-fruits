using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Utils
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class ResponsiveCanvasManager : MonoBehaviour
    {
        [Header("Reference Resolution")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);
        
        [Header("Match Settings")]
        [Tooltip("0 = Match Width, 0.5 = Match Average, 1 = Match Height")]
        [SerializeField] [Range(0, 1)] private float portraitMatch = 0.2f;
        [SerializeField] [Range(0, 1)] private float landscapeMatch = 0.8f;
        
        [Header("Aspect Ratio Thresholds")]
        [SerializeField] private float tabletAspectThreshold = 1.5f;
        [SerializeField] private float ultraWideThreshold = 2.1f;
        
        private CanvasScaler canvasScaler;
        private float lastAspect = -1f;
        
        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            ConfigureCanvasScaler();
        }
        
        private void Start()
        {
            ApplyResponsiveSettings();
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            float currentAspect = (float)Screen.width / Screen.height;
            if (Mathf.Abs(currentAspect - lastAspect) > 0.01f)
            {
                ApplyResponsiveSettings();
            }
        }
#endif
        
        private void ConfigureCanvasScaler()
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        }
        
        public void ApplyResponsiveSettings()
        {
            float aspect = (float)Screen.width / Screen.height;
            lastAspect = aspect;
            
            bool isPortrait = aspect < 1f;
            bool isTablet = aspect > tabletAspectThreshold || (1f / aspect) > tabletAspectThreshold;
            bool isUltraWide = aspect > ultraWideThreshold;
            
            if (isPortrait)
            {
                canvasScaler.matchWidthOrHeight = portraitMatch;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = landscapeMatch;
            }
            
            if (isUltraWide)
            {
                canvasScaler.matchWidthOrHeight = 1f;
            }
            
            Debug.Log($"[ResponsiveCanvas] Aspect: {aspect:F2}, Portrait: {isPortrait}, Tablet: {isTablet}, Match: {canvasScaler.matchWidthOrHeight:F2}");
        }
    }
}

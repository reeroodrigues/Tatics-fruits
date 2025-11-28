using UnityEngine;

namespace Gameplay.Utils
{
    [RequireComponent(typeof(Camera))]
    public class CameraAspectRatioController : MonoBehaviour
    {
        [Header("Target Aspect Ratio")]
        [SerializeField] private float targetAspect = 9f / 16f;
        
        [Header("Letterbox Settings")]
        [SerializeField] private bool enableLetterboxing = true;
        [SerializeField] private Color letterboxColor = Color.black;
        
        [Header("Camera Adjustment Mode")]
        [SerializeField] private AdjustmentMode adjustmentMode = AdjustmentMode.FitInside;
        
        private Camera cam;
        private float lastAspect = -1f;
        
        public enum AdjustmentMode
        {
            FitInside,
            FillScreen,
            None
        }
        
        private void Awake()
        {
            cam = GetComponent<Camera>();
        }
        
        private void Start()
        {
            AdjustCamera();
        }
        
        private void Update()
        {
            float currentAspect = (float)Screen.width / Screen.height;
            if (Mathf.Abs(currentAspect - lastAspect) > 0.01f)
            {
                AdjustCamera();
            }
        }
        
        private void AdjustCamera()
        {
            float windowAspect = (float)Screen.width / Screen.height;
            lastAspect = windowAspect;
            float scaleHeight = windowAspect / targetAspect;
            
            if (adjustmentMode == AdjustmentMode.None)
                return;
            
            if (enableLetterboxing)
            {
                if (scaleHeight < 1f)
                {
                    Rect rect = cam.rect;
                    rect.width = 1f;
                    rect.height = scaleHeight;
                    rect.x = 0;
                    rect.y = (1f - scaleHeight) / 2f;
                    cam.rect = rect;
                }
                else
                {
                    float scaleWidth = 1f / scaleHeight;
                    Rect rect = cam.rect;
                    rect.width = scaleWidth;
                    rect.height = 1f;
                    rect.x = (1f - scaleWidth) / 2f;
                    rect.y = 0;
                    cam.rect = rect;
                }
            }
            
            if (cam.orthographic && adjustmentMode == AdjustmentMode.FillScreen)
            {
                if (scaleHeight < 1f)
                {
                    cam.orthographicSize = cam.orthographicSize / scaleHeight;
                }
            }
            
            cam.backgroundColor = letterboxColor;
        }
        
        public void SetTargetAspect(float aspect)
        {
            targetAspect = aspect;
            AdjustCamera();
        }
    }
}

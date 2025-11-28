using UnityEngine;
using System;

namespace Gameplay.Utils
{
    public class DeviceOrientationHandler : MonoBehaviour
    {
        [Header("Allowed Orientations")]
        [SerializeField] private bool allowPortrait = true;
        [SerializeField] private bool allowPortraitUpsideDown = false;
        [SerializeField] private bool allowLandscapeLeft = false;
        [SerializeField] private bool allowLandscapeRight = false;
        [SerializeField] private bool autoRotation = false;
        public event Action<ScreenOrientation> OnOrientationChanged;
        
        private ScreenOrientation lastOrientation;
        
        private void Awake()
        {
            ApplyOrientationSettings();
        }
        
        private void Start()
        {
            lastOrientation = Screen.orientation;
        }
        
        private void Update()
        {
            if (Screen.orientation != lastOrientation)
            {
                lastOrientation = Screen.orientation;
                OnOrientationChanged?.Invoke(lastOrientation);
                Debug.Log($"[DeviceOrientation] Orientation changed to: {lastOrientation}");
            }
        }
        
        private void ApplyOrientationSettings()
        {
            Screen.autorotateToPortrait = allowPortrait;
            Screen.autorotateToPortraitUpsideDown = allowPortraitUpsideDown;
            Screen.autorotateToLandscapeLeft = allowLandscapeLeft;
            Screen.autorotateToLandscapeRight = allowLandscapeRight;
            
            if (autoRotation)
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            else
            {
                if (allowPortrait)
                    Screen.orientation = ScreenOrientation.Portrait;
                else if (allowLandscapeLeft)
                    Screen.orientation = ScreenOrientation.LandscapeLeft;
                else if (allowLandscapeRight)
                    Screen.orientation = ScreenOrientation.LandscapeRight;
            }
            
            Debug.Log($"[DeviceOrientation] Applied: Portrait={allowPortrait}, Landscape={allowLandscapeLeft || allowLandscapeRight}, Auto={autoRotation}");
        }
        
        public void SetOrientation(ScreenOrientation orientation)
        {
            Screen.orientation = orientation;
        }
        
        public void LockToPortrait()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
        
        public void LockToLandscape()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
    }
}

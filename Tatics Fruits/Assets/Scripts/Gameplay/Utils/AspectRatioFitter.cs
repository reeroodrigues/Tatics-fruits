using UnityEngine;

namespace Gameplay.Utils
{
    [ExecuteAlways]
    public class AspectRatioFitter : MonoBehaviour
    {
        [Header("Aspect Ratio Settings")]
        [SerializeField] private float targetAspect = 9f / 16f;
        [SerializeField] private bool maintainWidth = true;
        
        [Header("Min/Max Constraints")]
        [SerializeField] private bool useMinSize = false;
        [SerializeField] private Vector2 minSize = new Vector2(300, 300);
        [SerializeField] private bool useMaxSize = false;
        [SerializeField] private Vector2 maxSize = new Vector2(800, 800);
        
        private RectTransform rectTransform;
        private Vector2 lastParentSize;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Start()
        {
            ApplyAspectRatio();
        }
        
        private void OnEnable()
        {
            ApplyAspectRatio();
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                ApplyAspectRatio();
            }
        }
#endif
        
        private void OnRectTransformDimensionsChange()
        {
            ApplyAspectRatio();
        }
        
        public void ApplyAspectRatio()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (rectTransform.parent == null)
                return;
            
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null)
                return;
            
            Vector2 parentSize = parent.rect.size;
            
            if (parentSize == lastParentSize)
                return;
            
            lastParentSize = parentSize;
            
            float currentWidth = parentSize.x;
            float currentHeight = parentSize.y;
            
            float newWidth, newHeight;
            
            if (maintainWidth)
            {
                newWidth = currentWidth;
                newHeight = newWidth / targetAspect;
                
                if (newHeight > currentHeight)
                {
                    newHeight = currentHeight;
                    newWidth = newHeight * targetAspect;
                }
            }
            else
            {
                newHeight = currentHeight;
                newWidth = newHeight * targetAspect;
                
                if (newWidth > currentWidth)
                {
                    newWidth = currentWidth;
                    newHeight = newWidth / targetAspect;
                }
            }
            
            if (useMinSize)
            {
                newWidth = Mathf.Max(newWidth, minSize.x);
                newHeight = Mathf.Max(newHeight, minSize.y);
            }
            
            if (useMaxSize)
            {
                newWidth = Mathf.Min(newWidth, maxSize.x);
                newHeight = Mathf.Min(newHeight, maxSize.y);
            }
            
            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }
    }
}

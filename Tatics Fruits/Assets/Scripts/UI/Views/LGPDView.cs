using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace UI.Views
{
    public class LgpdView : MonoBehaviour
    {
        [SerializeField] private GameObject lgpdPanel;
        [SerializeField] private TextMeshProUGUI lgpdText;
        [SerializeField] private Button acceptButton;
        
        [SerializeField] private ScrollRect scrollRect;

        [SerializeField] private bool hasReachedEnd = false;

        private void Start()
        {
            acceptButton.gameObject.SetActive(false);
            lgpdPanel.SetActive(true);
            
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private void OnScrollValueChanged(Vector2 scrollPos)
        {
            if (!hasReachedEnd && scrollRect.verticalNormalizedPosition <= 0.001f)
            {
                hasReachedEnd = true;
                acceptButton.gameObject.SetActive(true);
            }
        }

        public void AcceptLgpd()
        {
            lgpdPanel.SetActive(false);
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.SaveSystem;

namespace UI.Views
{
    public class LgpdView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject lgpdPanel;
        [SerializeField] private TextMeshProUGUI lgpdText;
        [SerializeField] private Button acceptButton;

        [Header("Scroll")]
        [SerializeField] private ScrollRect scrollRect;

        private bool hasReachedEnd = false;

        private void Start()
        {
            var profile = SaveManager.Instance.Load<PlayerProfileData>();
            if (profile.hasAcceptedLGPD)
            {
                lgpdPanel.SetActive(false);
                return;
            }
            
            acceptButton.gameObject.SetActive(false);
            lgpdPanel.SetActive(true);

            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private void OnScrollValueChanged(Vector2 pos)
        {
            if (!hasReachedEnd && scrollRect.verticalNormalizedPosition <= 0.001f)
            {
                hasReachedEnd = true;
                acceptButton.gameObject.SetActive(true);
            }
        }

        public void AcceptLgpd()
        {
            var profile = SaveManager.Instance.Load<PlayerProfileData>();
            profile.hasAcceptedLGPD = true;
            SaveManager.Instance.Save(profile);
            
            lgpdPanel.SetActive(false);
        }
    }
}
using Gameplay.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace UI.Views
{
    public class AllLevelsCompletedView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button menuButton;

        public void Initialized(System.Action onMenuClick)
        {
            if(titleText)
                titleText.text = Localizer.Instance.Tr("levels_completed_title");

            if (messageText)
                messageText.text = Localizer.Instance.Tr("levels_completed_subtitle");
            
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() =>
            {
                Managers.AnalyticsManager.Instance?.TrackButtonClicked("all_levels_completed_menu");
                onMenuClick();
            });

            Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup)
                canvasGroup.alpha = 1f;
        }
    }
}
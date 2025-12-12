using Core.Services;
using Gameplay.Utils;
using New_GameplayCore.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class DefeatView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image blocker;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image star1, star2, star3;

        [Header("Buttons")]
        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;

        private DefeatPresenter _presenter;

        public void Bind(DefeatPresenter presenter, DefeatModel model)
        {
            _presenter = presenter;
            
            if (titleText)
            {
                titleText.text = Localizer.Instance.Tr(
                    "defeat_title_timeup",
                    "Tempo esgotado! Não foi dessa vez."
                );
            }
            
            if (scoreText)
            {
                scoreText.text = Localizer.Instance.TrFormat(
                    "defeat_score_line",
                    "Você fez: {0} pontos.",
                    model.totalScore
                );
            }
            
            SetStar(star1, model.starsEarned >= 1);
            SetStar(star2, model.starsEarned >= 2);
            SetStar(star3, model.starsEarned >= 3);
            
            if (replayButton)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(() =>
                {
                    Managers.AnalyticsManager.Instance?.TrackButtonClicked("defeat_replay");
                    _presenter.ClickReplay();
                });
            }
            if (menuButton)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(() =>
                {
                    Managers.AnalyticsManager.Instance?.TrackButtonClicked("defeat_menu");
                    _presenter.ClickMenu();
                });
            }
            Show();
        }


        private void SetStar(Image img, bool on)
        {
            if (!img) return;
            img.enabled = true;
        }

        private void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup) canvasGroup.alpha = 1f;
            if (blocker) blocker.raycastTarget = true;
        }
    }
}
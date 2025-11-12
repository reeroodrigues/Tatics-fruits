using New_GameplayCore.Services;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class VictoryView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image blocker;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI recordText;
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;
        [SerializeField] private Sprite starOn;
        [SerializeField] private Sprite starOff;
        
        [Header("Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;

        private VictoryPresenter _presenter;

        public void Bind(VictoryPresenter presenter, VictoryModel model)
        {
            _presenter = presenter;

            if (titleText)
            {
                titleText.text = Localizer.Instance.Tr(
                    "victory_title",
                    "Vitória!");
            }

            if (scoreText)
            {
                scoreText.text = Localizer.Instance.TrFormat(
                    "victory_score_line",
                    "Você fez: {0} pontos!", model.totalScore);
            }

            if (recordText)
            {
                if (model.newRecord)
                {
                    recordText.text = Localizer.Instance.Tr(
                        "victory_new_record",
                        "Novo Recorde!"
                    );
                }
                else
                {
                    recordText.text = Localizer.Instance.TrFormat(
                        "victory_previous_record",
                        "{0}",
                        model.bestBefore
                    );
                }
            }
            
            SetStar(star1, model.starsEarned >= 1);
            SetStar(star2, model.starsEarned >= 2);
            SetStar(star3, model.starsEarned >= 3);
            
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(_presenter.ClickNext);
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(_presenter.ClickReplay);
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenu");
            });
            
            Show();
        }

        private void SetStar(Image img, bool on)
        {
            if (!img) return;
            img.enabled = true;
            img.sprite  = on ? starOn : starOff;
            img.color   = Color.white;
        }

        private void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup)
                canvasGroup.alpha = 1f;

            if (blocker)
                blocker.raycastTarget = true;
        }

    }
}
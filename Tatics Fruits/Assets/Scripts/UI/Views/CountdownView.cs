using System.Collections;
using DG.Tweening;
using Gameplay.Utils;
using TMPro;
using UnityEngine;

namespace UI.Views
{
    public class CountdownView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Config")]
        [SerializeField] private float numberDuration = 1f;
        [SerializeField] private float scalePunch = 1.3f;

        public IEnumerator PlayCountdown()
        {
            gameObject.SetActive(true);
            
            countdownText.alpha = 0;
            subtitleText.alpha = 0;
            
            yield return ShowStep("3", Localizer.Instance.Tr("countdown_3", "PREPARAR"));
            
            yield return ShowStep("2", Localizer.Instance.Tr("countdown_2", "APONTAR"));
            
            yield return ShowStep("1", Localizer.Instance.Tr("countdown_1", "FAÇA PARES!"));
            
            yield return ShowStep(Localizer.Instance.Tr("countdown_go", "JÁ!"), "");
            
            countdownText.DOFade(0, 0.3f);
            subtitleText.DOFade(0, 0.3f);

            yield return new WaitForSeconds(0.3f);

            gameObject.SetActive(false);
        }

        private IEnumerator ShowStep(string number, string subtitle)
        {
            countdownText.text = number;
            subtitleText.text = subtitle;

            countdownText.alpha = 0;
            subtitleText.alpha = 0;

            countdownText.rectTransform.localScale = Vector3.one * 0.5f;
            
            countdownText.DOFade(1f, 0.2f);
            countdownText.rectTransform
                .DOScale(scalePunch, 0.25f)
                .SetEase(Ease.OutBack);

            subtitleText.DOFade(1f, 0.25f);

            yield return new WaitForSeconds(numberDuration);
            
            countdownText.DOFade(0f, 0.2f);
            subtitleText.DOFade(0f, 0.2f);
        }
    }
}

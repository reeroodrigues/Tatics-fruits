using System;
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
        [SerializeField] private TextMeshProUGUI countdownTime;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Config")]
        [SerializeField] private float numberDuration = 1f;
        [SerializeField] private float scalePunch = 1.3f;

        public IEnumerator PlayCountdown()
        {
            gameObject.SetActive(true);

            countdownText.text = "Preparar...apontar...faça pares!";
            countdownText.alpha = 0;

            countdownText.DOFade(1f, 0.5f);

            yield return ShowNumber("3");
            yield return ShowNumber("2");
            yield return ShowNumber("1");
            yield return ShowNumber("Já!");

            countdownTime.DOFade(0, 0.4f);
            countdownText.DOFade(0, 0.4f);
            
            gameObject.SetActive(false);
        }

        private IEnumerator ShowNumber(string value)
        {
            countdownTime.text = value;
            countdownTime.alpha = 0f;
            countdownTime.rectTransform.localScale = Vector3.one * 0.5f;

            countdownTime.DOFade(1f, 0.2f);
            countdownTime.rectTransform.DOScale(scalePunch, 0.25f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(numberDuration);

            countdownTime.DOFade(0f, 0.2f);

        }
    }
}
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
        
        [SerializeField] private RectTransform punchRoot;

        [Header("Cores por etapa")]
        [SerializeField] private Color color3 = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color color2 = new Color(1f, 0.6f, 0.1f);
        [SerializeField] private Color color1 = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color colorGo = new Color(0.2f, 1f, 0.2f);

        [Header("Timings")]
        [SerializeField] private float numberDuration = 0.6f;
        [SerializeField] private float goDuration = 0.8f;

        [Header("Escala / animação")]
        [SerializeField] private float scalePunch = 1.35f;
        [SerializeField] private float goScalePunch = 1.6f;

        [Header("Flash estilo arcade (opcional)")]
        [SerializeField] private CanvasGroup flashOverlay;
        [SerializeField] private float flashMaxAlpha = 0.65f;
        [SerializeField] private float flashDuration = 0.15f;

        public IEnumerator PlayCountdown()
        {
            gameObject.SetActive(true);

            if (!punchRoot && countdownText != null)
                punchRoot = countdownText.rectTransform;

            if (countdownText != null)
            {
                countdownText.alpha = 0;
                countdownText.rectTransform.localScale = Vector3.one;
            }

            if (subtitleText != null)
            {
                subtitleText.alpha = 0;
            }

            if (flashOverlay != null)
            {
                flashOverlay.alpha = 0;
                flashOverlay.gameObject.SetActive(true);
            }
            
            yield return ShowStep(
                "3",
                Localizer.Instance.Tr("countdown_3", "PREPARAR"),
                color3,
                scalePunch,
                numberDuration,
                isGo: false
            );
            
            yield return ShowStep(
                "2",
                Localizer.Instance.Tr("countdown_2", "APONTAR"),
                color2,
                scalePunch,
                numberDuration,
                isGo: false
            );
            
            yield return ShowStep(
                "1",
                Localizer.Instance.Tr("countdown_1", "FAÇA PARES!"),
                color1,
                scalePunch,
                numberDuration,
                isGo: false
            );
            
            yield return ShowStep(
                Localizer.Instance.Tr("countdown_go", "JÁ!"),
                "",
                colorGo,
                goScalePunch,
                goDuration,
                isGo: true
            );
            
            if (countdownText != null)
                countdownText.DOFade(0, 0.25f);

            if (subtitleText != null)
                subtitleText.DOFade(0, 0.25f);

            yield return new WaitForSeconds(0.25f);

            if (flashOverlay != null)
            {
                flashOverlay.alpha = 0;
                flashOverlay.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private IEnumerator ShowStep(
            string number,
            string subtitle,
            Color numberColor,
            float punchScale,
            float stepDuration,
            bool isGo)
        {
            if (countdownText == null)
                yield break;
            
            countdownText.DOKill();
            subtitleText?.DOKill();
            punchRoot?.DOKill();

            countdownText.text = number;
            countdownText.color = numberColor;
            countdownText.alpha = 0f;

            if (subtitleText != null)
            {
                subtitleText.text = subtitle;
                subtitleText.alpha = 0f;
            }

            if (punchRoot != null)
            {
                punchRoot.localScale = Vector3.one * 0.5f;
                punchRoot.localRotation = Quaternion.identity;
                punchRoot.anchoredPosition = Vector2.zero;
            }
            
            countdownText.DOFade(1f, 0.15f);

            if (punchRoot != null)
            {
                punchRoot
                    .DOScale(punchScale, 0.25f)
                    .SetEase(isGo ? Ease.OutElastic : Ease.OutBack);
            }

            if (subtitleText != null && !string.IsNullOrEmpty(subtitle))
            {
                subtitleText.DOFade(1f, 0.2f);
            }
            
            if (isGo)
            {
                PlayGoSpecialEffects();
            }

            yield return new WaitForSeconds(stepDuration);
            
            if (!isGo)
            {
                countdownText.DOFade(0f, 0.2f);
                subtitleText?.DOFade(0f, 0.2f);
            }
        }

        private void PlayGoSpecialEffects()
        {
            if (punchRoot != null)
            {
                punchRoot
                    .DOPunchRotation(new Vector3(0, 0, 18f), 0.35f, 10, 0.9f)
                    .SetUpdate(true);

                punchRoot
                    .DOShakeAnchorPos(0.3f, strength: new Vector2(15f, 10f), vibrato: 15, randomness: 60f)
                    .SetUpdate(true);
            }

            if (flashOverlay != null)
            {
                flashOverlay.DOKill();
                flashOverlay.alpha = 0f;
                flashOverlay
                    .DOFade(flashMaxAlpha, flashDuration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }
        }
    }
}

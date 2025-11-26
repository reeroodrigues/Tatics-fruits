using System.Collections;
using DG.Tweening;
using Gameplay.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class CountdownView : MonoBehaviour
    {
        [Header("UI")] 
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image[] backgroundGlows;
        [SerializeField] private ParticleSystem burstParticles;

        [Header("Configs")] 
        [SerializeField] private float numberDuration = 1f;
        [SerializeField] private float scalePunch = 1.8f;
        [SerializeField] private float rotationAmount = 20f;
        [SerializeField] private float shakeStrength = 0.4f;
        [SerializeField] private bool enableScreenShake = true;
        [SerializeField] private bool enablePulseEffect = true;

        [Header("Colors")] 
        [SerializeField] private Color color3 = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color color2 = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color color1 = new Color(1f, 0.4f, 0.3f);
        [SerializeField] private Color colorGo = new Color(0.3f, 1f, 1f);

        [Header("Audio")] 
        [SerializeField] private AudioClip tickSound;
        [SerializeField] private AudioClip goSound;

        private AudioSource _audioSource;
        private Camera _mainCamera;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _mainCamera = Camera.main;
        }

        private void InitializeGlows()
        {
            if(backgroundGlows == null || backgroundGlows.Length == 0)
                return;

            foreach (var glow in backgroundGlows)
            {
                if (glow != null)
                    glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 0);
            }
        }

        private void AnimateAllGlows(Color textColor, bool isGoStep)
        {
            if(backgroundGlows == null || backgroundGlows.Length == 0)
                return;

            for (int i = 0; i < backgroundGlows.Length; i++)
            {
                var glow = backgroundGlows[i];
                if(glow == null)
                    continue;

                var scaleMultiplier = 1f + (i * 0.2f);
                var delay = i * 0.05f;

                glow.color = new Color(textColor.r, textColor.g, textColor.b, 0);
                glow.DOFade(0.7f, 0.2f).SetDelay(delay);
                glow.transform.localScale = Vector3.one * (0.8f + i * 0.1f);
                glow.transform.DOScale((1.4f + i * 0.2f) * scaleMultiplier, numberDuration)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(delay);


                if (enablePulseEffect)
                    glow.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 5, 0.5f).SetDelay(delay);
                
                if(isGoStep)
                    glow.DOFade(1f, 0.1f).SetLoops(2,  LoopType.Yoyo).SetDelay(delay);
            }
        }

        private void FadeOutAllGlows(float duration)
        {
            if(backgroundGlows == null || backgroundGlows.Length == 0)
                return;

            foreach (var glow in backgroundGlows)
            {
                if (glow != null)
                    glow.DOFade(0, duration);
            }
        }

        public IEnumerator PlayCountdown()
        {
            gameObject.SetActive(true);

            countdownText.alpha = 0;
            subtitleText.alpha = 0;

            InitializeGlows();

            yield return ShowStep("3", Localizer.Instance.Tr("countdown_3", "Preparar"), color3, tickSound);
            yield return ShowStep("2", Localizer.Instance.Tr("countdown_2", "Apontar"), color2, tickSound);
            yield return ShowStep("1", Localizer.Instance.Tr("countdown_1", "Faça pares!"), color1, tickSound);
            yield return ShowStep(Localizer.Instance.Tr("countdown_go", "Já!"), "", colorGo, goSound, isGoStep: true);

            countdownText.DOFade(0, 0.3f);
            subtitleText.DOFade(0, 0.3f);

            FadeOutAllGlows(0.3f);

            yield return new WaitForSeconds(0.3f);

            gameObject.SetActive(false);
        }

        private IEnumerator ShowStep(string number, string subtitle, Color textColor, AudioClip sound,
            bool isGoStep = false)
        {
            countdownText.text = number;
            subtitleText.text = subtitle;
            countdownText.color = textColor;

            countdownText.alpha = 0;
            subtitleText.alpha = 0;

            countdownText.rectTransform.DOScale(scalePunch * 0.3f, 0.4f);
            countdownText.rectTransform.localRotation = Quaternion.Euler(0, 0, rotationAmount);

            AnimateAllGlows(textColor, isGoStep);

            PlaySound(sound);

            if (isGoStep)
            {
                countdownText.DOFade(1f, 0.15f);
                countdownText.rectTransform.DOScale(scalePunch * 1.5f, 0.4f).SetEase(Ease.OutElastic);

                countdownText.rectTransform.DORotate(Vector3.zero, 0.35f).SetEase(Ease.OutBack);
                
                countdownText.rectTransform.DOPunchRotation(new Vector3(0, 0, 10f), 0.5f, 8, 0.5f);

                if (enableScreenShake && _mainCamera != null)
                {
                    _mainCamera.DOShakePosition(0.4f, shakeStrength * 2f, 25, 90, false);
                }

                if (burstParticles != null)
                {
                    burstParticles.Play();
                }
            }
            else
            {
                countdownText.DOFade(1f, 0.2f);
                countdownText.rectTransform.DOScale(scalePunch, 0.4f).SetEase(Ease.OutBack);

                countdownText.rectTransform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
                
                countdownText.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 6, 0.5f);

                if (enableScreenShake && _mainCamera != null)
                {
                    _mainCamera.DOShakePosition(0.25f, shakeStrength, 18, 90, false);
                }
            }

            subtitleText.DOFade(0.85f, 0.3f);
            subtitleText.transform.DOScale(1.05f, 0.3f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(numberDuration);

            if (!isGoStep)
            {
                countdownText.DOFade(0f, 0.15f);
                subtitleText.DOFade(0f, 0.15f);

                FadeOutAllGlows(0.15f);

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
                _audioSource.PlayOneShot(clip);
        }
    }
}

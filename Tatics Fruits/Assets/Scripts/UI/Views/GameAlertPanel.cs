using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameAlertPanel : MonoBehaviour
    {
        public CanvasGroup _canvasGroup;
        public TextMeshProUGUI _messageText;
        public float _hideDelay = 3f;

        public event Action OnGameAlertHidden;

        private void Start()
        {
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void ShowMessage(string message, bool isStart, float duration = 2f)
        {
            gameObject.SetActive(true);

            _canvasGroup.DOKill();
            _canvasGroup.alpha = 1f;

            _messageText.text = message;
            _messageText.color = isStart ? Color.green : Color.red;
            _messageText.transform.localScale = Vector3.zero;

            _messageText.transform.DOScale(1.2f, 0.4f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (message != "Time's up!")
                        _messageText.transform.DOShakeScale(0.5f, 0.3f, 5, 90);
                });

            _canvasGroup.DOFade(1f, 0.5f);
            
            if (message != "Time's up!")
            {
                _canvasGroup.DOFade(0f, 0.5f).SetDelay(duration).OnComplete(() =>
                {
                    _messageText.transform.localScale = Vector3.zero;
                    Invoke(nameof(HidePanel), _hideDelay);
                });
            }
        }
        
        private void HidePanel()
        {
            gameObject.SetActive(false);
            OnGameAlertHidden?.Invoke();
        }
        
        public void ClosePanel()
        {
            _canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnGameAlertHidden?.Invoke();
                Destroy(gameObject);
            });
        }
        
        public void OnCloseButtonPressed()
        {
            ClosePanel();
        }
    }
}
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace New_GameplayCore.Views
{
    public class TimeDeltaToast : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private float rise = 80f;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private Color plusColors = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color minusColors = new Color(1f, 0.3f, 0.3f);

        public void Play(int delta)
        {
            if (!label)
                label = GetComponentInChildren<TextMeshProUGUI>();
            
            if(!cg)
                cg = GetComponentInChildren<CanvasGroup>();

            var sing = delta > 0 ? "+" : "";
            label.text = $"{sing}{delta}";
            label.color = delta >= 0 ? plusColors : minusColors;
            
            var rt = (RectTransform) transform;
            var start = rt.anchoredPosition;

            cg.alpha = 0f;
            rt.localScale = Vector3.one * 0.9f;
            DOTween.Kill(this);

            var s = DOTween.Sequence().SetTarget(this);
            s.Append(cg.DOFade(1f, 0.12f));
            s.Join(rt.DOScale(1.05f, 0.12f).SetEase(Ease.OutBack));
            s.Append(rt.DOAnchorPos(start +  new Vector2(0, rise), duration).SetEase(Ease.OutQuad));
            s.Join(cg.DOFade(0f, duration));
            s.OnComplete(() => Destroy(gameObject));
        }
    }
}
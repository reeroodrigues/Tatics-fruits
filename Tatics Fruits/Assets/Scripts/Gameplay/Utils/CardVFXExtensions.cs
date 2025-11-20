using DG.Tweening;
using UnityEngine;

namespace New_GameplayCore.Utils
{
    public static class CardVFXExtensions
    {
        public static void PlayDropAnimation(this Transform cardTransform)
        {
            cardTransform.localRotation = Quaternion.Euler(0,0,0);
            cardTransform.DOLocalRotate(new Vector3(0, 0, 45), 0.30f);
        }

        public static void PlayPairMatched(this Transform cardTransform)
        {
            var seq = DOTween.Sequence();
            seq.Append(cardTransform.DOScale(1.2f, 0.30f)).Append(cardTransform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
        }
    }
}
using System;

namespace Ads
{
    public interface IInterstitialAdProvider
    {
        bool IsAdReady();
        void ShowAd(Action onAdCompleted, Action onAdFailed);
        void LoadAd();
    }
}

namespace Ads
{
    public interface IBannerAdProvider
    {
        void LoadAndShowBanner();
        void HideBanner();
        void DestroyBanner();
        bool IsBannerShowing();
    }
}

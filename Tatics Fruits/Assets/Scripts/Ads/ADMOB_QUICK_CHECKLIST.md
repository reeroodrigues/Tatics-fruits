# AdMob Testing - Quick Checklist

## ✅ Setup (Do Once)

- [ ] Verify ad unit ID in `AdMobInterstitialProvider.cs`
  - **Current Android ID**: `ca-app-pub-8609532543875876~3807758387`
  - ⚠️ **This looks like an App ID, not an Ad Unit ID!**
  - Go to AdMob Console → Get your Interstitial Ad Unit ID
  
- [ ] Add AdSystem GameObject to your scene
- [ ] Add these components to AdSystem:
  - `InterstitialAdManager`
  - `AdInitializer` (set to **AdMob**)
  - `AdDebugUI` (for testing only)

## 🧪 Testing in Unity Editor

- [ ] Press Play
- [ ] Check Console for these logs:
  ```
  [GoogleMobileAdsInitializer] Initializing Google Mobile Ads SDK...
  [GoogleMobileAdsInitializer] Google Mobile Ads initialization complete.
  [AdInitializer] Ad provider initialized: AdMob
  [AdMobInterstitialProvider] Loading interstitial ad...
  ```
- [ ] **Note**: Ads won't show in editor (this is normal!)

## 📱 Testing on Android Device

### First Time Setup
- [ ] Connect Android device via USB
- [ ] Enable USB debugging on device
- [ ] Build and Run from Unity

### Testing Ads
- [ ] App launches successfully
- [ ] Debug panel appears in top-left corner
- [ ] Click "TOGGLE TEST MODE" button
- [ ] Click "SHOW AD NOW" button
- [ ] **Ad should appear!**
  - If not, check troubleshooting section

### What to Verify
- [ ] Ad loads successfully
- [ ] Ad displays full screen
- [ ] Game pauses during ad (`Time.timeScale = 0`)
- [ ] Can close the ad
- [ ] Game resumes after closing ad
- [ ] Timer resets and counts down
- [ ] Next ad loads automatically

## 🐛 Quick Troubleshooting

### No ads appearing?

**Check 1: Ad Unit ID**
```
Current: ca-app-pub-8609532543875876~3807758387
This might be wrong! Should look like:
Correct: ca-app-pub-XXXXXXXXXXXXXXXX/1234567890
```

**Check 2: Use Test Ad Unit ID**

Temporarily change in `AdMobInterstitialProvider.cs`:
```csharp
private const string ANDROID_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";
```

**Check 3: Internet Connection**
- Ads require internet
- Test on WiFi or mobile data

**Check 4: Console Logs**

Connect device and check logs:
```bash
adb logcat -s Unity
```

Look for errors like:
- "ad failed to load"
- "invalid ad unit ID"
- "network error"

## 📊 Expected Console Logs (Success)

```
[GoogleMobileAdsInitializer] Initializing Google Mobile Ads SDK...
[GoogleMobileAdsInitializer] Google Mobile Ads initialization complete.
[AdMobInterstitialProvider] Initialized with ad unit ID: ca-app-pub-...
[AdInitializer] Ad provider initialized: AdMob
[AdMobInterstitialProvider] Loading interstitial ad with ID: ca-app-pub-...
[AdMobInterstitialProvider] Interstitial ad loaded successfully
[InterstitialAdManager] Test mode ENABLED. Interval: 30s
[AdDebugUI] Manual ad trigger requested
[InterstitialAdManager] Showing interstitial ad...
[AdMobInterstitialProvider] Showing interstitial ad with ID: ca-app-pub-...
[AdMobInterstitialProvider] Interstitial ad full screen content opened
[AdMobInterstitialProvider] Interstitial ad full screen content closed
[InterstitialAdManager] Interstitial ad completed successfully
[AdMobInterstitialProvider] Loading interstitial ad with ID: ca-app-pub-...
```

## 🎯 Next Steps After Testing

- [ ] Test with Google's test ad unit ID first (see ADMOB_SETUP_GUIDE.md)
- [ ] Verify your real ad unit ID from AdMob console
- [ ] Update the ad unit ID in code
- [ ] Test again with real ad unit ID
- [ ] Disable Test Mode for production (`testMode = false`)
- [ ] Remove AdDebugUI component for production
- [ ] Build final version

## 🔗 Quick Links

- **Get Ad Unit ID**: [AdMob Console](https://apps.admob.com/) → Apps → Your App → Ad Units
- **Test Ad Unit IDs**: [Google's Test IDs](https://developers.google.com/admob/unity/test-ads)
- **Full Setup Guide**: See `ADMOB_SETUP_GUIDE.md` in this folder

---

**Most Common Issue**: Wrong ad unit ID! Double-check it matches your interstitial ad unit from AdMob console.

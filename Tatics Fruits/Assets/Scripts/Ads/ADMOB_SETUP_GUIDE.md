# AdMob Integration Setup Guide

Your ad system is now fully integrated with Google AdMob SDK! Follow this guide to test and deploy.

## ✅ Current Setup Status

- ✅ Google Mobile Ads SDK installed
- ✅ AdMob provider implemented
- ✅ Auto-initialization configured
- ✅ Your Android Ad Unit ID configured: `ca-app-pub-8609532543875876~3807758387`

## 🚀 Quick Start - Testing AdMob

### Step 1: Configure Your Scene

1. **Find or create the AdSystem GameObject:**
   - If you used `AdSystemSetup`, you already have this
   - Otherwise, create an empty GameObject named "AdSystem"

2. **Add required components:**
   - `InterstitialAdManager`
   - `AdInitializer`
   - `AdDebugUI` (for testing)

3. **Configure AdInitializer:**
   - Select the AdSystem GameObject
   - In the `AdInitializer` component
   - Set **Provider Type** to **AdMob**

### Step 2: Test in Unity Editor

**Important:** AdMob ads won't show in the Unity Editor, but you can verify the initialization and loading process.

1. Press Play
2. Check the Console for these logs:

```
[GoogleMobileAdsInitializer] Initializing Google Mobile Ads SDK...
[GoogleMobileAdsInitializer] Google Mobile Ads initialization complete.
[AdInitializer] Ad provider initialized: AdMob
[AdMobInterstitialProvider] Loading interstitial ad with ID: ca-app-pub-8609532543875876~3807758387
```

If you see errors, check the troubleshooting section below.

### Step 3: Test on Android Device

AdMob ads will only display on actual devices, not in the editor.

**Build Settings:**
1. Go to File → Build Settings
2. Select Android
3. Click "Build and Run"

**Testing on Device:**
1. Install the app on your Android device
2. Enable test mode in the debug panel (toggle for 30-second intervals)
3. Click "SHOW AD NOW" to trigger an ad
4. You should see a test ad appear

**Connect Device Logs:**
```bash
adb logcat -s Unity
```

Look for the same initialization logs plus:
```
[AdMobInterstitialProvider] Interstitial ad loaded successfully
[AdMobInterstitialProvider] Showing interstitial ad
[AdMobInterstitialProvider] Interstitial ad full screen content opened
[AdMobInterstitialProvider] Interstitial ad full screen content closed
```

## 🔑 Important: Ad Unit ID Information

### Your Current Configuration

**Android Ad Unit ID:** `ca-app-pub-8609532543875876~3807758387`
**iOS Ad Unit ID:** `ca-app-pub-3940256099942544/4411468910` (test ID)

### ⚠️ Verify Your Ad Unit ID

The Android ID you have looks like it might be an **App ID** rather than an **Ad Unit ID** because:
- Ad Unit IDs for interstitials typically end with different numbers
- It ends with `~3807758387` which is common for App IDs

**To check:**
1. Go to [AdMob Console](https://apps.admob.com/)
2. Navigate to Apps → Your App → Ad Units
3. Find your **Interstitial Ad Unit**
4. Copy the Ad Unit ID (should look like `ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX`)

**If you need to update it:**

Edit `/Assets/Scripts/Ads/AdMobInterstitialProvider.cs`:

```csharp
private const string ANDROID_AD_UNIT_ID = "YOUR_ACTUAL_INTERSTITIAL_AD_UNIT_ID";
```

### Test Ad Unit IDs

For testing, you can temporarily use Google's test ad unit IDs:

```csharp
// Android Test Interstitial
private const string ANDROID_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";

// iOS Test Interstitial  
private const string IOS_AD_UNIT_ID = "ca-app-pub-3940256099942544/4411468910";
```

**Benefits of test IDs:**
- Always serve test ads
- No risk of invalid traffic
- Can test unlimited times

## 📱 Platform-Specific Setup

### Android Setup

**1. Verify AndroidManifest.xml includes AdMob App ID:**

The Google Mobile Ads SDK should have already added this, but verify:

Location: `/Assets/Plugins/Android/AndroidManifest.xml`

```xml
<meta-data
    android:name="com.google.android.gms.ads.APPLICATION_ID"
    android:value="ca-app-pub-XXXXXXXXXXXXXXXX~XXXXXXXXXX"/>
```

**2. Test on Android Device:**
- Minimum Android version: 5.0 (API level 21)
- Recommended: Use a physical device, not an emulator
- Enable USB debugging

### iOS Setup

**1. Update iOS Ad Unit ID:**

Once you have your iOS ad unit from AdMob console, update:

```csharp
private const string IOS_AD_UNIT_ID = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
```

**2. Configure Info.plist:**
- Add SKAdNetworkItems (Google Mobile Ads SDK should do this automatically)

**3. Test on iOS Device:**
- Requires a physical iOS device
- Build through Xcode

## 🧪 Testing Workflow

### Phase 1: Verify Integration (Unity Editor)

1. Set Provider to **AdMob** in AdInitializer
2. Press Play
3. ✅ Check Console for successful initialization
4. ✅ Check for "Loading interstitial ad" messages
5. ❌ Ads won't display in editor (this is normal)

### Phase 2: Test on Device with Test Ads

1. **Use Google's test ad unit IDs** (see above)
2. Build to Android device
3. Enable Test Mode (30-second intervals)
4. Click "SHOW AD NOW"
5. ✅ Test ads should appear
6. ✅ Game should pause during ad
7. ✅ Game should resume after closing ad

### Phase 3: Test with Your Real Ad Unit IDs

1. **Replace test IDs with your real ad unit IDs**
2. **Add your device as a test device** in AdMob console
3. Build to device
4. Test ads should still show but from your ad units
5. Monitor AdMob dashboard for impressions

### Phase 4: Production Testing

1. Remove debug UI (`AdDebugUI` component)
2. Disable Test Mode
3. Verify 3-minute (180s) interval
4. Test complete gameplay flow
5. Monitor AdMob dashboard

## 🔍 Debugging & Troubleshooting

### Common Issues

**"Google Mobile Ads initialization failed"**
- Check internet connection
- Verify Google Play Services is installed (Android)
- Check AdMob account is active

**"Interstitial ad failed to load"**
- Verify ad unit ID is correct
- Check internet connection
- AdMob needs time to prepare ads (can take hours for new accounts)
- Try using test ad unit IDs first

**"Interstitial ad is not ready to be shown"**
- Ad hasn't loaded yet (wait a few seconds)
- Ad failed to load (check previous error logs)
- Try loading again

**Ads show in test but not production**
- Make sure you've added your app to AdMob console
- Verify ad units are active
- Check your app is approved (can take hours)
- Make sure you're not clicking your own ads

### Enable Verbose Logging

For more detailed AdMob logs on Android:

```bash
adb shell setprop log.tag.Ads DEBUG
adb logcat -s Ads
```

### Check Ad Inspector (On Device)

The Google Mobile Ads SDK includes an Ad Inspector for debugging:

Add this code temporarily to a UI button:

```csharp
using GoogleMobileAds.Api;

public void OpenAdInspector()
{
    MobileAds.OpenAdInspector((AdInspectorError error) =>
    {
        if (error != null)
        {
            Debug.LogError("Ad Inspector failed: " + error);
        }
    });
}
```

## 📊 Monitoring Performance

### In Your Game (Debug Panel)

The debug panel shows:
- ⏱️ Time until next ad
- ✅ Ad ready status
- 🔧 Test mode status

### In AdMob Dashboard

Monitor:
- Impressions
- Fill rate
- eCPM (earnings per thousand impressions)
- Click-through rate (CTR)

## ⚙️ Configuration Options

### Adjust Ad Frequency

Edit `/Assets/Scripts/Ads/InterstitialAdManager.cs`:

```csharp
private const float AD_INTERVAL_SECONDS = 180f;  // Change to your desired interval
```

**Recommendations:**
- 180s (3 minutes) - Good balance
- 120s (2 minutes) - More aggressive
- 300s (5 minutes) - More user-friendly

### Multiple Ad Networks (Mediation)

AdMob supports mediation with other ad networks. To add:

1. Go to AdMob Console → Mediation
2. Create mediation group
3. Add ad sources (Unity Ads, Meta, etc.)
4. No code changes needed - AdMob handles it!

## 🚀 Going to Production

### Pre-Launch Checklist

- [ ] Replace test ad unit IDs with real ones
- [ ] Test on real devices with real ad units
- [ ] Remove or disable `AdDebugUI` component
- [ ] Set Test Mode to `false`
- [ ] Verify 180-second interval
- [ ] Test across multiple devices
- [ ] Monitor AdMob dashboard for test impressions
- [ ] Enable ad mediation (optional)
- [ ] Set up app-ads.txt (for better ad performance)

### Post-Launch Monitoring

**First 24 Hours:**
- Check AdMob dashboard for impressions
- Monitor fill rate
- Check for unusual patterns
- Verify no policy violations

**Ongoing:**
- Monitor eCPM trends
- Test new ad placements
- Consider adding banner/rewarded ads
- Optimize ad frequency based on user feedback

## 🎯 Next Steps

1. **Verify your ad unit ID** is correct (see "Important: Ad Unit ID Information" above)
2. **Test with Google's test ad unit IDs** first
3. **Build to Android device** and verify ads show
4. **Switch to your real ad unit IDs** when ready
5. **Monitor AdMob dashboard** for performance

## 💡 Pro Tips

- **Start with test IDs**: Always test with Google's test ad unit IDs first
- **Add your device as a test device**: Prevents invalid traffic
- **Don't click your own ads**: Can get your account banned
- **Test ad frequency**: Too many ads = bad UX, too few = less revenue
- **Use mediation**: Can increase fill rate and eCPM
- **Monitor analytics**: Track session length vs ad frequency

## 📚 Resources

- [AdMob Console](https://apps.admob.com/)
- [Google Mobile Ads Unity SDK Documentation](https://developers.google.com/admob/unity/quick-start)
- [AdMob Policy Center](https://support.google.com/admob/answer/6128543)
- [Test Ad Unit IDs](https://developers.google.com/admob/unity/test-ads)

---

**Need Help?** Check the Console logs first - they contain detailed information about what's happening with your ads!

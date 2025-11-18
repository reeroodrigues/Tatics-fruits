# Ad Testing Guide

This guide will help you test your interstitial ad implementation.

## Setup

1. **Add Components to Your Scene:**
   - Create an empty GameObject named "AdSystem"
   - Add the `InterstitialAdManager` component
   - Add the `AdInitializer` component
   - Add the `AdDebugUI` component (optional, for testing)

2. **Configure Ad Provider:**
   - In the `AdInitializer` inspector, select your ad provider (UnityAds or AdMob)
   - Make sure your ad SDK is properly installed and initialized

## Testing Methods

### Method 1: Debug UI Panel (Recommended for Quick Testing)

The debug UI panel appears in the top-left corner of your game.

**Features:**
- **Status Display**: Shows if ads are ready
- **Countdown Timer**: Shows time until next ad
- **Show Ad Now**: Manually trigger an ad immediately
- **Toggle Test Mode**: Switch between 3-minute (180s) and 30-second intervals
- **Press F1**: Hide/show the debug panel

**Steps:**
1. Run your game in Play mode
2. The debug panel should appear in the top-left
3. Click "TOGGLE TEST MODE" to enable 30-second intervals
4. Click "SHOW AD NOW" to immediately test an ad
5. Watch the countdown timer to see when the next ad will show

### Method 2: Test Mode Timer

Enable test mode to reduce the ad interval from 3 minutes to 30 seconds.

**In Inspector:**
- Select the GameObject with `InterstitialAdManager`
- Check the "Test Mode" checkbox

**At Runtime:**
- Use the debug UI panel to toggle test mode
- Call `InterstitialAdManager.Instance.ToggleTestMode()` from code

### Method 3: Manual Ad Triggering

Force show an ad at any time during testing.

**From Debug UI:**
- Click "SHOW AD NOW" button

**From Code:**
```csharp
InterstitialAdManager.Instance.ForceShowAd();
```

**From Console:**
- You can add a cheat code in your game to trigger ads manually

## What to Check

### ✅ Checklist

- [ ] Ad system initializes without errors
- [ ] Debug UI panel appears (if AdDebugUI is added)
- [ ] Countdown timer counts down correctly
- [ ] Ads show after the interval expires (30s in test mode, 180s in normal mode)
- [ ] Game pauses when ad is showing (Time.timeScale = 0)
- [ ] Game resumes after ad completes
- [ ] Timer resets after ad is shown
- [ ] Next ad loads automatically after one is shown
- [ ] Manual "SHOW AD NOW" button works
- [ ] Test mode toggle works correctly

### 🔍 Console Logs to Look For

When everything is working, you should see logs like:

```
[InterstitialAdManager] Test mode ENABLED. Interval: 30s
Ad provider initialized: UnityAds
Unity Ads: Loading interstitial ad with ID: Interstitial_Android
[InterstitialAdManager] Showing interstitial ad...
Unity Ads: Showing interstitial ad with ID: Interstitial_Android
[InterstitialAdManager] Interstitial ad completed successfully.
Unity Ads: Loading interstitial ad with ID: Interstitial_Android
```

### ⚠️ Common Issues

**"Ad Manager not found!"**
- Make sure you have `InterstitialAdManager` component in your scene
- Check that the GameObject is active

**"No ad provider set"**
- Add `AdInitializer` component
- Make sure it runs before you try to show ads

**"Interstitial ad is not ready yet"**
- The ad SDK hasn't loaded an ad yet
- Wait a few seconds and try again
- Check your ad SDK initialization

**Ads not showing automatically**
- Check that "Enable Ads" is checked in the inspector
- Verify the timer is counting up (check debug UI)
- Make sure Time.timeScale is not 0 when not showing ads

## Testing on Device

For final testing, build to your device:

1. **Disable Test Mode** (set back to 3-minute intervals)
2. **Remove or disable AdDebugUI** in production builds
3. Use your ad network's test device IDs
4. Check device logs using:
   - Android: `adb logcat -s Unity`
   - iOS: Xcode console

## Advanced Testing

### Custom Test Intervals

Modify the constants in `InterstitialAdManager.cs`:

```csharp
private const float AD_INTERVAL_SECONDS = 180f;          // Normal: 3 minutes
private const float TEST_MODE_INTERVAL_SECONDS = 30f;    // Test: 30 seconds
```

You can change `TEST_MODE_INTERVAL_SECONDS` to 10 or 15 seconds for even faster testing.

### Simulating Ad Failures

To test error handling, modify your ad provider implementation to randomly fail:

```csharp
public void ShowAd(Action onAdCompleted, Action onAdFailed)
{
    // Simulate random failures for testing
    if (Random.value < 0.3f)  // 30% failure rate
    {
        onAdFailed?.Invoke();
    }
    else
    {
        onAdCompleted?.Invoke();
    }
}
```

## Production Checklist

Before releasing:

- [ ] Set `testMode = false` in InterstitialAdManager inspector
- [ ] Remove or disable AdDebugUI component
- [ ] Replace test ad unit IDs with real ones
- [ ] Test on actual devices
- [ ] Verify ads don't show too frequently
- [ ] Check ad doesn't break game flow
- [ ] Ensure Time.timeScale is properly restored
- [ ] Test across scene transitions

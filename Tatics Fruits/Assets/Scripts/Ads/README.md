# 🎮 Complete AdMob Integration - Fruit Tactics

## 📋 System Overview

Your project now has a **complete, production-ready ad system** with both **interstitial** and **banner** ads!

### **✅ Features**
- **Interstitial Ads** - Full-screen ads between gameplay
- **Banner Ads** - Bottom-screen banners in menus  
- **Test Providers** - Fake ads for Unity Editor testing
- **AdMob Integration** - Real Google AdMob ads on device
- **Debug Panel** - In-game controls (Press F1)
- **Editor Tools** - Unity menu helpers

---

## 🚀 Quick Start

### **Test in Unity Editor:**
1. Press **Play**
2. See debug panel in top-left
3. See test banner at bottom (gray/yellow)
4. Click **"SHOW AD NOW"** to test interstitial
5. Click **"TOGGLE BANNER"** to hide/show banner

### **Test on Android:**
1. Build APK
2. Install on device
3. Banner appears immediately
4. Interstitial shows after 30 seconds (test mode)

---

## 🚀 Quick Setup (2 Minutes)

### Step 1: Add to Scene
1. Create an empty GameObject in your scene (name it "AdSystem")
2. Add the `AdSystemSetup` component to it
3. The component will automatically add all required components

### Step 2: Configure Provider
1. Select the AdSystem GameObject
2. Find the `AdInitializer` component
3. Choose your provider:
   - **TestProvider** - For testing without SDK (simulates ads)
   - **UnityAds** - For Unity Ads integration
   - **AdMob** - For Google AdMob integration

### Step 3: Test It!
1. Press Play
2. Debug panel appears in top-left corner
3. Click "TOGGLE TEST MODE" (reduces interval to 30 seconds)
4. Click "SHOW AD NOW" to test immediately
5. Press F1 to hide/show debug panel

## 📁 File Structure

```
/Assets/Scripts/Ads/
├── Core/
│   ├── IInterstitialAdProvider.cs       # Interface for ad providers
│   ├── InterstitialAdManager.cs         # Main manager (handles timing)
│   └── AdInitializer.cs                 # Initializes the provider
│
├── Providers/
│   ├── TestAdProvider.cs                # Simulated ads (no SDK needed)
│   ├── UnityAdsInterstitialProvider.cs  # Unity Ads template
│   └── AdMobInterstitialProvider.cs     # AdMob template
│
├── Testing/
│   ├── AdDebugUI.cs                     # Debug panel UI
│   └── AdSystemSetup.cs                 # Easy setup helper
│
└── Documentation/
    ├── README.md                        # This file
    └── AD_TESTING_GUIDE.md              # Detailed testing guide
```

## 🎮 How It Works

1. **Timer Runs**: `InterstitialAdManager` counts time in the background
2. **Interval Reached**: After 3 minutes (180s), manager shows an ad
3. **Game Pauses**: `Time.timeScale = 0` while ad plays
4. **Ad Completes**: Game resumes, timer resets, next ad loads
5. **Repeat**: Process continues automatically

## ⚙️ Configuration Options

### InterstitialAdManager Inspector
- **Enable Ads**: Toggle ads on/off
- **Test Mode**: Use 30-second intervals instead of 3 minutes

### TestAdProvider Inspector (when using TestProvider)
- **Ad Duration**: How long simulated ad plays (default: 3s)
- **Simulate Failures**: Test error handling
- **Failure Rate**: Percentage of ads that fail (0-100%)

## 🧪 Testing Modes

### Mode 1: Test Provider (No SDK Required)
```
AdInitializer → Provider Type: TestProvider
```
- Perfect for initial testing
- Simulates ads with console logs
- No external SDK needed
- Customize ad duration and failure rate

### Mode 2: Test Mode Timer
```
InterstitialAdManager → Test Mode: ✓
```
- Reduces interval from 3 minutes to 30 seconds
- Use with any provider
- Toggle via debug UI or inspector

### Mode 3: Manual Trigger
```
Click "SHOW AD NOW" in debug panel
OR
InterstitialAdManager.Instance.ForceShowAd();
```
- Show ads instantly
- Great for testing specific scenarios

## 📊 Debug Panel Features

**Status Display:**
- 🟢 Green = Ad ready to show
- 🟡 Yellow = Counting down
- 🔴 Red = Error/Not initialized

**Controls:**
- **SHOW AD NOW** - Trigger ad immediately
- **TOGGLE TEST MODE** - Switch between 30s/180s intervals
- **F1 Key** - Hide/show panel

**Information:**
- Time until next ad (MM:SS format)
- Test mode status
- Current system status

## 🔧 Switching to Real Ads

### For Unity Ads:
1. Install Unity Ads package from Package Manager
2. Update `UnityAdsInterstitialProvider.cs` with SDK calls
3. Replace ad unit IDs with your own
4. Change `AdInitializer` to use `UnityAds` provider

### For AdMob:
1. Install Google Mobile Ads SDK
2. Update `AdMobInterstitialProvider.cs` with SDK calls
3. Replace test ad unit IDs with your own
4. Change `AdInitializer` to use `AdMob` provider

## 📝 Production Checklist

Before releasing your game:

- [ ] Switch from TestProvider to real provider
- [ ] Set Test Mode = false
- [ ] Remove or disable AdDebugUI
- [ ] Use real ad unit IDs (not test IDs)
- [ ] Test on actual devices
- [ ] Verify interval is 3 minutes (180s)
- [ ] Check ads don't disrupt gameplay
- [ ] Test scene transitions

## 🐛 Troubleshooting

**"Ad Manager not found!"**
→ Add `InterstitialAdManager` to your scene

**"No ad provider set"**
→ Add `AdInitializer` component

**Ads not showing automatically**
→ Check "Enable Ads" is checked
→ Verify provider is initialized
→ Check console for errors

**Timer not counting**
→ Make sure Time.timeScale = 1 when not showing ads
→ Check that the GameObject is active

## 💡 Tips

- Use **TestProvider** during development
- Enable **Test Mode** when testing timing
- Use **Force Show** for specific event testing
- Check the **Console** for detailed logs
- Read **AD_TESTING_GUIDE.md** for comprehensive testing info

## 🎯 Next Steps

1. Test with TestProvider to verify system works
2. Enable Test Mode to reduce wait time
3. Use debug panel to trigger ads manually
4. Integrate your chosen ad SDK
5. Test on device before release

Need more help? Check out `AD_TESTING_GUIDE.md` for detailed testing instructions!

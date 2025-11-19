# 🚀 Google Play Internal Testing Setup Guide

## ✅ Current Banner Ad Configuration

Your banner ads are already configured with your real AdMob ad unit ID:

```
Android Banner Ad Unit ID: ca-app-pub-8609532543875876/5682573689 ✅
Android App ID: ca-app-pub-8609532543875876~3807758387 ✅
```

---

## 📋 Pre-Release Checklist

### **1. Disable Interstitial Ads** ✅

**AdSystem GameObject Configuration:**

```
AdSystem
├── AdInitializer
│   ├── Initialize Interstitial: FALSE ✅ (Already set!)
│   └── Initialize Banner: TRUE ✅
│
└── InterstitialAdManager
    └── Enable Ads: FALSE ✅ (Recommended to disable completely)
```

**Action Required:**
1. Select `AdSystem` GameObject in `MainMenu` scene
2. Find `InterstitialAdManager` component
3. Set `Enable Ads` to **FALSE**

---

### **2. Banner Ads Configuration** ✅

**BannerAdManager Settings:**

```
BannerAdManager
├── Show Banner On Start: TRUE ✅
└── Enable Banners: TRUE ✅
```

**Banner Ad Unit IDs (Already configured):**

```csharp
// In AdMobBannerProvider.cs
Android: ca-app-pub-8609532543875876/5682573689 ✅
iOS: ca-app-pub-3940256099942544/2934735716 (Test ID - update when you get iOS ID)
```

---

### **3. Build Settings for Android**

#### **A. Player Settings:**

1. **File → Build Settings → Android → Player Settings**

2. **Company Name:**
   - Set your company/developer name

3. **Product Name:**
   - Currently: `Fruit Tactics` ✅

4. **Version:**
   - Start with `1.0.0` for internal testing
   - Increment for each new upload (1.0.1, 1.0.2, etc.)

5. **Bundle Identifier (Package Name):**
   - Must match your Google Play Console app
   - Format: `com.yourcompany.fruittactics`
   - Example: `com.myname.fruittactics`
   - **IMPORTANT:** This must match exactly what you registered in Google Play Console

6. **Version Code (Bundle Version Code):**
   - Start with `1`
   - Increment for each new build you upload (2, 3, 4...)
   - Google Play requires each upload to have a higher version code

#### **B. Other Settings:**

1. **Minimum API Level:**
   - Recommended: `Android 7.0 'Nougat' (API level 24)` or higher
   - Google Play requires minimum API level 24+ for new apps

2. **Target API Level:**
   - Set to `Android 14.0 'Upside Down Cake' (API level 34)` or latest
   - Google Play requires target API 33+ (as of Aug 2024)

3. **Scripting Backend:**
   - Recommended: `IL2CPP` (required for 64-bit)

4. **Target Architectures:**
   - Enable: `ARM64` ✅ (required)
   - Enable: `ARMv7` (optional, for older devices)
   - **IMPORTANT:** Google Play requires 64-bit support (ARM64)

5. **Graphics API:**
   - Remove `Vulkan` if you have compatibility issues
   - Keep `OpenGLES3` and optionally `OpenGLES2`

---

### **4. Build Configuration**

1. **File → Build Settings**
2. **Platform:** Android
3. **Build System:** Gradle
4. **Export Project:** ❌ (leave unchecked)
5. **Build App Bundle (Google Play):** ✅ **ENABLE THIS!**
   - Google Play requires AAB (Android App Bundle) format
   - This creates a `.aab` file instead of `.apk`

---

### **5. AdMob App ID Verification**

**Your configuration is already correct!**

**Files to verify:**

1. **`GoogleMobileAdsSettings.asset`:**
   ```
   Location: Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset
   Android App ID: ca-app-pub-8609532543875876~3807758387 ✅
   ```

2. **`AndroidManifest.xml`:**
   ```
   Location: Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml
   App ID: ca-app-pub-8609532543875876~3807758387 ✅
   ```

Both files already have your correct App ID! ✅

---

### **6. Test Banner Ads Before Building**

1. **Open MainMenu scene**
2. **Press Play**
3. **Verify console shows:**
   ```
   [AdInitializer] Banner ad provider initialized: AdMob
   [AdMobBannerProvider] Initialized with ad unit ID: ca-app-pub-8609532543875876/5682573689
   [AdMobBannerProvider] Creating and loading banner ad...
   [BannerAdManager] Showing banner ad
   ```

4. **Check if banner appears at bottom of screen**

**Note:** In Unity Editor, you might see test ads or errors. Real ads will appear on actual Android devices.

---

## 🔨 Building for Internal Testing

### **Step-by-Step Build Process:**

1. **Verify Configuration:**
   ```
   ✅ AdSystem → InterstitialAdManager → Enable Ads: FALSE
   ✅ AdSystem → BannerAdManager → Enable Banners: TRUE
   ✅ Player Settings → Package Name set correctly
   ✅ Player Settings → Version and Version Code set
   ✅ Player Settings → Minimum API Level: 24+
   ✅ Player Settings → Target API Level: 34
   ✅ Player Settings → Scripting Backend: IL2CPP
   ✅ Player Settings → Target Architectures: ARM64 enabled
   ```

2. **Open Build Settings:**
   ```
   File → Build Settings
   Platform: Android
   Build App Bundle: ✅ TRUE
   ```

3. **Add Scenes:**
   ```
   Scenes in Build:
   ✅ MainMenu
   ✅ Gameplay Scene
   (Add all scenes your game uses)
   ```

4. **Click "Build":**
   - Choose output location
   - Wait for build to complete
   - Output: `YourGame.aab` file

5. **Sign the AAB:**
   - Unity will prompt you to create or use a keystore
   - **IMPORTANT:** Save your keystore file and password securely!
   - You'll need the same keystore for all future updates

---

## 📤 Uploading to Google Play Console

### **Prerequisites:**

1. **Google Play Developer Account** ($25 one-time fee)
2. **Create App in Google Play Console**
3. **Fill required store listing information**

### **Upload Process:**

1. **Go to Google Play Console:**
   - https://play.google.com/console

2. **Select Your App**

3. **Navigate to:**
   ```
   Production → Internal testing → Create new release
   ```

4. **Upload AAB:**
   - Drag and drop your `.aab` file
   - Or click "Upload" and select the file

5. **Release Notes:**
   - Add what's new in this version
   - Example: "Initial internal testing release"

6. **Add Testers:**
   ```
   Internal testing → Testers → Create email list
   Add email addresses of your testers
   ```

7. **Review and Rollout:**
   - Click "Review release"
   - Click "Start rollout to internal testing"

8. **Share Testing Link:**
   - Google Play will provide a testing link
   - Share with your testers
   - They can install the app from the link

---

## 🧪 Testing Banner Ads on Device

### **Important Notes:**

1. **Ads won't show immediately:**
   - AdMob needs time to serve ads to new app IDs (can take hours)
   - You might see "No fill" errors initially

2. **Test on real device, not emulator:**
   - Emulators don't receive real ads
   - Install the AAB on a physical Android device

3. **Don't click your own ads:**
   - AdMob will ban your account for invalid traffic
   - Add your device as a test device if needed

4. **Check logcat for ad events:**
   ```
   Unity → Window → Analysis → Android Logcat
   Filter by: AdMob, Banner
   ```

### **Expected Console Logs:**

**Success:**
```
[AdInitializer] Banner ad provider initialized: AdMob
[AdMobBannerProvider] Initialized with ad unit ID: ca-app-pub-8609532543875876/5682573689
[AdMobBannerProvider] Creating and loading banner ad...
[AdMobBannerProvider] ✅ Banner ad loaded successfully!
[AdMobBannerProvider] Banner ad impression recorded
```

**No Fill (normal for new apps):**
```
[AdMobBannerProvider] ❌ Banner ad failed to load
Error Code: 3
Error Message: No fill
```

**Common Issues:**
- **Error Code 3 (No fill):** AdMob has no ads to serve (normal initially)
- **Error Code 1 (Invalid request):** Check ad unit ID is correct
- **Error Code 2 (Network error):** Check internet connection

---

## ✅ Pre-Upload Checklist

Before uploading to Google Play Console:

```
✅ InterstitialAdManager → Enable Ads: FALSE
✅ BannerAdManager → Enable Banners: TRUE
✅ AdMobBannerProvider has correct ad unit ID
✅ GoogleMobileAdsSettings has correct app ID
✅ AndroidManifest.xml has correct app ID
✅ Package name matches Google Play Console
✅ Version code incremented from previous build
✅ Minimum API level 24+
✅ Target API level 34
✅ ARM64 architecture enabled
✅ Scripting backend: IL2CPP
✅ Build as AAB (Android App Bundle)
✅ Keystore created and saved securely
✅ All required scenes added to build
✅ Game tested in Unity Editor
```

---

## 🎯 Quick Build Commands

### **Build Settings Path:**
```
File → Build Settings → Android
```

### **Player Settings Path:**
```
Edit → Project Settings → Player → Android tab
```

### **Critical Player Settings:**

```csharp
// Company Name
YourCompanyName

// Product Name
Fruit Tactics

// Package Name (must match Google Play Console)
com.yourcompany.fruittactics

// Version
1.0.0

// Bundle Version Code
1 (increment for each upload: 2, 3, 4...)

// Minimum API Level
Android 7.0 (API level 24)

// Target API Level
Android 14.0 (API level 34)

// Scripting Backend
IL2CPP

// Target Architectures
✅ ARM64
☑️ ARMv7 (optional)
```

---

## 📊 AdMob Dashboard

### **Monitor Your Ads:**

1. **Go to AdMob:**
   - https://apps.admob.com

2. **Select Your App:**
   - `Fruit Tactics`

3. **Check Ad Units:**
   - Banner ad unit should show: `ca-app-pub-8609532543875876/5682573689`

4. **Monitor Performance:**
   - Impressions
   - Clicks
   - Estimated earnings
   - Fill rate

**Note:** Stats update with ~24 hour delay.

---

## 🚨 Common Issues & Solutions

### **Issue 1: Banner doesn't show**

**Solutions:**
```
1. Check internet connection
2. Verify ad unit ID is correct
3. Wait for AdMob to process new app (can take hours)
4. Check logcat for error messages
5. Ensure device is not in airplane mode
```

### **Issue 2: "App not signed" error**

**Solutions:**
```
1. Create keystore in Unity Build Settings
2. Set keystore password
3. Set key alias and password
4. Rebuild AAB with keystore
```

### **Issue 3: Google Play rejects AAB**

**Solutions:**
```
1. Verify target API level is 33+
2. Ensure ARM64 is enabled
3. Check package name matches console
4. Increment version code
5. Check for missing permissions in AndroidManifest
```

### **Issue 4: Ads show test ads instead of real ads**

**Solutions:**
```
1. Verify you're using production ad unit IDs (not test IDs)
2. Check AdMob account is approved
3. Remove test device IDs from code
4. Wait for AdMob to serve real ads (can take time)
```

---

## 🎉 Success Indicators

### **You'll know everything works when:**

1. **Build succeeds without errors** ✅
2. **AAB file is created** ✅
3. **Google Play accepts the upload** ✅
4. **Testers can install from internal testing link** ✅
5. **Banner ads show at bottom of screen** ✅ (may take hours)
6. **Game runs smoothly on device** ✅
7. **No crashes or errors** ✅

---

## 📱 Device Testing Recommendations

**Test on multiple devices:**
- Different Android versions (7.0+, 10, 12, 14)
- Different screen sizes (phone, tablet)
- Different manufacturers (Samsung, Google Pixel, etc.)

**Test scenarios:**
- Fresh install
- Launch game
- Navigate through menus
- Play levels
- Check banner visibility
- Check banner doesn't overlap UI
- Test with/without internet
- Background/resume app

---

## 🔑 Important Files to Backup

**Before uploading, backup these files:**

1. **Keystore file** (`.keystore` or `.jks`)
   - Location: Where you saved it during build
   - **CRITICAL:** You can't update your app without this!

2. **Keystore password**
3. **Key alias**
4. **Key password**

**Store these securely!** Losing your keystore means you can't update your app.

---

## 🎯 Next Steps After Internal Testing

1. **Test with internal testers (1-2 weeks)**
2. **Fix bugs and issues**
3. **Upload new builds with incremented version code**
4. **Move to Closed Testing (alpha)**
5. **Then Open Testing (beta)**
6. **Finally Production release**

Each stage allows more testers and collects feedback.

---

## 💡 Pro Tips

1. **Start version at 1.0.0** for easier tracking
2. **Use semantic versioning:** MAJOR.MINOR.PATCH (1.0.0, 1.0.1, 1.1.0)
3. **Increment version code for EVERY upload** (Unity auto-increments if you check the box)
4. **Test on lowest supported API level** (Android 7.0)
5. **Monitor Google Play Console for crash reports**
6. **Keep build logs** for troubleshooting
7. **Test banner ads in different scenes** (menu, gameplay, victory)
8. **Check banner doesn't cover important UI**
9. **Add privacy policy URL** (required by Google Play and AdMob)
10. **Review Google Play policies** before publishing

---

## 📞 Support Resources

**Google Play Console:**
- https://support.google.com/googleplay/android-developer

**AdMob Help:**
- https://support.google.com/admob

**Unity Build Issues:**
- https://docs.unity3d.com/Manual/android-BuildProcess.html

**Google Mobile Ads Unity Plugin:**
- https://developers.google.com/admob/unity/start

---

## ✅ Final Pre-Build Verification

Run through this checklist one more time before clicking Build:

```
□ Interstitial ads disabled
□ Banner ads enabled
□ Correct banner ad unit ID (ca-app-pub-8609532543875876/5682573689)
□ Package name set and matches Google Play Console
□ Version and version code set correctly
□ Minimum API 24+
□ Target API 34
□ IL2CPP scripting backend
□ ARM64 enabled
□ Build App Bundle enabled
□ Keystore configured
□ All scenes added to build
□ Game tested in editor
□ AdMob app ID verified in settings
□ AdMob app ID verified in AndroidManifest
```

---

**Good luck with your internal testing! 🚀**

Your ads are configured correctly, and you're ready to build and upload to Google Play Console.

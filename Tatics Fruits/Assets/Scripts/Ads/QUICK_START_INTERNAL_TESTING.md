# 🚀 Quick Start: Internal Testing Setup

## ✅ Your Configuration Status

### **Banner Ads** ✅
```
Android Ad Unit ID: ca-app-pub-8609532543875876/5682573689
AdMob App ID: ca-app-pub-8609532543875876~3807758387
Status: Configured and ready!
```

### **Interstitial Ads** ⚠️
```
Status: Need to disable for internal testing
```

---

## 📋 5-Minute Setup Checklist

### **Step 1: Disable Interstitial Ads** (2 minutes)

1. Open `MainMenu` scene
2. Select `AdSystem` GameObject in Hierarchy
3. Find `InterstitialAdManager` component
4. Set `Enable Ads` to **FALSE**
5. Save scene

**Or use the helper tool:**
```
Menu: Tools → Ads → Google Play Setup Checker
Click: "Disable Interstitial Ads" button
```

---

### **Step 2: Configure Player Settings** (3 minutes)

```
Edit → Project Settings → Player → Android
```

**Required Settings:**

| Setting | Value | Required |
|---------|-------|----------|
| **Package Name** | `com.yourname.fruittactics` | ✅ CRITICAL |
| **Version** | `1.0.0` | ✅ |
| **Version Code** | `1` | ✅ |
| **Minimum API Level** | `24` (Android 7.0) | ✅ |
| **Target API Level** | `34` (Android 14) | ✅ |
| **Scripting Backend** | `IL2CPP` | ✅ |
| **ARM64** | Enabled ✅ | ✅ CRITICAL |

**Important Notes:**
- **Package Name:** Must match exactly what you'll use in Google Play Console
- **Version Code:** Increment for each new upload (1, 2, 3...)
- **ARM64:** Required by Google Play, won't accept without it!

---

### **Step 3: Build Settings** (1 minute)

```
File → Build Settings
```

**Configuration:**

```
Platform: Android
Build System: Gradle
Build App Bundle (Google Play): ✅ ENABLE THIS!
```

**Add Scenes:**
```
✅ MainMenu
✅ Gameplay Scene
(Add all scenes your game uses)
```

---

### **Step 4: Create Keystore** (One-time, 2 minutes)

```
File → Build Settings → Player Settings → Publishing Settings
```

1. Click "Keystore Manager"
2. Create new keystore:
   - **Keystore file:** Choose location and name (e.g., `fruittactics.keystore`)
   - **Password:** Create strong password
   - **Alias:** Name it (e.g., `fruittactics_key`)
   - **Alias password:** Create password

**⚠️ CRITICAL:** Save this information securely! You can't update your app without it.

```
Keystore file: /path/to/fruittactics.keystore
Keystore password: [SAVE THIS!]
Alias: fruittactics_key
Alias password: [SAVE THIS!]
```

---

### **Step 5: Build!** (5-10 minutes)

1. Click **"Build"** in Build Settings
2. Choose output location
3. Name it: `FruitTactics_v1.0.0.aab`
4. Wait for build to complete

**Expected output:**
```
✅ FruitTactics_v1.0.0.aab (20-100 MB)
```

---

## 📤 Upload to Google Play Console

### **Prerequisites:**
- Google Play Developer account ($25 one-time)
- App created in Google Play Console

### **Upload Steps:**

1. **Go to:** https://play.google.com/console
2. **Select your app**
3. **Navigate to:** `Production → Internal testing → Create new release`
4. **Upload AAB:** Drag and drop your `.aab` file
5. **Add release notes:** "Initial internal testing release"
6. **Add testers:** Create email list with tester emails
7. **Click:** "Review release" → "Start rollout to internal testing"
8. **Share testing link** with your testers

---

## 🧪 Test Banner Ads

### **On Device:**

1. **Install from internal testing link**
2. **Open app**
3. **Check bottom of screen for banner**

### **Expected Behavior:**

**✅ Success:**
- Banner appears at bottom
- Doesn't cover game UI
- Shows actual ads (may take hours for new app)

**⚠️ Initial State:**
- Might show blank banner initially
- AdMob needs time to serve ads
- "No fill" errors are normal at first

### **Console Logs to Check:**

```
Connect device → Window → Analysis → Android Logcat
Filter: AdMob
```

**Good logs:**
```
✅ [AdMobBannerProvider] Banner ad loaded successfully
✅ [AdMobBannerProvider] Banner ad impression recorded
```

**Normal initial logs:**
```
ℹ️ [AdMobBannerProvider] Banner ad failed to load (Error: No fill)
```
This is normal! AdMob needs time to serve ads to new apps.

---

## ⚙️ Configuration Verification Tool

**Use the built-in checker:**

```
Menu: Tools → Ads → Google Play Setup Checker
```

This tool will:
- ✅ Check if interstitial ads are disabled
- ✅ Verify banner ads are enabled
- ✅ Check Player Settings
- ✅ Verify build settings
- ✅ Show what needs fixing

---

## 🎯 Pre-Upload Checklist

Before clicking "Build", verify:

```
□ MainMenu scene open
□ AdSystem → InterstitialAdManager → Enable Ads: FALSE
□ AdSystem → BannerAdManager → Enable Banners: TRUE
□ Player Settings → Package Name set correctly
□ Player Settings → Version: 1.0.0
□ Player Settings → Version Code: 1
□ Player Settings → Min API: 24+
□ Player Settings → Target API: 34
□ Player Settings → IL2CPP enabled
□ Player Settings → ARM64 enabled
□ Build Settings → Platform: Android
□ Build Settings → Build App Bundle: TRUE
□ Build Settings → All scenes added
□ Keystore created and saved
```

---

## 🚨 Common Issues

### **"Interstitial ads still showing"**
```
Solution: 
1. Check AdSystem → InterstitialAdManager → Enable Ads: FALSE
2. Also check AdInitializer → Initialize Interstitial: FALSE
```

### **"Build fails with ARM64 error"**
```
Solution:
1. Player Settings → Other Settings → Scripting Backend: IL2CPP
2. Player Settings → Other Settings → Target Architectures: ARM64 ✅
```

### **"Google Play rejects AAB"**
```
Solution:
1. Check Target API Level is 33+
2. Verify ARM64 is enabled
3. Ensure version code incremented
```

### **"Banner ads don't show"**
```
Solution:
1. Wait 24-48 hours for AdMob to process
2. Check internet connection on device
3. Verify ad unit ID in AdMobBannerProvider.cs
4. Check Android Logcat for errors
```

---

## 📊 What to Expect

### **Timeline:**

```
Day 0: Build and upload to Google Play
Day 0-1: Internal testers can install
Day 0-2: Banner ads might show "No fill"
Day 2-7: Banner ads start showing real ads
Week 2+: Stable ad serving
```

### **Performance:**

**Banner Ads:**
- Fill rate: 60-90% (varies by region)
- CPM: $0.50-$5.00 (varies widely)
- Impressions: Each time banner loads

**Don't click your own ads!** AdMob will ban your account.

---

## 🎉 Success Indicators

**You'll know it's working when:**

1. ✅ Build completes without errors
2. ✅ AAB file created (20-100 MB)
3. ✅ Google Play accepts upload
4. ✅ Testers can install from link
5. ✅ Game runs on device
6. ✅ Banner appears at bottom (may take time)
7. ✅ No crashes

---

## 📞 Need Help?

**Use the verification tool:**
```
Menu: Tools → Ads → Google Play Setup Checker
```

**Read full guide:**
```
Assets/Scripts/Ads/GOOGLE_PLAY_INTERNAL_TESTING_SETUP.md
```

**Check AdMob:**
- https://apps.admob.com

**Google Play Console:**
- https://play.google.com/console

---

## 🔑 Remember!

**SAVE YOUR KEYSTORE:**
- File location
- Keystore password
- Key alias name
- Key alias password

**Losing this = can't update your app ever!**

Store in:
- Password manager
- Secure cloud storage
- Multiple backups

---

## 🚀 Next Steps

1. **Build AAB** following this guide
2. **Upload to Google Play** internal testing
3. **Add testers** (friends, colleagues)
4. **Test for 1-2 weeks**
5. **Fix bugs** and upload new versions
6. **Move to closed testing** (alpha)
7. **Then open testing** (beta)
8. **Finally production** release

**Good luck! 🎉**

Your banner ads are already configured correctly. Just disable interstitials and build!

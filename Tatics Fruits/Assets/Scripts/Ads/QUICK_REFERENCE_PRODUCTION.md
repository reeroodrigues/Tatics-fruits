# 🎯 Quick Reference - Production Build

## ✅ What Changed

### **1. AdDebugUI (Debug Panel)**
```
Unity Editor:         ✅ Visible (Press F1)
Development Build:    ✅ Visible (Press F1)
Production Build:     ❌ Automatically Hidden
```

### **2. Banner Ads**
```
MainMenu Scene:       ✅ Shows Banner
Gameplay Scene:       ❌ Hides Banner
Other Scenes:         ❌ Hides Banner (unless configured)
```

---

## ⚙️ Configuration

### **AdSystem GameObject Settings:**

```
AdSystem
├── BannerAdManager
│   ├── Enable Banners: TRUE
│   ├── Show Banner On Start: TRUE
│   └── Scenes With Banners
│       ├── Size: 1
│       └── Element 0: "MainMenu"
│
├── InterstitialAdManager
│   └── Enable Ads: FALSE
│
└── AdInitializer
    ├── Initialize Interstitial: FALSE
    └── Initialize Banner: TRUE
```

---

## 🧪 Testing Steps

### **In Unity Editor:**

1. **Play MainMenu scene:**
   - ✅ See AdDebugUI panel (top-left)
   - ✅ See banner at bottom
   - ✅ Console: `Banner allowed: True`

2. **Load Gameplay Scene:**
   - ✅ AdDebugUI still visible
   - ❌ Banner disappears
   - ✅ Console: `Banner allowed: False`

3. **Back to MainMenu:**
   - ✅ Banner reappears automatically

### **In Production Build:**

1. **Build AAB for Android**
2. **Install and run on device:**
   - ❌ No debug panel
   - ✅ Banner in MainMenu
   - ❌ No banner in Gameplay

---

## 📝 Console Logs

### **Expected Logs:**

**MainMenu:**
```
[BannerAdManager] Scene: MainMenu, Banner allowed: True
[BannerAdManager] Showing banner ad
[AdMobBannerProvider] ✅ Banner ad loaded successfully!
```

**Gameplay Scene:**
```
[BannerAdManager] Scene: Gameplay Scene, Banner allowed: False
[BannerAdManager] Hiding banner ad
```

**Production Build:**
```
[AdDebugUI] Debug UI disabled in production build
```

---

## 🎯 To Add More Scenes With Banners

1. **Select AdSystem** in MainMenu
2. **Find BannerAdManager component**
3. **Expand "Scenes With Banners"**
4. **Increase Size** (e.g., from 1 to 2)
5. **Add scene name** in new element

**Example:**
```
Scenes With Banners
├── Size: 2
├── Element 0: "MainMenu"
└── Element 1: "VictoryScreen"
```

---

## ✅ Pre-Build Checklist

```
□ AdDebugUI will auto-hide (no action needed)
□ BannerAdManager → Scenes With Banners: ["MainMenu"]
□ InterstitialAdManager → Enable Ads: FALSE
□ BannerAdManager → Enable Banners: TRUE
□ Tested in Editor: Banner shows/hides correctly
```

---

## 🚀 Ready to Build!

**Your ad system is production-ready:**
- ✅ No debug UI in builds
- ✅ Banner only in MainMenu
- ✅ Clean gameplay experience
- ✅ Real ad unit IDs configured

**Next:** Build AAB and upload to Google Play Console!

See `/Assets/Scripts/Ads/PRODUCTION_BUILD_SETUP.md` for full details.

# 🎯 Production Build Setup

## ✅ Changes Made for Internal Testing

### **1. AdDebugUI - Hidden in Production Builds**

The debug UI panel is now **automatically disabled** in production builds.

**Behavior:**

| Build Type | AdDebugUI Status |
|------------|------------------|
| **Unity Editor** | ✅ Visible (Press F1 to toggle) |
| **Development Build** | ✅ Visible (Press F1 to toggle) |
| **Production Build** | ❌ Hidden (Disabled) |

**How it works:**
```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Debug UI is created and active
#else
    // Debug UI is disabled in production
#endif
```

**No action required!** The debug panel will automatically be hidden when you build for Google Play internal testing.

---

### **2. Banner Ads - Show Only in MainMenu**

Banners now **automatically show/hide** based on the current scene.

**Behavior:**

| Scene | Banner Status |
|-------|---------------|
| **MainMenu** | ✅ Showing |
| **Gameplay Scene** | ❌ Hidden |
| **Other Scenes** | ❌ Hidden |

**Configuration:**

1. **Select `AdSystem` GameObject** in MainMenu scene
2. **Find `BannerAdManager` component**
3. **Check the `Scenes With Banners` list:**
   ```
   Scenes With Banners
   ├── Size: 1
   └── Element 0: "MainMenu"
   ```

**To add more scenes with banners:**
1. Increase `Size` to add more entries
2. Add scene names (e.g., "VictoryScreen", "LevelSelect")
3. Banner will show in those scenes automatically

**Console Logs:**
```
[BannerAdManager] Scene: MainMenu, Banner allowed: True
[BannerAdManager] Showing banner ad

[BannerAdManager] Scene: Gameplay Scene, Banner allowed: False
[BannerAdManager] Hiding banner ad
```

---

## 🔧 BannerAdManager Configuration

### **Inspector Settings:**

```
AdSystem
└── BannerAdManager
    ├── Show Banner On Start: TRUE
    ├── Enable Banners: TRUE
    └── Scenes With Banners
        ├── Size: 1
        └── Element 0: "MainMenu"
```

### **Add Banner to More Scenes:**

**Example: Show banner in MainMenu and VictoryScreen:**
```
Scenes With Banners
├── Size: 2
├── Element 0: "MainMenu"
└── Element 1: "VictoryScreen"
```

**Example: Show banner in all scenes except gameplay:**
```
Scenes With Banners
├── Size: 3
├── Element 0: "MainMenu"
├── Element 1: "LevelSelect"
└── Element 2: "Settings"
```

---

## 🧪 Testing

### **In Unity Editor:**

1. **Open MainMenu scene**
2. **Press Play**
3. **Expected:**
   - ✅ AdDebugUI panel visible in top-left
   - ✅ Banner shows at bottom
   - ✅ Console log: `Banner allowed: True`

4. **Load Gameplay Scene**
5. **Expected:**
   - ✅ AdDebugUI still visible (you can toggle with F1)
   - ❌ Banner hidden
   - ✅ Console log: `Banner allowed: False`

### **In Production Build:**

1. **Build for Android** (Build App Bundle)
2. **Install on device**
3. **Launch app**
4. **Expected:**
   - ❌ No AdDebugUI panel
   - ✅ Banner shows in MainMenu
   - ❌ Banner hidden in Gameplay Scene

---

## 📋 Pre-Build Checklist

Before building for Google Play:

```
✅ AdDebugUI will auto-hide in production (no action needed)
✅ BannerAdManager → Scenes With Banners: ["MainMenu"]
✅ InterstitialAdManager → Enable Ads: FALSE
✅ BannerAdManager → Enable Banners: TRUE
✅ Test in Editor: Banner shows in MainMenu, hides in Gameplay
```

---

## 🎯 How Scene-Based Banners Work

### **Automatic Scene Detection:**

```csharp
// When scene loads
OnSceneLoaded()
├── Check if current scene is in allowedScenes list
├── If allowed → Show banner
└── If not allowed → Hide banner
```

### **Scene Transitions:**

```
MainMenu (banner shows)
    ↓ [Load Gameplay Scene]
Gameplay Scene (banner auto-hides)
    ↓ [Back to MainMenu]
MainMenu (banner auto-shows)
```

### **Benefits:**

1. ✅ **Clean gameplay** - No banner covering game UI
2. ✅ **Better UX** - Banner only in menu screens
3. ✅ **Automatic** - No manual show/hide calls needed
4. ✅ **Flexible** - Easy to add more scenes with banners

---

## 🔍 Debug Logs to Check

### **MainMenu Scene:**
```
[BannerAdManager] Scene: MainMenu, Banner allowed: True
[BannerAdManager] Banner provider set: AdMobBannerProvider
[AdMobBannerProvider] Initialized with ad unit ID: ca-app-pub-8609532543875876/5682573689
[BannerAdManager] Showing banner ad
[AdMobBannerProvider] Creating and loading banner ad...
[AdMobBannerProvider] ✅ Banner ad loaded successfully!
```

### **Gameplay Scene:**
```
[BannerAdManager] Scene: Gameplay Scene, Banner allowed: False
[BannerAdManager] Banner not allowed in scene: Gameplay Scene
[AdMobBannerProvider] Hiding banner ad
```

### **Production Build (AdDebugUI):**
```
[AdDebugUI] Debug UI disabled in production build
```

---

## 🎨 Scene Name Reference

**Important:** Scene names must match exactly (case-sensitive)!

Check your scene names:
```
File → Build Settings → Scenes In Build
```

Current scenes:
- ✅ `MainMenu` (banner shows)
- ❌ `Gameplay Scene` (banner hidden)

If your scene is named differently (e.g., "Main Menu" with space), update the `Scenes With Banners` array accordingly.

---

## 🚀 Quick Verification

### **Step 1: Check AdSystem Configuration**
```
MainMenu scene → AdSystem GameObject → BannerAdManager
└── Scenes With Banners: ["MainMenu"]
```

### **Step 2: Test in Editor**
```
1. Play MainMenu → Banner shows ✅
2. Load Gameplay → Banner hides ✅
3. Back to MainMenu → Banner shows ✅
```

### **Step 3: Build and Test**
```
1. Build AAB for Android
2. Install on device
3. Launch app
4. Verify: No debug UI ✅, Banner in MainMenu ✅, No banner in Gameplay ✅
```

---

## 💡 Advanced Configuration

### **Show banner only during gameplay:**
```
Scenes With Banners
├── Size: 1
└── Element 0: "Gameplay Scene"
```

### **Show banner in all scenes:**
```
Scenes With Banners
├── Size: N (number of scenes)
├── Element 0: "MainMenu"
├── Element 1: "Gameplay Scene"
├── Element 2: "VictoryScreen"
└── Element N-1: "Settings"
```

### **Programmatically control banner:**
```csharp
// Still works! Manual control overrides scene settings
BannerAdManager.Instance.ShowBanner();
BannerAdManager.Instance.HideBanner();
```

---

## ✅ Summary

**What you get:**

1. ✅ **AdDebugUI automatically hidden** in production builds
2. ✅ **Banner shows only in MainMenu** scene
3. ✅ **Banner auto-hides in Gameplay** Scene
4. ✅ **Automatic scene-based control** - no manual code needed
5. ✅ **Easy to configure** via Inspector
6. ✅ **Production-ready** for Google Play internal testing

**No additional code changes needed!**

Just build your AAB and upload to Google Play Console.

---

## 🎉 Ready for Internal Testing!

Your ad system is now configured for production:

- ✅ Debug UI hidden in builds
- ✅ Banner only in MainMenu
- ✅ Clean gameplay without ads
- ✅ Production ad unit IDs configured
- ✅ Interstitial ads disabled

**Build, upload, and test!** 🚀

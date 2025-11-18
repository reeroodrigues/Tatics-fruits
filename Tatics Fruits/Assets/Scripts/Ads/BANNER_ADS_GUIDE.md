# 📱 Banner Ads Integration Guide

## ✅ What's Already Set Up

Your banner ad system is **ready to use**! Here's what's configured:

### **Components Created:**
- ✅ `IBannerAdProvider` - Interface for banner providers
- ✅ `AdMobBannerProvider` - Real AdMob banner ads
- ✅ `TestBannerProvider` - Test banners for Editor
- ✅ `BannerAdManager` - Singleton manager
- ✅ Updated `AdInitializer` - Auto-initializes banners
- ✅ Updated `AdDebugUI` - Banner toggle controls

### **Test Ad Unit IDs (Already Configured):**
- **Android:** `ca-app-pub-3940256099942544/6300978111`
- **iOS:** `ca-app-pub-3940256099942544/2934735716`

---

## 🚀 How to Use Banner Ads

### **1. Automatic Setup (Recommended)**

If you're using `AdSystemSetup`, banners are **already configured**!

Just press Play and you'll see:
- ✅ Banner appears at bottom of screen
- ✅ "TOGGLE BANNER" button in debug panel
- ✅ Banner status indicator

### **2. Manual Setup (If Not Using AdSystemSetup)**

Add these components to a GameObject:
1. `BannerAdManager`
2. `AdInitializer` (set to AdMob)

The banner will automatically appear on Start.

---

## 🎮 Using Banners in Your Scene

### **Show Banner:**
```csharp
using Ads;

BannerAdManager.Instance.ShowBanner();
```

### **Hide Banner:**
```csharp
BannerAdManager.Instance.HideBanner();
```

### **Destroy Banner:**
```csharp
BannerAdManager.Instance.DestroyBanner();
```

### **Check if Banner is Showing:**
```csharp
bool isShowing = BannerAdManager.Instance.IsBannerShowing();
```

### **Enable/Disable Banners:**
```csharp
// Disable banners completely
BannerAdManager.Instance.EnableBanners(false);

// Re-enable banners
BannerAdManager.Instance.EnableBanners(true);
```

---

## 🎯 Common Use Cases

### **Example 1: Show Banner Only in Main Menu**

In your MainMenu scene script:
```csharp
using Ads;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {
        if (BannerAdManager.Instance != null)
        {
            BannerAdManager.Instance.ShowBanner();
        }
    }

    private void OnDestroy()
    {
        if (BannerAdManager.Instance != null)
        {
            BannerAdManager.Instance.HideBanner();
        }
    }
}
```

### **Example 2: Hide Banner During Gameplay**

In your GameManager:
```csharp
using Ads;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // Hide banner when game starts
        if (BannerAdManager.Instance != null)
        {
            BannerAdManager.Instance.HideBanner();
        }
    }

    public void OnGameOver()
    {
        // Show banner when game ends
        if (BannerAdManager.Instance != null)
        {
            BannerAdManager.Instance.ShowBanner();
        }
    }
}
```

### **Example 3: Scene-Specific Banner Control**

```csharp
using Ads;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (BannerAdManager.Instance == null) return;

        // Show banners only in main menu and store
        if (scene.name == "MainMenu" || scene.name == "Store")
        {
            BannerAdManager.Instance.ShowBanner();
        }
        else
        {
            BannerAdManager.Instance.HideBanner();
        }
    }
}
```

---

## 🛠️ Configuration Options

### **BannerAdManager Inspector Settings:**

- **Show Banner On Start** - Auto-show banner when scene loads
- **Enable Banners** - Master switch for all banners

### **Banner Position:**

By default, banners appear at **bottom** of screen.

To change position, edit `/Assets/Scripts/Ads/AdMobBannerProvider.cs`:

```csharp
// Current: Bottom
_bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);

// Options:
AdPosition.Top
AdPosition.Bottom
AdPosition.TopLeft
AdPosition.TopRight
AdPosition.BottomLeft
AdPosition.BottomRight
AdPosition.Center
```

### **Banner Size:**

Change the `AdSize` parameter:

```csharp
AdSize.Banner              // 320x50
AdSize.MediumRectangle     // 300x250
AdSize.IABBanner           // 468x60
AdSize.Leaderboard         // 728x90
AdSize.SmartBanner         // Screen-wide adaptive
```

---

## 📊 Testing

### **In Unity Editor:**
1. Press Play
2. You'll see a **gray test banner** at bottom with "🎮 TEST BANNER AD 🎮"
3. Click **TOGGLE BANNER** in debug panel to hide/show

### **On Android Device:**
1. Build and install APK
2. Banner should appear immediately (using test ad unit ID)
3. Use debug panel to toggle banner on/off

### **Expected Logs:**
```
[AdInitializer] Banner ad provider initialized: AdMob
[AdMobBannerProvider] Creating and loading banner ad
[AdMobBannerProvider] ✅ Banner ad loaded successfully!
[BannerAdManager] Showing banner ad
```

---

## 🎨 UI Layout Considerations

### **Safe Area for Bottom Banners:**

Banners take up **50-90 pixels** at the bottom of the screen. Make sure your UI doesn't overlap!

**Recommended approach:**
1. Add a `LayoutElement` or padding to your main Canvas
2. Reserve 100px at the bottom for the banner

Example:
```csharp
using UnityEngine;
using UnityEngine.UI;

public class SafeAreaBannerAdjuster : MonoBehaviour
{
    [SerializeField] private RectTransform contentArea;
    
    private void Start()
    {
        AdjustForBanner();
    }

    private void AdjustForBanner()
    {
        // Add 60px padding at bottom for banner
        contentArea.offsetMin = new Vector2(contentArea.offsetMin.x, 60);
    }
}
```

---

## 🔄 Switching to Real Ad Unit IDs

When you're ready to use real ads:

1. **Get Your Banner Ad Unit ID** from AdMob dashboard
2. **Update `AdMobBannerProvider.cs`:**

```csharp
// Replace test IDs with your real IDs:
private const string ANDROID_AD_UNIT_ID = "ca-app-pub-XXXXXXXXXXXXXXXX/YYYYYYYYYY";
private const string IOS_AD_UNIT_ID = "ca-app-pub-XXXXXXXXXXXXXXXX/YYYYYYYYYY";
```

3. Rebuild and test!

---

## 📱 Best Practices

✅ **DO:**
- Show banners in menus, not during active gameplay
- Hide banners during fullscreen events
- Use adaptive banner sizes for better fill rates
- Test on real devices

❌ **DON'T:**
- Show banners during cutscenes or tutorials
- Overlap interactive UI elements with banners
- Show multiple banner ads simultaneously
- Refresh banners manually (AdMob handles this)

---

## 🐛 Troubleshooting

### **Banner not showing:**
1. Check `BannerAdManager` has `Enable Banners` checked
2. Check Console for error logs
3. Verify internet connection on device
4. Make sure `AdInitializer` has `Initialize Banner` checked

### **Banner overlaps UI:**
- Add bottom padding to your Canvas
- Move important UI elements up

### **Test banner works but real banner doesn't:**
- Wait 1-2 hours after creating ad unit in AdMob
- Verify ad unit ID is correct
- Check device logs with `adb logcat -s Unity`

---

## 🎯 Summary

Your banner ad system is **fully functional** and ready to use!

**Quick Test:**
1. Press Play in Unity
2. Look for test banner at bottom
3. Click **TOGGLE BANNER** in debug panel
4. Build APK to test real AdMob banners

**To integrate in your game:**
- Banners auto-show on Start (if enabled)
- Use `BannerAdManager.Instance.HideBanner()` during gameplay
- Use `BannerAdManager.Instance.ShowBanner()` in menus

Happy monetizing! 💰

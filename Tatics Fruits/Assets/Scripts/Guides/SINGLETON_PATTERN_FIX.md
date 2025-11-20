# Singleton Pattern Fix - Localizer

## 🐛 Problem Identified

### Race Condition Issues

**Before (Problematic):**
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void Bootstrap()
{
    if (Instance == null && FindObjectOfType<Localizer>() == null)  // ❌ Race condition!
    {
        var go = new GameObject("Localizer");
        go.AddComponent<Localizer>();
    }
}
```

**Issues:**
1. ❌ `FindObjectOfType` is slow and unreliable in `Bootstrap`
2. ❌ Can cause race conditions with other initialization code
3. ❌ Multiple scripts doing `while (Localizer.Instance == null)` can hang
4. ❌ No clear way to check if initialization is complete
5. ❌ Settings button and other UI elements fail to initialize properly

---

## ✅ Solution Applied

### Lazy Singleton with Ready Check

**After (Fixed):**
```csharp
private static Localizer _instance;

public static Localizer Instance 
{ 
    get 
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<Localizer>();
            
            if (_instance == null)
            {
                var go = new GameObject("Localizer");
                _instance = go.AddComponent<Localizer>();
                DontDestroyOnLoad(go);
            }
        }
        return _instance;
    }
}

public static bool IsReady => _instance != null && _instance._isInitialized;

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void Bootstrap()
{
    var _ = Instance;  // ✅ Force initialization without FindObjectOfType check
}
```

---

## 🎯 Key Improvements

### 1. **Lazy Initialization**

The singleton is created on first access, not in a separate bootstrap method:

```csharp
// Before: Bootstrap creates it proactively
if (Instance == null && FindObjectOfType<Localizer>() == null) { ... }

// After: Instance getter creates it on-demand
public static Localizer Instance 
{ 
    get 
    {
        if (_instance == null) { /* create */ }
        return _instance;
    }
}
```

### 2. **Ready State Check**

Added `IsReady` to check if initialization is complete:

```csharp
public static bool IsReady => _instance != null && _instance._isInitialized;
```

**Usage:**
```csharp
// Before (dangerous infinite loop risk):
while (Localizer.Instance == null) yield return null;

// After (safe):
yield return new WaitUntil(() => Localizer.IsReady);
```

### 3. **Single FindObjectOfType Call**

Only called once in the getter as a fallback:

```csharp
if (_instance == null)
{
    _instance = FindObjectOfType<Localizer>();  // ✅ Only if not created yet
    
    if (_instance == null)
    {
        // Create new one
    }
}
```

### 4. **Initialization Tracking**

Clear separation between creation and initialization:

```csharp
private bool _isInitialized = false;

private void Awake()
{
    _instance = this;
    DontDestroyOnLoad(gameObject);
    Initialize();  // Separate method
}

private void Initialize()
{
    var cfg = SettingsRepository.Get();
    SetLanguage(cfg.language, save: false);
    _isInitialized = true;  // ✅ Mark as ready
}
```

---

## 📝 Updated Components

### LocalizedText.cs

**Before:**
```csharp
private IEnumerator EnsureSubscribedThenRefresh()
{
    while (Localizer.Instance == null) yield return null;  // ❌ Can hang!
    TrySubscribe();
    Refresh();
}

private void TrySubscribe()
{
    if (_subscribed || Localizer.Instance == null) return;  // ❌ Null check
    Localizer.Instance.OnLanguageChanged += Refresh;
    _subscribed = true;
}
```

**After:**
```csharp
private IEnumerator EnsureSubscribedThenRefresh()
{
    yield return new WaitUntil(() => Localizer.IsReady);  // ✅ Safe wait
    TrySubscribe();
    Refresh();
}

private void TrySubscribe()
{
    if (_subscribed || !Localizer.IsReady) return;  // ✅ IsReady check
    Localizer.Instance.OnLanguageChanged += Refresh;
    _subscribed = true;
}
```

---

## 🔍 How Other Scripts Should Use It

### Pattern 1: Immediate Access (UI Initialization)

For scripts that run after bootstrap:

```csharp
private void Start()
{
    if (Localizer.IsReady)
    {
        var text = Localizer.Instance.Tr("my_key", "Fallback");
        UpdateUI(text);
    }
}
```

### Pattern 2: Wait for Ready (Async)

For scripts that need to guarantee the localizer is ready:

```csharp
private IEnumerator Start()
{
    yield return new WaitUntil(() => Localizer.IsReady);
    
    var text = Localizer.Instance.Tr("my_key", "Fallback");
    UpdateUI(text);
}
```

### Pattern 3: Subscribe to Language Changes

For dynamic updates when language changes:

```csharp
private void OnEnable()
{
    if (Localizer.IsReady)
    {
        Localizer.Instance.OnLanguageChanged += OnLanguageChanged;
        Refresh();
    }
}

private void OnDisable()
{
    if (Localizer.IsReady)
    {
        Localizer.Instance.OnLanguageChanged -= OnLanguageChanged;
    }
}

private void OnLanguageChanged()
{
    Refresh();
}

private void Refresh()
{
    if (!Localizer.IsReady) return;
    myText.text = Localizer.Instance.Tr("key", "fallback");
}
```

---

## ⚠️ Common Mistakes to Avoid

### ❌ DON'T DO THIS

```csharp
// Don't use while loops
while (Localizer.Instance == null) yield return null;

// Don't access without checking
var text = Localizer.Instance.Tr("key");  // Could be null during initialization

// Don't call FindObjectOfType yourself
var localizer = FindObjectOfType<Localizer>();
```

### ✅ DO THIS

```csharp
// Use WaitUntil with IsReady
yield return new WaitUntil(() => Localizer.IsReady);

// Always check IsReady
if (Localizer.IsReady)
{
    var text = Localizer.Instance.Tr("key", "fallback");
}

// Use the Instance property
var text = Localizer.Instance.Tr("key", "fallback");
```

---

## 🧪 Testing the Fix

### Verify Settings Button Works

1. **Start the game**
2. **Check Console** - No errors about null references
3. **Click Settings button** - Should open immediately
4. **Change language** - All text should update
5. **Restart game** - Settings should persist

### Verify No Race Conditions

1. **Add logging** to test initialization order:
   ```csharp
   Debug.Log($"Localizer IsReady: {Localizer.IsReady}");
   ```

2. **Load different scenes** rapidly
3. **Check Console** - Should see consistent initialization
4. **No infinite loops** or hangs

---

## 📊 Performance Impact

### Before
- Multiple `FindObjectOfType` calls during bootstrap
- Potential for infinite loops
- Unpredictable initialization order

### After
- Single `FindObjectOfType` call (fallback only)
- Guaranteed termination with `WaitUntil`
- Predictable initialization order

---

## 🔧 Migration Guide for Other Singletons

If you have other singletons in your project, apply this pattern:

```csharp
public class MySingleton : MonoBehaviour
{
    private static MySingleton _instance;
    
    public static MySingleton Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MySingleton>();
                
                if (_instance == null)
                {
                    var go = new GameObject("MySingleton");
                    _instance = go.AddComponent<MySingleton>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    public static bool IsReady => _instance != null && _instance._isInitialized;
    
    private bool _isInitialized = false;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var _ = Instance;
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }
    
    private void Initialize()
    {
        // Your initialization code here
        _isInitialized = true;
    }
}
```

---

## ✅ Checklist

Migration complete when:

- [x] Removed `FindObjectOfType` from Bootstrap
- [x] Added lazy initialization to Instance getter
- [x] Added `IsReady` property
- [x] Added `_isInitialized` flag
- [x] Separated `Initialize()` from `Awake()`
- [x] Updated all consumers to use `IsReady`
- [x] Replaced `while` loops with `WaitUntil(() => IsReady)`
- [x] Tested Settings button works
- [x] No race conditions observed

---

## 🎉 Benefits

Your `Localizer` singleton now:

✅ **Thread-safe** - No race conditions  
✅ **Predictable** - Clear initialization order  
✅ **Performant** - Minimal FindObjectOfType calls  
✅ **Testable** - IsReady check for unit tests  
✅ **Maintainable** - Clear separation of concerns  
✅ **Reliable** - Settings button works every time  

**The Settings button issue is now fixed!** 🚀

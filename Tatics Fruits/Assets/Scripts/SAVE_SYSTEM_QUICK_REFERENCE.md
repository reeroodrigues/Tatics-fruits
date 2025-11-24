# Save System - Quick Reference

## 🚀 Quick Start

### 1. Create Save Data Class

```csharp
using System;
using Core.SaveSystem;

[Serializable]
public class MyData : ISaveData
{
    public int value;
    
    public string GetFileName() => "my_data.json";
    public void OnBeforeSave() { /* validate */ }
    public void OnAfterLoad() { /* initialize */ }
}
```

### 2. Save and Load

```csharp
using Core.SaveSystem;

// Load (auto-creates if doesn't exist)
var data = SaveManager.Instance.Load<MyData>();

// Modify
data.value = 100;

// Save immediately
SaveManager.Instance.Save(data);

// OR mark for auto-save (saves every 60s)
SaveManager.Instance.MarkDirty<MyData>();
```

---

## 📋 Common Operations

### Load Data
```csharp
var profile = SaveManager.Instance.Load<PlayerProfileData>();
```

### Save Immediately
```csharp
SaveManager.Instance.Save(profile);
```

### Mark for Auto-Save
```csharp
SaveManager.Instance.MarkDirty<PlayerProfileData>();
```

### Get Cached Data (Fast)
```csharp
var cached = SaveManager.Instance.GetCached<PlayerProfileData>();
```

### Check If Exists
```csharp
if (SaveManager.Instance.Exists<PlayerProfileData>())
{
    // Save file exists
}
```

### Delete Save
```csharp
SaveManager.Instance.Delete<PlayerProfileData>();
```

### Delete All Saves
```csharp
SaveManager.Instance.DeleteAll();  // ⚠️ Use with caution!
```

### Get Last Save Time
```csharp
var lastSave = SaveManager.Instance.GetLastSaveTime<PlayerProfileData>();
if (lastSave.HasValue)
{
    Debug.Log($"Last saved: {lastSave.Value}");
}
```

---

## 📋 ISaveData Interface

All save data classes must implement:

```csharp
using System;
using Core.SaveSystem;

[Serializable]  // ⚠️ Required!
public class GameData : ISaveData
{
    // Your data fields
    public int score;
    public List<string> items = new();

    // Required: Return the save file name
    public string GetFileName() => "game_data.json";

    // Called before saving (validate/prepare data)
    public void OnBeforeSave()
    {
        score = Mathf.Max(0, score);  // Ensure valid
    }

    // Called after loading (initialize/fix null refs)
    public void OnAfterLoad()
    {
        if (items == null) items = new();  // Prevent null
    }
}
```

---

## 🎯 When to Use Each Method

| Method | Use Case | Performance |
|--------|----------|-------------|
| `Load<T>()` | First access to data | ~5ms (disk read) |
| `GetCached<T>()` | Frequent read-only access | ~0.001ms (memory) |
| `Save<T>()` | Critical data (purchases) | ~2-5ms (disk write) |
| `MarkDirty<T>()` | Frequent updates (score) | ~0.001ms (flag only) |
| `SaveDirty()` | Manual batch save | ~2-5ms per object |
| `SaveAll()` | App quit, pause | Variable |

---

## ✅ Best Practices Checklist

When creating new save data:

- [ ] Class is marked `[Serializable]`
- [ ] Implements `ISaveData` interface
- [ ] `GetFileName()` returns unique file name
- [ ] `OnAfterLoad()` initializes null collections
- [ ] `OnBeforeSave()` validates data ranges
- [ ] Use `List<T>` not arrays (better serialization)
- [ ] Avoid circular references
- [ ] Use immediate save for critical data
- [ ] Use dirty tracking for frequent updates

---

## 🎮 Usage Patterns

### Pattern 1: Service Class

```csharp
public class ProfileService
{
    private PlayerProfileData _profile;

    public void Initialize()
    {
        _profile = SaveManager.Instance.Load<PlayerProfileData>();
    }

    public void AddGold(int amount)
    {
        _profile.gold += amount;
        SaveManager.Instance.Save(_profile);  // Important!
    }
}
```

### Pattern 2: MonoBehaviour

```csharp
public class GameManager : MonoBehaviour
{
    private GameData _gameData;

    private void Start()
    {
        _gameData = SaveManager.Instance.Load<GameData>();
    }

    public void UpdateScore(int score)
    {
        _gameData.score = score;
        SaveManager.Instance.MarkDirty<GameData>();  // Auto-save
    }

    private void OnApplicationQuit()
    {
        SaveManager.Instance.SaveAll();  // Final save
    }
}
```

### Pattern 3: Event Subscription

```csharp
private void Start()
{
    SaveManager.Instance.OnSaved += HandleSaved;
}

private void HandleSaved(Type dataType)
{
    if (dataType == typeof(PlayerProfileData))
    {
        ShowSaveIndicator();
    }
}

private void OnDestroy()
{
    SaveManager.Instance.OnSaved -= HandleSaved;
}
```

---

## 🎛️ SaveManager Configuration

Access via Unity Inspector or code:

```csharp
var sm = SaveManager.Instance;

sm.autoSaveEnabled = true;      // Enable auto-save
sm.autoSaveInterval = 60f;      // Save every 60 seconds
sm.enableBackup = true;         // Create backups
sm.maxBackups = 3;              // Keep last 3 backups
```

---

## 🐛 Common Mistakes

### ❌ Forgot [Serializable]
```csharp
public class MyData : ISaveData  // ❌ Won't save!
{
    public int value;
}
```

### ✅ Correct
```csharp
[Serializable]  // ✅ Required!
public class MyData : ISaveData
{
    public int value;
}
```

---

### ❌ Null Collection
```csharp
public void OnAfterLoad() { }  // ❌ items can be null!
```

### ✅ Correct
```csharp
public void OnAfterLoad()
{
    if (items == null) items = new();  // ✅ Initialize
}
```

---

### ❌ Saving Too Often
```csharp
void Update()
{
    score++;
    SaveManager.Instance.Save(data);  // ❌ Every frame!
}
```

### ✅ Correct
```csharp
void Update()
{
    score++;
}

void OnScoreChanged()
{
    SaveManager.Instance.MarkDirty<GameData>();  // ✅ Batch
}
```

---

## 🔍 Debugging

### Get Save File Location
```csharp
Debug.Log(SaveManager.Instance.GetSavePath());
// Windows: C:/Users/[User]/AppData/LocalLow/[Company]/[Game]/
// Mac: ~/Library/Application Support/[Company]/[Game]/
```

### View Save File
Save files are JSON - you can open and edit them!

```json
{
    "playerName": "MyPlayer",
    "gold": 500,
    "level": 5
}
```

### Check Auto-Save
```csharp
// Auto-save triggers every 60 seconds
// Or manually:
SaveManager.Instance.SaveDirty();
```

---

## 📊 Available Save Events

```csharp
SaveManager.Instance.OnSaved += (type) => { };
SaveManager.Instance.OnLoaded += (type) => { };
SaveManager.Instance.OnSaveError += (type, ex) => { };
SaveManager.Instance.OnLoadError += (type, ex) => { };
```

---

## 🎯 Examples

### Player Profile
```csharp
var profile = SaveManager.Instance.Load<PlayerProfileData>();
profile.gold += 100;
SaveManager.Instance.Save(profile);
```

### High Score
```csharp
var highScore = SaveManager.Instance.Load<HighScoreData>();
if (score > highScore.score)
{
    highScore.score = score;
    SaveManager.Instance.Save(highScore);
}
```

### Settings
```csharp
var settings = SaveManager.Instance.Load<GameSettingsData>();
settings.musicVolume = 0.8f;
SaveManager.Instance.MarkDirty<GameSettingsData>();  // Auto-save
```

---

## 📞 Need Help?

1. Check `/Assets/Scripts/SAVE_SYSTEM_GUIDE.md` for detailed docs
2. Review `PlayerProfileData.cs` for working example
3. Review `HighScoreData.cs` for simple example
4. Use Unity Console for save/load logs (Editor only)

---

**Remember:** Load once, cache it, save when needed! 💾

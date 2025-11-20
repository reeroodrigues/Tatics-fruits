# Architecture Migration Guide

## ⚠️ DEPRECATED CLASSES

The following classes have been **DEPRECATED** and marked with `[Obsolete]` attributes. They should **NOT** be used in new code.

### Deprecated Files (Old Architecture)

| Old Class | Status | Replacement |
|-----------|--------|-------------|
| `GameController_DEPRECATED` | ❌ Obsolete | `New_GameplayCore.Controllers.GameController` |
| `ScoreManager_DEPRECATED` | ❌ Obsolete | `New_GameplayCore.Services.ScoreService` |
| `Timer_DEPRECATED` | ❌ Obsolete | `New_GameplayCore.GameState.TimeManager` |
| `PreRoundPanelController_DEPRECATED` | ❌ Obsolete | `New_GameplayCore.Services.PreRoundPresenter` + `PreRoundView` |
| `LevelCompletedPanel` | ❌ Obsolete | `VictoryView` + `DefeatView` |

---

## ✅ NEW ARCHITECTURE (Use This!)

### Core Components

The new architecture follows **MVC pattern** with proper **separation of concerns**:

```
New_GameplayCore/
├── Controllers/           # Game flow coordination
│   └── GameController.cs
├── Services/              # Business logic & state
│   ├── ScoreService.cs
│   ├── HandService.cs
│   ├── DeckService.cs
│   ├── RuleEngine.cs
│   └── SwapService.cs
├── GameState/             # State management
│   ├── TimeManager.cs
│   ├── GameStateMachine.cs
│   └── ComboTracker.cs
├── Views/                 # UI presentation
│   ├── GameControllerInitializer.cs  ⭐ MAIN ENTRY POINT
│   ├── HUDView.cs
│   ├── HandView.cs
│   ├── VictoryView.cs
│   └── DefeatView.cs
└── Models/                # Data structures
```

### How to Use the New Architecture

#### 1. **Scene Setup**

In your gameplay scene, you need:

1. A GameObject with `GameControllerInitializer` component
2. Assign all required references in the Inspector:
   - `levelConfig` (ScriptableObject)
   - `deckConfig` (ScriptableObject)
   - `hudView`, `handView`, `preRoundView`
   - `victoryPrefab`, `defeatPrefab`
   - etc.

#### 2. **Accessing Game Systems**

Instead of `FindObjectOfType<ScoreManager>()`, use dependency injection:

```csharp
// In your MonoBehaviour
[SerializeField] private GameControllerInitializer bootstrap;

private void Start()
{
    // Access services through the initializer
    var scoreService = bootstrap.Score;
    var handService = bootstrap.Hand;
    var controller = bootstrap.Controller;
    
    // Subscribe to events
    scoreService.OnScoreChanged += HandleScoreChanged;
}

private void HandleScoreChanged(int total, int delta)
{
    Debug.Log($"Score: {total} (+{delta})");
}
```

#### 3. **Creating Views**

Views should be **passive** and only handle presentation:

```csharp
public class MyCustomView : MonoBehaviour
{
    [SerializeField] private GameControllerInitializer bootstrap;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    private void Start()
    {
        bootstrap.Score.OnScoreChanged += UpdateScoreUI;
    }
    
    private void UpdateScoreUI(int total, int delta)
    {
        scoreText.text = total.ToString();
    }
    
    private void OnDestroy()
    {
        if (bootstrap?.Score != null)
            bootstrap.Score.OnScoreChanged -= UpdateScoreUI;
    }
}
```

---

## 🔄 Migration Checklist

### For Existing Scenes

- [ ] Ensure `Gameplay Scene` uses `GameControllerInitializer` (already done ✅)
- [ ] Remove any old `GameController_DEPRECATED` components from scenes
- [ ] Remove any old `ScoreManager_DEPRECATED` components from scenes
- [ ] Remove any old `Timer_DEPRECATED` components from scenes
- [ ] Update prefab references to use new Victory/Defeat views

### For Code

- [ ] Replace `FindObjectOfType<GameController>()` with proper references
- [ ] Replace `FindObjectOfType<ScoreManager>()` with `bootstrap.Score`
- [ ] Replace `FindObjectOfType<Timer>()` with `bootstrap` time access
- [ ] Update event subscriptions to use new services
- [ ] Remove `using` statements for deprecated classes

### Testing

- [ ] Play through a complete level
- [ ] Verify victory conditions work
- [ ] Verify defeat conditions work
- [ ] Check that score tracking works
- [ ] Verify time management works
- [ ] Test level progression

---

## 📚 Key Architectural Benefits

### Old Architecture Issues ❌
- Tight coupling with `FindObjectOfType`
- Static state in `GameSession`
- Mixed responsibilities (UI + Logic)
- Hard to test
- Singletons everywhere

### New Architecture Benefits ✅
- **Dependency Injection**: Services are injected, not found
- **Separation of Concerns**: Views, Services, Controllers
- **Testable**: Services can be unit tested
- **Event-Driven**: Loosely coupled through events
- **Scalable**: Easy to add new features
- **Clear Data Flow**: Models → Services → Views

---

## 🎯 Next Steps

1. **Review this guide** and understand the new architecture
2. **Test the Gameplay Scene** to see the new system in action
3. **Update any custom code** that references old classes
4. **Consider removing** deprecated files after migration is complete

---

## ❓ Questions?

If you're unsure about how to migrate specific functionality, refer to:
- `GameControllerInitializer.cs` - Main initialization example
- `HUDView.cs` - Example of UI binding to services
- `VictoryView.cs` / `DefeatView.cs` - Example of end-game panels

**The old code will continue to work** but will show compiler warnings. 
**New features should ONLY use the new architecture.**

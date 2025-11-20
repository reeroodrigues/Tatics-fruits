# 🎯 Next Steps: Action Plan

## ✅ What Has Been Completed

### Phase 1: Deprecation (DONE ✅)

All old architecture files have been successfully deprecated:

```
✅ GameController.cs → GameController_DEPRECATED
✅ ScoreManager.cs → ScoreManager_DEPRECATED
✅ Timer.cs → Timer_DEPRECATED
✅ PreRoundPanelController.cs → PreRoundPanelController_DEPRECATED
✅ GameSession.cs → GameSession_DEPRECATED (static class)
✅ LevelCompletedPanel.cs → Marked as obsolete
✅ All internal cross-references updated
✅ Documentation created
```

### Compiler Warnings

Your Unity Console will now show helpful warnings like:

```
⚠️ warning CS0618: 'GameController_DEPRECATED' is obsolete: 
   'This class is deprecated. Use New_GameplayCore.Controllers.GameController instead.'
```

**This is intentional!** These warnings guide you to migrate remaining code.

---

## 🔄 Phase 2: Scene Cleanup (YOUR TASK)

### Step 1: Inspect Gameplay Scene

1. Open `Gameplay Scene.unity`
2. Look for these components in the Hierarchy:
   - `GameController_DEPRECATED`
   - `ScoreManager_DEPRECATED`
   - `Timer_DEPRECATED`
   - `PreRoundPanelController_DEPRECATED`
   - `LevelCompletedPanel`

### Step 2: Remove Old Components

**If you find any old components:**

1. Select the GameObject
2. In Inspector, click the ⚙️ icon on the old component
3. Choose "Remove Component"
4. Save the scene

**Expected state:**
- ✅ Should have: `GameControllerInitializer` component
- ❌ Should NOT have: Any `_DEPRECATED` components

### Step 3: Check Prefabs

Search for prefabs that might use old components:

1. In Project window, search: `t:Prefab`
2. Check these prefabs:
   - Pre-round panel prefab
   - Level completed panel prefab
   - HUD prefabs
   - Timer prefabs

**Action:** Remove or replace old components from prefabs

---

## 🔧 Phase 3: Code Migration (YOUR TASK)

### Find All Usages

Use Unity's search or grep to find remaining references:

```
Search in Project:
1. "GameController " (old type without _DEPRECATED)
2. "ScoreManager " (old type without _DEPRECATED)
3. "Timer " (old type without _DEPRECATED)
4. "GameSession."
5. "FindObjectOfType<"
```

### Common Migration Patterns

#### Pattern 1: FindObjectOfType → Dependency Injection

**Before:**
```csharp
public class MyScript : MonoBehaviour
{
    private ScoreManager scoreManager;
    
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        scoreManager.AddScore(10);
    }
}
```

**After:**
```csharp
public class MyScript : MonoBehaviour
{
    [SerializeField] private GameControllerInitializer bootstrap;
    
    void Start()
    {
        bootstrap.Score.OnScoreChanged += HandleScoreChanged;
        // Score is added through the service, not directly
    }
    
    private void HandleScoreChanged(int total, int delta)
    {
        Debug.Log($"New score: {total}");
    }
    
    void OnDestroy()
    {
        if (bootstrap?.Score != null)
            bootstrap.Score.OnScoreChanged -= HandleScoreChanged;
    }
}
```

#### Pattern 2: Static GameSession → Service

**Before:**
```csharp
void SaveLevel()
{
    GameSession._currentLevel = 5;
    GameSession._targetScore = 1000;
}
```

**After:**
```csharp
[SerializeField] private GameControllerInitializer bootstrap;

void SaveLevel()
{
    bootstrap.Progress.SetCurrentLevel(5);
    // Target score comes from LevelConfigSO
}
```

#### Pattern 3: Direct Timer Access → TimeManager

**Before:**
```csharp
Timer timer = FindObjectOfType<Timer>();
timer.AddTime(5f);
float remaining = timer.remainingTime;
```

**After:**
```csharp
[SerializeField] private GameControllerInitializer bootstrap;

void AddTimeBonus()
{
    // Access through the time service
    var timeManager = bootstrap.Controller as GameController;
    // Or subscribe to time events
    bootstrap.Time.OnTimeChanged += HandleTimeChanged;
}

void HandleTimeChanged(float remaining)
{
    Debug.Log($"Time remaining: {remaining}");
}
```

---

## 📝 Phase 4: Testing Checklist

After migration, test these scenarios:

### Gameplay Flow
- [ ] Start a new level
- [ ] Pre-round panel displays correctly
- [ ] Timer starts when gameplay begins
- [ ] Score updates when making pairs
- [ ] Combos work correctly
- [ ] Time bonuses apply
- [ ] Victory condition triggers
- [ ] Victory screen displays correctly
- [ ] Defeat condition triggers
- [ ] Defeat screen displays correctly

### Level Progression
- [ ] Can advance to next level
- [ ] Level progression is saved
- [ ] Can retry a failed level
- [ ] Can return to main menu
- [ ] Progress persists between sessions

### Edge Cases
- [ ] Pause/resume works
- [ ] Scene reload works
- [ ] Multiple plays in succession
- [ ] App backgrounding/foregrounding
- [ ] No null reference exceptions

---

## 🗑️ Phase 5: Final Cleanup (AFTER TESTING)

### When All Tests Pass

1. **Verify no warnings** in Console about deprecated classes
2. **Make a backup** of your project
3. **Delete deprecated files:**
   ```
   /Assets/Scripts/GameController.cs
   /Assets/Scripts/ScoreManager.cs
   /Assets/Scripts/Timer.cs
   /Assets/Scripts/PreRoundPanelController.cs
   /Assets/Scripts/LevelCompletedPanel.cs
   /Assets/Scripts/GameSession.cs (if fully replaced)
   ```

4. **Update documentation:**
   - Remove migration guides
   - Keep architecture overview
   - Update README if you have one

---

## 🎓 Training Your Team

### Share These Documents

1. **Architecture Overview** 
   `/Assets/Scripts/New GameplayCore/ARCHITECTURE_OVERVIEW.md`
   - How the system works
   - Best practices

2. **Migration Guide**
   `/Assets/Scripts/ARCHITECTURE_MIGRATION_GUIDE.md`
   - How to use new architecture
   - Migration patterns

### Code Review Checklist

When reviewing new code, check:
- [ ] No `FindObjectOfType<>` for services
- [ ] No static state usage
- [ ] Proper dependency injection
- [ ] Event subscriptions have unsubscriptions
- [ ] Views are passive (no game logic)
- [ ] Services are accessed through `GameControllerInitializer`

---

## 📊 Progress Tracking

### Current Status

```
[■■■■■■■■░░] 80% Complete

✅ Deprecated old classes
✅ Added compiler warnings
✅ Created documentation
✅ Updated internal references
⏳ Scene cleanup (YOUR TASK)
⏳ Code migration (YOUR TASK)
⏳ Testing (YOUR TASK)
⏳ Final cleanup (AFTER TESTING)
```

### Estimated Timeline

- **Scene Cleanup**: 30 minutes
- **Code Migration**: 2-4 hours (depends on custom code)
- **Testing**: 1-2 hours
- **Final Cleanup**: 15 minutes

**Total**: ~4-7 hours

---

## 🆘 Troubleshooting

### Issue: "GameControllerInitializer not found in scene"

**Solution:**
1. Create empty GameObject in scene
2. Add `GameControllerInitializer` component
3. Assign all required references:
   - Level Config (ScriptableObject)
   - Deck Config (ScriptableObject)
   - View references (HUD, Hand, PreRound, etc.)

### Issue: "Null reference when accessing bootstrap.Score"

**Solution:**
1. Ensure `GameControllerInitializer` is in the scene
2. Check `bootstrap` field is assigned in Inspector
3. Wait for `bootstrap.IsReady` before accessing services:
   ```csharp
   IEnumerator Start()
   {
       yield return new WaitUntil(() => bootstrap.IsReady);
       bootstrap.Score.OnScoreChanged += HandleScoreChanged;
   }
   ```

### Issue: "Events not firing"

**Solution:**
1. Check you've subscribed to events in `Start()` or after `IsReady`
2. Verify you're not unsubscribing too early
3. Check the service is actually being used (not old deprecated code)

### Issue: "Old and new systems running simultaneously"

**Solution:**
1. Remove ALL `_DEPRECATED` components from scenes
2. Remove ALL prefab instances with old components
3. Ensure only `GameControllerInitializer` exists

---

## 📞 Support

If you encounter issues:

1. **Check Console** for error messages and warnings
2. **Review Documentation** in `/Assets/Scripts/`
3. **Study Examples** in `New GameplayCore/Views/`
4. **Test Incrementally** - migrate one system at a time

---

## 🎉 Success!

When migration is complete, you'll have:

- ✅ Clean, maintainable architecture
- ✅ Testable code
- ✅ Proper separation of concerns
- ✅ Scalable system
- ✅ No compiler warnings
- ✅ Professional-grade codebase

**Keep pushing forward! You've got this!** 💪

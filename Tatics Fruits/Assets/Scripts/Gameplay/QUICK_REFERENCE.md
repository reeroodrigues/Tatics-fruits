# Quick Reference Card

## 🚀 Bootstrap Pattern

### Setup (Once per scene)

```csharp
// Add this component to a GameObject in your scene
[SerializeField] private GameControllerInitializer bootstrap;
```

---

## 📦 Accessing Services

### Score Service

```csharp
// Get current score
int currentScore = bootstrap.Score.Total;

// Subscribe to score changes
bootstrap.Score.OnScoreChanged += (total, delta) => 
{
    Debug.Log($"Score: {total} (+{delta})");
};
```

### Hand Service

```csharp
// Get cards in hand
IReadOnlyList<CardInstance> cards = bootstrap.Hand.Cards;

// Subscribe to hand changes
bootstrap.Hand.OnCardAdded += (card) => 
{
    Debug.Log($"Card added: {card.Value}");
};

bootstrap.Hand.OnCardRemoved += (card) => 
{
    Debug.Log($"Card removed: {card.Value}");
};
```

### Deck Service

```csharp
// Get remaining cards
int remaining = bootstrap.Deck.RemainingCount;

// Subscribe to deck changes
bootstrap.Deck.OnDeckChanged += () => 
{
    Debug.Log($"Deck updated: {bootstrap.Deck.RemainingCount} cards left");
};
```

### Controller

```csharp
// Subscribe to level end
bootstrap.Controller.OnLevelEnded += (cause) => 
{
    if (cause == EndCause.Victory)
        Debug.Log("You won!");
    else
        Debug.Log("You lost!");
};

// Select a card
bootstrap.Controller.OnCardSelected(card);

// Draw a card
bool success = bootstrap.Controller.TryDrawOne();
```

### Progress Service

```csharp
// Get current level
int level = bootstrap.Progress.CurrentLevel;

// Level progression
bootstrap.Progress.SetCurrentLevel(5);
```

### Profile Service

```csharp
// Access player profile
var profile = bootstrap.Profile;
```

---

## 🎨 Creating a View

### Basic View Template

```csharp
using UnityEngine;
using TMPro;
using New_GameplayCore.Views;

public class MyCustomView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameControllerInitializer bootstrap;
    [SerializeField] private TextMeshProUGUI displayText;
    
    private void Start()
    {
        if (bootstrap == null)
        {
            Debug.LogError("Bootstrap not assigned!");
            return;
        }
        
        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        bootstrap.Score.OnScoreChanged += UpdateDisplay;
    }
    
    private void UpdateDisplay(int total, int delta)
    {
        displayText.text = $"Score: {total}";
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void UnsubscribeFromEvents()
    {
        if (bootstrap?.Score != null)
            bootstrap.Score.OnScoreChanged -= UpdateDisplay;
    }
}
```

### Async View Template (Wait for Ready)

```csharp
using System.Collections;
using UnityEngine;
using New_GameplayCore.Views;

public class MyAsyncView : MonoBehaviour
{
    [SerializeField] private GameControllerInitializer bootstrap;
    
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => bootstrap.IsReady);
        
        Initialize();
    }
    
    private void Initialize()
    {
        bootstrap.Score.OnScoreChanged += HandleScoreChanged;
    }
    
    private void HandleScoreChanged(int total, int delta)
    {
        Debug.Log($"Score changed: {total}");
    }
    
    private void OnDestroy()
    {
        if (bootstrap?.Score != null)
            bootstrap.Score.OnScoreChanged -= HandleScoreChanged;
    }
}
```

---

## 🎯 Common Events

### Score Events

```csharp
bootstrap.Score.OnScoreChanged += (total, delta) => { };
bootstrap.Score.OnComboChanged += (combo) => { };
```

### Hand Events

```csharp
bootstrap.Hand.OnCardAdded += (card) => { };
bootstrap.Hand.OnCardRemoved += (card) => { };
```

### Deck Events

```csharp
bootstrap.Deck.OnDeckChanged += () => { };
```

### Controller Events

```csharp
bootstrap.Controller.OnLevelEnded += (cause) => { };
bootstrap.Controller.OnEnterPreRound += () => { };
bootstrap.Controller.OnExitPreRound += () => { };
```

### Time Events

```csharp
// Implement ITimeObserver for time events
```

---

## ⚠️ Common Mistakes

### ❌ DON'T DO THIS

```csharp
// Don't use FindObjectOfType
var score = FindObjectOfType<ScoreService>(); // ❌

// Don't access static state
GameSession._currentLevel = 5; // ❌

// Don't forget to unsubscribe
bootstrap.Score.OnScoreChanged += Update; // ❌ (no OnDestroy)

// Don't access before ready
void Start() 
{
    bootstrap.Score.Total; // ❌ Might be null!
}
```

### ✅ DO THIS

```csharp
// Use dependency injection
[SerializeField] private GameControllerInitializer bootstrap; // ✅

// Use services
bootstrap.Progress.SetCurrentLevel(5); // ✅

// Always unsubscribe
void Start() { bootstrap.Score.OnScoreChanged += Update; }
void OnDestroy() { bootstrap.Score.OnScoreChanged -= Update; } // ✅

// Wait for ready or check IsReady
IEnumerator Start() 
{
    yield return new WaitUntil(() => bootstrap.IsReady); // ✅
    bootstrap.Score.OnScoreChanged += Update;
}
```

---

## 🔍 Debugging Tips

### Check Bootstrap State

```csharp
void Start()
{
    Debug.Log($"Bootstrap ready: {bootstrap.IsReady}");
    Debug.Log($"Controller: {bootstrap.Controller != null}");
    Debug.Log($"Score: {bootstrap.Score != null}");
}
```

### Log Events

```csharp
void Start()
{
    bootstrap.Score.OnScoreChanged += (t, d) => 
        Debug.Log($"[SCORE] Total: {t}, Delta: {d}");
        
    bootstrap.Hand.OnCardAdded += (c) => 
        Debug.Log($"[HAND] Added: {c.Value}");
        
    bootstrap.Controller.OnLevelEnded += (cause) => 
        Debug.Log($"[LEVEL] Ended: {cause}");
}
```

### Verify Subscriptions

```csharp
void OnDestroy()
{
    Debug.Log("Unsubscribing from events...");
    
    if (bootstrap?.Score != null)
    {
        bootstrap.Score.OnScoreChanged -= HandleScore;
        Debug.Log("Unsubscribed from Score");
    }
}
```

---

## 📚 Further Reading

- **Architecture Overview**: `/Assets/Scripts/New GameplayCore/ARCHITECTURE_OVERVIEW.md`
- **Migration Guide**: `/Assets/Scripts/ARCHITECTURE_MIGRATION_GUIDE.md`
- **Examples**: Check `/Assets/Scripts/New GameplayCore/Views/` folder

---

## 💡 Pro Tips

1. **Always check `IsReady`** before accessing services
2. **Always unsubscribe** in `OnDestroy()`
3. **Keep views passive** - no game logic, only presentation
4. **Use events** for communication between systems
5. **Serialize references** in Inspector instead of finding at runtime
6. **Test incrementally** - one feature at a time

---

**Happy coding!** 🚀

# New GameplayCore Architecture Overview

## 🎯 Architecture Pattern: MVC + Service Layer

This architecture separates concerns into clear layers:

```
┌─────────────────────────────────────────────────────┐
│                    VIEWS (UI)                       │
│  HUDView, HandView, VictoryView, DefeatView, etc.  │
└─────────────────┬───────────────────────────────────┘
                  │ Events & Binding
┌─────────────────▼───────────────────────────────────┐
│                 CONTROLLERS                         │
│      GameController, GameControllerInitializer      │
└─────────────────┬───────────────────────────────────┘
                  │ Orchestration
┌─────────────────▼───────────────────────────────────┐
│                  SERVICES                           │
│  ScoreService, HandService, DeckService, etc.       │
└─────────────────┬───────────────────────────────────┘
                  │ State Management
┌─────────────────▼───────────────────────────────────┐
│                   MODELS                            │
│    CardInstance, LevelConfigSO, PreRoundModel       │
└─────────────────────────────────────────────────────┘
```

---

## 📁 Directory Structure

### `/Controllers`
**Purpose**: Orchestrate game flow and coordinate between services

- `GameController.cs` - Main game coordinator, implements `IGameController`
  - Handles level start/end
  - Coordinates services
  - Manages game state transitions
  - Emits high-level events (`OnLevelEnded`)

### `/Services`
**Purpose**: Business logic and state management

- `ScoreService.cs` - Score calculation and tracking
- `HandService.cs` - Player's hand management
- `DeckService.cs` - Deck building and card drawing
- `RuleEngine.cs` - Game rules and card matching logic
- `SwapService.cs` - Card swap power-up logic
- `PreRoundPresenter.cs` - Pre-round state presenter
- `VictoryPresenter.cs` - Victory state presenter
- `DefeatPresenter.cs` - Defeat state presenter
- `LevelProgressService.cs` - Level progression tracking
- `PlayerProfileService.cs` - Player data persistence

### `/GameState`
**Purpose**: State machines and state-specific logic

- `GameStateMachine.cs` - FSM for game states (Boot → PreRound → Playing → Results)
- `TimeManager.cs` - Time tracking and time-based events
- `ComboTracker.cs` - Combo system logic
- `DeckService.cs` - Deck state management

### `/Views`
**Purpose**: UI presentation and user input handling

- `GameControllerInitializer.cs` ⭐ **Main Entry Point**
  - Bootstraps entire game system
  - Creates and wires services
  - Initializes views
  
- `HUDView.cs` - In-game HUD display
- `HandView.cs` - Player hand visualization
- `PreRoundView.cs` - Pre-round panel
- `VictoryView.cs` - Victory screen
- `DefeatView.cs` - Defeat screen
- `CardView.cs` - Individual card display
- `DrawButtonView.cs` - Draw card button
- `DeckCounterView.cs` - Deck count display

### `/Models`
**Purpose**: Data structures and DTOs

- `PreRoundModel` - Pre-round data
- `VictoryModel` - Victory screen data
- `DefeatModel` - Defeat screen data
- `LevelProgressData` - Level progression data
- `PlayerProfileData` - Player profile data

### `/Utils`
**Purpose**: Helper utilities

- `CardVFXExtensions.cs` - Card visual effect helpers

---

## 🔄 Data Flow Example: Score Update

```
1. User Action
   └─> Card matched in HandView

2. View calls Controller
   └─> HandView → GameController.OnCardSelected()

3. Controller delegates to Service
   └─> GameController → RuleEngine.TryMakePair()

4. Service updates state
   └─> RuleEngine → ScoreService.AddPairScore()

5. Service emits event
   └─> ScoreService.OnScoreChanged(total, delta)

6. Views listen and update
   └─> HUDView receives event → Updates score text
```

---

## 🎮 Initialization Flow

```csharp
// GameControllerInitializer.Awake()
1. Create all services
   _time = new TimeManager()
   _score = new ScoreService()
   _deck = new DeckService()
   _hand = new HandService()
   _rule = new RuleEngine()
   
2. Wire services together
   _controller = new GameController(
       fsm, time, deck, hand, rule, swap, config, score
   )
   
3. Subscribe to events
   _controller.OnLevelEnded += HandleLevelEnded
   
4. Initialize views
   hudView.Initialize(_time, _score, _swap)
   handView.Initialize(_hand, _controller)
   
5. Start gameplay
   _controller.StartLevel(levelConfig, deckConfig)
```

---

## 🎯 Key Interfaces

### `IGameController`
Main game coordinator interface
```csharp
public interface IGameController
{
    void StartLevel(LevelConfigSO cfg, DeckConfigSo deckCfg);
    void UpdateTick(float deltaTime);
    void BeginPlayFromPreRound(PreRoundModel model);
    void OnCardSelected(CardInstance card);
    bool TryDrawOne();
    event Action<EndCause> OnLevelEnded;
}
```

### `IScoreService`
Score management interface
```csharp
public interface IScoreService
{
    int Total { get; }
    int CurrentCombo { get; }
    event Action<int, int> OnScoreChanged;
    void AddPairScore(CardInstance a, CardInstance b, float multiplier, out int points);
}
```

### `IHandService`
Hand management interface
```csharp
public interface IHandService
{
    IReadOnlyList<CardInstance> Cards { get; }
    event Action<CardInstance> OnCardAdded;
    event Action<CardInstance> OnCardRemoved;
    bool TryAdd(CardInstance card);
    bool TryRemove(CardInstance card);
}
```

---

## ✅ Best Practices

### DO ✅
- Use dependency injection through `GameControllerInitializer`
- Subscribe to service events in Views
- Keep Views passive (presentation only)
- Use ScriptableObjects for configuration
- Unsubscribe from events in `OnDestroy()`
- Use interfaces for services

### DON'T ❌
- Use `FindObjectOfType<>` for services
- Put game logic in Views
- Use static state (like old `GameSession`)
- Create tight coupling between systems
- Forget to unsubscribe from events

---

## 🧪 Testing

Services are designed to be testable:

```csharp
[Test]
public void ScoreService_AddPairScore_CalculatesCorrectly()
{
    // Arrange
    var config = ScriptableObject.CreateInstance<LevelConfigSO>();
    config.scorePerPairBase = 10;
    var scoreService = new ScoreService(config);
    
    var cardA = new CardInstance { Value = 5 };
    var cardB = new CardInstance { Value = 5 };
    
    // Act
    scoreService.AddPairScore(cardA, cardB, 1.0f, out int points);
    
    // Assert
    Assert.AreEqual(20, points); // 10 + 5 + 5
    Assert.AreEqual(20, scoreService.Total);
}
```

---

## 🔌 Extending the System

### Adding a New Power-Up

1. **Create Service** (`/Services/NewPowerUpService.cs`)
2. **Define Interface** (`INewPowerUpService`)
3. **Initialize in GameControllerInitializer**
4. **Create View** (`/Views/NewPowerUpView.cs`)
5. **Bind View to Service** in `Initialize()`

### Adding a New Game State

1. **Add state to enum** in `GameState.cs`
2. **Update GameStateMachine** with new transitions
3. **Add state handling** in `GameController`
4. **Create View** for the new state
5. **Handle state change** in `GameControllerInitializer`

---

## 📖 Further Reading

- Study `GameControllerInitializer.cs` for initialization patterns
- Review `RuleEngine.cs` for service-to-service communication
- Check `VictoryPresenter.cs` for Presenter pattern example
- Look at `HUDView.cs` for View binding example

---

**Questions?** Check `/Assets/Scripts/ARCHITECTURE_MIGRATION_GUIDE.md`

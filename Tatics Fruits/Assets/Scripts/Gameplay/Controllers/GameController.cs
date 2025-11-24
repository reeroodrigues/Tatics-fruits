using System;
using System.Collections.Generic;
using DefaultNamespace.New_GameplayCore;
using New_GameplayCore.GameState;
using New_GameplayCore.Services;

namespace New_GameplayCore.Controllers
{
    public class GameController : IGameController
    {
        private readonly IGameStateMachine _fsm;
        private readonly ITimeManager _time;
        private readonly IDeckService _deck;
        private readonly IHandService _hand;
        private readonly IRuleEngine _rule;
        private readonly ISwapService _swap;
        private LevelConfigSO _cfg;
        private readonly IScoreService _score;

        private CardInstance? _selectedCard = null;

        public event Action<EndCause> OnLevelEnded;

        public GameController(IGameStateMachine fsm, ITimeManager time, IDeckService deck, 
            IHandService hand, IRuleEngine rule, ISwapService swap, LevelConfigSO cfg, ScoreService score)
        {
            _fsm = fsm; _time = time; _deck = deck; _hand = hand; _rule = rule; _swap = swap;
            _cfg = cfg; _score =  score ?? throw new ArgumentNullException(nameof(score));

            _time.OnTimeChanged += t =>
            {
                if (t <= 0 && _fsm.Current == DefaultNamespace.New_GameplayCore.GameState.Playing)
                {
                    _fsm.SetState(DefaultNamespace.New_GameplayCore.GameState.Results);
                    OnLevelEnded?.Invoke(EndCause.TimeUp);
                }
            };
            
            _score.OnScoreChanged += (total, delta) =>
            {
                if (_fsm.Current == DefaultNamespace.New_GameplayCore.GameState.Playing && total >= _cfg.targetScore)
                {
                    _fsm.SetState(DefaultNamespace.New_GameplayCore.GameState.Results);
                    OnLevelEnded?.Invoke(EndCause.TargetReached);
                }
            };
        }

        public bool IsPlaying { get; }
        public event Action OnEnterPreRound;
        public event Action OnExitPreRound;

        public void StartLevel(LevelConfigSO cfg, DeckConfigSo deckCfg)
        {
            _cfg = cfg;
            
            var rng = cfg.useFixedSeed ? new Random(cfg.fixedSeed) : new Random();
            _deck.Build(deckCfg, rng);
            
            _fsm.SetState(DefaultNamespace.New_GameplayCore.GameState.PreRound);
            OnEnterPreRound?.Invoke();
            
            var cards = new List<CardInstance>();
            _deck.DrawMany(cfg.handSize, cards);
            _hand.AddMany(cards);
        }

        public void UpdateTick(float deltaTime)
        {
            if (_fsm.Current != DefaultNamespace.New_GameplayCore.GameState.Playing) return;
            (_time as TimeManager)?.Tick(deltaTime);
        }

        public void OnCardSelected(CardInstance card)
        {
            
            if (_selectedCard == null)
            {
                _selectedCard = card;
                return;
            }

            if (_rule.TryMakePair(_selectedCard.Value, card, out var result))
            {
            }

            _selectedCard = null;
        }

        public void BeginPlayFromPreRound(PreRoundModel model)
        {
            var rng = model.useFixedSeed ? new Random(model.effectiveSeed) : new Random();

            _time.TryPay(_time.TimeLeftSeconds);
            _time.Add(_cfg.initialTimeSeconds);

            var buf = new List<CardInstance>();
            _deck.DrawMany(_cfg.handSize, buf);
            _hand.ClearTo(new List<CardInstance>());
            _hand.AddMany(buf);
            
            OnExitPreRound?.Invoke();
            
            _fsm.SetState(DefaultNamespace.New_GameplayCore.GameState.Playing);
        }

        public void BackToLevelSelect()
        {
            _fsm.SetState(DefaultNamespace.New_GameplayCore.GameState.Boot);
        }

        public bool TryDrawOne()
        {
            
            if(!(_hand as HandService).HasSpace)
                return false;

            if (!_deck.TryDraw(out var card))
            {
                if (_cfg.allowEmptyDeckRefill && _deck.TryRefillFromDiscard())
                {
                    if(!_deck.TryDraw(out card))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return _hand.TryAdd(card);
        }

        public void OnSwapAllRequested() { _swap.TrySwapAll(); }
        public void OnSwapRandomRequested() { _swap.TrySwapRandom(); }
    }
}
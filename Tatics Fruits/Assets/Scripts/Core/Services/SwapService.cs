using System.Collections.Generic;
using Core.ScriptableObjects;

namespace New_GameplayCore.Services
{
    public class SwapService : ISwapService
    {
        private readonly IHandService _hand;
        private readonly IDeckService _deck;
        private readonly ITimeManager _time;
        private readonly LevelConfigSO _cfg;

        public SwapService(IHandService hand, IDeckService deck, ITimeManager time, LevelConfigSO cfg)
        {
            _hand = hand;
            _deck = deck;
            _time = time;
            _cfg = cfg;
        }

        public bool TrySwapAll()
        {
            if (!_time.CanPay(_cfg.swapAllTimePenalty))
                return false;

            int currentCount = _hand.Cards.Count;
            if (currentCount == 0)
                return false;

            _time.TryPay(_cfg.swapAllTimePenalty);
            
            var toDiscard = new List<CardInstance>(_hand.Cards);
            _hand.RemoveWhere(_ => true, toDiscard);
            _deck.DiscardMany(toDiscard);
            
            var newCards = new List<CardInstance>(currentCount);
            DrawUpTo(currentCount, newCards);
            
            _hand.AddMany(newCards);
            
            return newCards.Count > 0;
        }
        
        private void DrawUpTo(int target, IList<CardInstance> buffer)
        {
            int remaining = target;
            
            int drawn = _deck.DrawMany(remaining, buffer);
            remaining -= drawn;
            
            if (remaining > 0 && _cfg.allowEmptyDeckRefill && _deck.TryRefillFromDiscard())
            {
                drawn = _deck.DrawMany(remaining, buffer);
                remaining -= drawn;
            }
        }
        
        public bool TrySwapRandom()
        {
            if (!_time.CanPay(_cfg.swapRandomTimePenalty))
                return false;

            _time.TryPay(_cfg.swapRandomTimePenalty);

            if (_hand.Cards.Count == 0) 
                return false;

            int idx = UnityEngine.Random.Range(0, _hand.Cards.Count);
            var toRemove = _hand.Cards[idx];
            
            _deck.Discard(toRemove);
            
            if (_deck.TryDraw(out var newCard))
            {
                if (_hand is HandService handService)
                {
                    handService.ReplaceAt(idx, newCard);
                }
                else
                {
                    _hand.TryRemove(toRemove);
                    _hand.TryAdd(newCard);
                }
            }
            else
            {
                _hand.TryRemove(toRemove);
            }

            return true;
        }
    }
}
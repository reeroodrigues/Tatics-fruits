using System;
using System.Collections.Generic;
using UnityEngine;

namespace New_GameplayCore.Services
{
    public class RuleEngine : IRuleEngine
    {
        private readonly IHandService _hand;
        private readonly IDeckService _deck;
        private readonly IScoreService _score;
        private readonly ITimeManager _time;
        private readonly IComboTracker _combo;
        private readonly LevelConfigSO _cfg;

        public event Action<PairResult> OnPairResolved;

        public RuleEngine(IHandService hand, IDeckService deck, IScoreService score, 
            ITimeManager time, IComboTracker combo, LevelConfigSO cfg)
        {
            _hand = hand;
            _deck = deck;
            _score = score;
            _time = time;
            _combo = combo;
            _cfg = cfg;
        }

        public bool IsValidPair(CardInstance a, CardInstance b)
            => a.Type.id == b.Type.id;

        public bool TryMakePair(CardInstance a, CardInstance b, out PairResult result)
        {
            result = default;
            if (!IsValidPair(a, b)) return false;

            _combo.RegisterPair();

            int comboIndex = Mathf.Clamp(_combo.CurrentCombo - 1, 0, _cfg.comboMultipliers.Length - 1);
            float multiplier = _cfg.comboMultipliers[comboIndex];

            _score.AddPairScore(a, b, multiplier, out int added);
            int bonus = _cfg.timeBonusOnPair + Mathf.FloorToInt((_combo.CurrentCombo - 1) * 1);
            _time.Add(bonus);

            _hand.TryRemove(a);
            _hand.TryRemove(b);

            result = new PairResult(a, b, added, _combo.CurrentCombo, bonus);
            OnPairResolved?.Invoke(result);
            return true;
        }
    }
}
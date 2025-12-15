using System;
using System.Collections.Generic;
using Core.Services;

namespace New_GameplayCore.Services
{
    public class HandService : IHandService
    {
        private readonly List<CardInstance> _hand = new();
        private readonly int _maxSize;
        
        public int MaxSize => _maxSize;
        public bool HasSpace => _hand.Count < _maxSize;
        public IReadOnlyList<CardInstance> Cards => _hand;
        public event Action<IReadOnlyList<CardInstance>> OnHandChanged;

        public HandService(int maxSize)
        {
            _maxSize = maxSize;
        }

        public bool TryAdd(CardInstance card)
        {
            if (_hand.Count >= _maxSize) return false;
            _hand.Add(card);
            OnHandChanged?.Invoke(_hand);
            return true;
        }

        public void AddMany(IReadOnlyList<CardInstance> cards)
        {
            foreach (var c in cards)
                TryAdd(c);
        }

        public bool TryRemove(CardInstance card)
        {
            bool removed = _hand.Remove(card);
            if (removed) OnHandChanged?.Invoke(_hand);
            return removed;
        }

        public int RemoveWhere(Predicate<CardInstance> predicate, IList<CardInstance> removedOut)
        {
            int removed = _hand.RemoveAll(c =>
            {
                bool cond = predicate(c);
                if (cond) removedOut.Add(c);
                return cond;
            });
            if (removed > 0) OnHandChanged?.Invoke(_hand);
            return removed;
        }

        public void ClearTo(IList<CardInstance> movedOut)
        {
            if (movedOut == null) throw new ArgumentNullException(nameof(movedOut));
            movedOut.Clear();
            for (int i = 0; i < _hand.Count; i++)
                movedOut.Add(_hand[i]);
            _hand.Clear();
            OnHandChanged?.Invoke(_hand);
        }
        
        public void ReplaceAt(int index, CardInstance newCard)
        {
            if (index < 0 || index >= _hand.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _hand[index] = newCard;
            OnHandChanged?.Invoke(_hand);
        }
    }
}
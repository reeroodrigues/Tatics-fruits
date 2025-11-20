using System.Collections.Generic;
using UnityEngine;

namespace New_GameplayCore.Views
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private Transform cardParent;
        [SerializeField] private CardView cardPrefab;

        private IHandService _hand;
        private IGameController _game;
        
        private readonly List<CardView> _spawned = new();

        public void Initialize(IHandService hand, IGameController game)
        {
            _hand = hand;
            _game = game;

            _hand.OnHandChanged += UpdateHand;
            UpdateHand(_hand.Cards);
        }

        private void OnDestroy()
        {
            if (_hand != null)
                _hand.OnHandChanged -= UpdateHand;
        }

        private void UpdateHand(IReadOnlyList<CardInstance> cards)
        {
            foreach (var cv in _spawned)
            {
                if (cv != null && cv.transform != null && cv.transform.parent == cardParent)
                {
                    Destroy(cv.gameObject);
                }
            }
            _spawned.Clear();
            
            foreach (var card in cards)
            {
                var cv = Instantiate(cardPrefab, cardParent);
                cv.Initialize(card, _game.OnCardSelected);
                _spawned.Add(cv);
            }
        }
    }
}
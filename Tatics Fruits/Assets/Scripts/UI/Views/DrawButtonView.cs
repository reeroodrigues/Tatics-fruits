using System;
using New_GameplayCore.Services;
using UnityEngine;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class DrawButtonView : MonoBehaviour
    {
        [SerializeField] private Button drawButton;
        [SerializeField] private GameControllerInitializer bootstrap;
        
        private IGameController _controller;
        private IHandService _hand;
        private IDeckService _deck;

        private void Start()
        {
            _controller = bootstrap.Controller;
            _hand = bootstrap.Hand;
            _deck = bootstrap.Deck;
            
            drawButton.onClick.AddListener(OnDraw);

            _hand.OnHandChanged += _ => RefreshInteractable();
            RefreshInteractable();
        }

        private void OnDestroy()
        {
            if (_hand != null)
                _hand.OnHandChanged -= _ => RefreshInteractable();
        }

        private void OnDraw()
        {
            if (_controller is New_GameplayCore.Controllers.GameController gc)
            {
                if (gc.TryDrawOne())
                    RefreshInteractable();
            }
        }

        private void RefreshInteractable()
        {
            var hasSpace = (_hand as HandService).HasSpace;
            var canDraw = _deck.DeckCount > 0 || bootstrap.LevelConfig.allowEmptyDeckRefill;
            drawButton.interactable = hasSpace && canDraw;
        }
    }
}
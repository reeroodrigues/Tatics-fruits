using System.Collections.Generic;
using Gameplay.Controllers;
using New_GameplayCore;
using UnityEngine;
using UnityEngine.UI;
using New_GameplayCore.Views;
using UI.Views;

public class AutoBuyCardsController : MonoBehaviour
{
    [Header("UI Toggle")]
    [SerializeField] private Toggle autoBuyToggle;

    [Header("References")]
    [SerializeField] private GameControllerInitializer bootstrap;

    private IHandService _hand;
    private IDeckService _deck;
    private GameController _gameController;

    [Header("Settings")]
    [SerializeField] private int minCardsInHand = 6;

    private void Start()
    {
        _hand = bootstrap.Hand;
        _deck = bootstrap.Deck;
        _gameController = bootstrap.Controller as GameController;

        if (_hand != null)
            _hand.OnHandChanged += OnHandChanged;

        autoBuyToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDestroy()
    {
        if (_hand != null)
            _hand.OnHandChanged -= OnHandChanged;

        autoBuyToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
            TryAutoBuy();
    }

    private void OnHandChanged(IReadOnlyList<CardInstance> cards)
    {
        if (!IsEnabled) return;

        TryAutoBuy();
    }

    private bool IsEnabled => autoBuyToggle != null && autoBuyToggle.isOn;

    private void TryAutoBuy()
    {
        if (_gameController == null || _hand == null || _deck == null) 
            return;

        int currentHand = _hand.Cards.Count;

        if (currentHand >= minCardsInHand) 
            return;

        int missing = minCardsInHand - currentHand;

        for (int i = 0; i < missing; i++)
        {
            _gameController.TryDrawOne();
        }
    }
}
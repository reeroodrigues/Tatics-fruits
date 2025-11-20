using TMPro;
using UnityEngine;
using New_GameplayCore;
using New_GameplayCore.Views;

public class DeckCounterView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameControllerInitializer bootstrap;

    private IDeckService _deck;
    private IHandService _hand;

    void Start()
    {
        _deck = bootstrap.Deck;
        _hand = bootstrap.Hand;
        
        _deck.OnDeckChanged += HandleDeckChanged;
        _hand.OnHandChanged += HandleHandChanged;
        
        RefreshLabel();
    }

    void OnDestroy()
    {
        if (_deck != null) _deck.OnDeckChanged -= HandleDeckChanged;
        if (_hand != null) _hand.OnHandChanged -= HandleHandChanged;
    }

    private void HandleDeckChanged(int deckCount, int discardCount)
    {
        RefreshLabel();
    }

    private void HandleHandChanged(System.Collections.Generic.IReadOnlyList<CardInstance> _)
    {
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (_deck == null || label == null) return;
        label.text = $"{_deck.DeckCount}/{_deck.TotalInitialCount}";
    }
}
using ArenaGlass.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaGlass.Services;

/// <summary>
/// Service responsible for tracking cards in the player's library.
/// </summary>
public class CardTrackingService
{
    private List<Card> _remainingCards = new List<Card>();
    private readonly object _lockObject = new object();

    public event EventHandler<LibraryChangedEventArgs>? LibraryChanged;

    /// <summary>
    /// Sets the starting deck configuration.
    /// </summary>
    public void SetStartingDeck(List<Card> deck)
    {
        lock (_lockObject)
        {
            _remainingCards = deck.Select(c => new Card(c.Name, c.Quantity)).ToList();
            Console.WriteLine($"Starting deck loaded: {deck.Count} unique cards, {GetTotalCardCount()} total cards");
            PrintLibraryStatus();
            NotifyLibraryChanged();
        }
    }

    /// <summary>
    /// Records that a card was drawn from the library.
    /// </summary>
    public void CardDrawn(string cardName)
    {
        lock (_lockObject)
        {
            var card = _remainingCards.FirstOrDefault(c => c.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase));
            if (card != null)
            {
                card.Quantity--;
                Console.WriteLine($"Card drawn: {cardName} (remaining: {card.Quantity})");
                
                if (card.Quantity <= 0)
                {
                    _remainingCards.Remove(card);
                    Console.WriteLine($"  {cardName} is now exhausted from library");
                }
                
                PrintLibraryStatus();
                NotifyLibraryChanged();
            }
            else
            {
                Console.WriteLine($"Warning: Attempted to draw unknown card: {cardName}");
            }
        }
    }

    /// <summary>
    /// Gets a copy of the current remaining cards.
    /// </summary>
    public List<Card> GetRemainingCards()
    {
        lock (_lockObject)
        {
            return _remainingCards.Select(c => new Card(c.Name, c.Quantity)).ToList();
        }
    }

    /// <summary>
    /// Gets the total number of cards remaining in the library.
    /// </summary>
    public int GetTotalCardCount()
    {
        lock (_lockObject)
        {
            return _remainingCards.Sum(c => c.Quantity);
        }
    }

    /// <summary>
    /// Clears the current deck state.
    /// </summary>
    public void ClearDeck()
    {
        lock (_lockObject)
        {
            _remainingCards.Clear();
            Console.WriteLine("Deck cleared");
            NotifyLibraryChanged();
        }
    }

    /// <summary>
    /// Adds sample cards for testing purposes.
    /// </summary>
    public void AddSampleCards()
    {
        var sampleDeck = new List<Card>
        {
            new Card("Lightning Bolt", 4),
            new Card("Island", 2),
            new Card("Mountain", 2),
            new Card("Forest", 1),
            new Card("Plains", 1),
            new Card("Swamp", 1),
            new Card("Counterspell", 3),
            new Card("Giant Growth", 2),
            new Card("Healing Salve", 1),
            new Card("Dark Ritual", 2),
        };
        
        Console.WriteLine("Loading sample deck for demonstration...");
        SetStartingDeck(sampleDeck);
    }

    private void PrintLibraryStatus()
    {
        var totalCards = GetTotalCardCount();
        Console.WriteLine($"\n=== Library Status ===");
        Console.WriteLine($"Total cards remaining: {totalCards}");
        Console.WriteLine($"Unique cards: {_remainingCards.Count}");
        
        if (_remainingCards.Count > 0)
        {
            Console.WriteLine("Remaining cards:");
            foreach (var card in _remainingCards.OrderBy(c => c.Name))
            {
                Console.WriteLine($"  {card.Quantity}x {card.Name}");
            }
        }
        Console.WriteLine("=====================\n");
    }

    private void NotifyLibraryChanged()
    {
        var eventArgs = new LibraryChangedEventArgs(GetRemainingCards());
        LibraryChanged?.Invoke(this, eventArgs);
    }
}
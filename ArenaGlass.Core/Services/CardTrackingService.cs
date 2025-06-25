using ArenaGlass.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArenaGlass.Core.Services;

public class CardTrackingService : ICardTrackingService
{
    private readonly ILogger<CardTrackingService> _logger;
    private GameState _currentGameState;

    public GameState CurrentGameState 
    { 
        get => _currentGameState;
        private set
        {
            _currentGameState = value;
            _currentGameState.LastUpdated = DateTime.Now;
            GameStateUpdated?.Invoke(this, _currentGameState);
        }
    }

    public event EventHandler<GameState>? GameStateUpdated;

    public CardTrackingService(ILogger<CardTrackingService> logger)
    {
        _logger = logger;
        _currentGameState = new GameState();
        InitializeSampleLibrary();
    }

    public void ProcessGameEvent(GameEvent gameEvent)
    {
        try
        {
            _logger.LogDebug("Processing game event: {EventType}", gameEvent.EventType);

            switch (gameEvent.EventType)
            {
                case "DrawEvent":
                    ProcessDrawEvent(gameEvent);
                    break;
                case "LogEvent":
                    ProcessLogEvent(gameEvent);
                    break;
                default:
                    _logger.LogDebug("Unknown event type: {EventType}", gameEvent.EventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing game event: {EventType}", gameEvent.EventType);
        }
    }

    public void Reset()
    {
        _logger.LogInformation("Resetting card tracking state");
        CurrentGameState = new GameState
        {
            IsGameActive = false
        };
        InitializeSampleLibrary();
    }

    private void ProcessDrawEvent(GameEvent gameEvent)
    {
        // Simulate drawing a card by reducing the count of a random card
        var availableCards = CurrentGameState.Library.Where(c => c.Count > 0).ToList();
        if (availableCards.Any())
        {
            var random = new Random();
            var cardToReduce = availableCards[random.Next(availableCards.Count)];
            cardToReduce.Count--;
            
            if (cardToReduce.Count <= 0)
            {
                CurrentGameState.Library.Remove(cardToReduce);
            }

            CurrentGameState.TotalCardsInLibrary = CurrentGameState.Library.Sum(c => c.Count);
            _logger.LogInformation("Card drawn: {CardName}, Remaining: {Count}", 
                cardToReduce.Name, CurrentGameState.TotalCardsInLibrary);
            
            // Trigger update
            CurrentGameState = new GameState
            {
                Library = CurrentGameState.Library,
                TotalCardsInLibrary = CurrentGameState.TotalCardsInLibrary,
                IsGameActive = CurrentGameState.IsGameActive
            };
        }
    }

    private void ProcessLogEvent(GameEvent gameEvent)
    {
        // Process general log events
        if (gameEvent.Data.Contains("Game Start"))
        {
            CurrentGameState.IsGameActive = true;
            _logger.LogInformation("Game started");
        }
        else if (gameEvent.Data.Contains("Game End"))
        {
            CurrentGameState.IsGameActive = false;
            _logger.LogInformation("Game ended");
        }
    }

    private void InitializeSampleLibrary()
    {
        // Initialize with a sample library for demonstration
        var sampleCards = new List<Card>
        {
            new Card { Name = "Lightning Bolt", Count = 4, ManaCost = "R", Type = "Instant" },
            new Card { Name = "Counterspell", Count = 4, ManaCost = "1U", Type = "Instant" },
            new Card { Name = "Giant Growth", Count = 4, ManaCost = "G", Type = "Instant" },
            new Card { Name = "Dark Ritual", Count = 4, ManaCost = "B", Type = "Instant" },
            new Card { Name = "Healing Salve", Count = 4, ManaCost = "W", Type = "Instant" },
            new Card { Name = "Island", Count = 8, ManaCost = "", Type = "Basic Land" },
            new Card { Name = "Mountain", Count = 8, ManaCost = "", Type = "Basic Land" },
            new Card { Name = "Forest", Count = 8, ManaCost = "", Type = "Basic Land" },
            new Card { Name = "Swamp", Count = 8, ManaCost = "", Type = "Basic Land" },
            new Card { Name = "Plains", Count = 8, ManaCost = "", Type = "Basic Land" }
        };

        CurrentGameState.Library = sampleCards;
        CurrentGameState.TotalCardsInLibrary = sampleCards.Sum(c => c.Count);
        CurrentGameState.IsGameActive = false;
        
        _logger.LogInformation("Initialized sample library with {Count} cards", 
            CurrentGameState.TotalCardsInLibrary);
    }
}
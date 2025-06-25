using ArenaGlass.Core.Models;
using ArenaGlass.Core.Services;
using Microsoft.Extensions.Logging;

namespace ArenaGlass.UI;

public class ArenaGlassApplication
{
    private readonly IGameMonitorService _gameMonitor;
    private readonly ILogParserService _logParser;
    private readonly ICardTrackingService _cardTracker;
    private readonly ILogger<ArenaGlassApplication> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ArenaGlassApplication(
        IGameMonitorService gameMonitor,
        ILogParserService logParser,
        ICardTrackingService cardTracker,
        ILogger<ArenaGlassApplication> logger)
    {
        _gameMonitor = gameMonitor;
        _logParser = logParser;
        _cardTracker = cardTracker;
        _logger = logger;

        // Subscribe to events
        _gameMonitor.GameStateChanged += OnGameStateChanged;
        _logParser.GameEventParsed += OnGameEventParsed;
        _cardTracker.GameStateUpdated += OnGameStateUpdated;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting ArenaGlass application");
        
        Console.WriteLine("Starting services...");
        
        // Start services
        await _gameMonitor.StartMonitoringAsync(_cancellationTokenSource.Token);
        await _logParser.StartParsingAsync(_cancellationTokenSource.Token);
        
        Console.WriteLine("Services started. Monitoring for MTG Arena...");
        Console.WriteLine();
        
        DisplayCurrentState();
        
        // Simulate overlay behavior with console updates
        Console.WriteLine("Press 'd' to simulate drawing a card, 'r' to reset, or 'q' to quit:");
        
        var key = Console.ReadKey();
        while (key.KeyChar != 'q')
        {
            Console.WriteLine();
            
            switch (key.KeyChar)
            {
                case 'd':
                    SimulateDrawCard();
                    break;
                case 'r':
                    _cardTracker.Reset();
                    break;
            }
            
            Console.WriteLine("Press 'd' to simulate drawing a card, 'r' to reset, or 'q' to quit:");
            key = Console.ReadKey();
        }
        
        _logger.LogInformation("Shutting down ArenaGlass application");
        _cancellationTokenSource.Cancel();
        
        _gameMonitor.StopMonitoring();
        _logParser.StopParsing();
    }

    private void OnGameStateChanged(object? sender, bool isRunning)
    {
        Console.WriteLine($"\n[Game Monitor] MTG Arena is {(isRunning ? "RUNNING" : "NOT RUNNING")}");
        if (isRunning)
        {
            Console.WriteLine("[Overlay] Would position overlay relative to game window");
        }
        else
        {
            Console.WriteLine("[Overlay] Would hide overlay");
        }
    }

    private void OnGameEventParsed(object? sender, GameEvent gameEvent)
    {
        Console.WriteLine($"\n[Log Parser] Event: {gameEvent.EventType} at {gameEvent.Timestamp:HH:mm:ss}");
        _cardTracker.ProcessGameEvent(gameEvent);
    }

    private void OnGameStateUpdated(object? sender, GameState gameState)
    {
        Console.Clear();
        Console.WriteLine("ArenaGlass - Live Deck Tracking Overlay");
        Console.WriteLine("=======================================");
        Console.WriteLine();
        
        DisplayCurrentState(gameState);
        
        Console.WriteLine("Press 'd' to simulate drawing a card, 'r' to reset, or 'q' to quit:");
    }

    private void DisplayCurrentState(GameState? gameState = null)
    {
        gameState ??= _cardTracker.CurrentGameState;
        
        Console.WriteLine($"Game Status: {(gameState.IsGameActive ? "ACTIVE" : "INACTIVE")}");
        Console.WriteLine($"Total Cards in Library: {gameState.TotalCardsInLibrary}");
        Console.WriteLine($"Last Updated: {gameState.LastUpdated:HH:mm:ss}");
        Console.WriteLine();
        
        if (gameState.Library.Any())
        {
            Console.WriteLine("Cards in Library:");
            Console.WriteLine("─────────────────");
            
            foreach (var card in gameState.Library.OrderBy(c => c.Name))
            {
                Console.WriteLine($"{card.Name,-20} | {card.Count,2}x | {card.ManaCost,-4} | {card.Type}");
            }
        }
        else
        {
            Console.WriteLine("No cards in library");
        }
        
        Console.WriteLine();
    }

    private void SimulateDrawCard()
    {
        var drawEvent = new GameEvent
        {
            Timestamp = DateTime.Now,
            EventType = "DrawEvent",
            Data = "Simulated card draw"
        };
        
        _cardTracker.ProcessGameEvent(drawEvent);
    }
}
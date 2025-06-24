using ArenaGlass.Models;
using ArenaGlass.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArenaGlass;

/// <summary>
/// ArenaGlass MVP - MTG Arena Deck Tracker Console Application
/// 
/// This is the core implementation that demonstrates the MVP functionality:
/// - Game state detection (MTG Arena process monitoring)
/// - Log file monitoring and parsing
/// - Real-time deck tracking
/// - Library card management
/// </summary>
class Program
{
    private static GameMonitorService? _gameMonitor;
    private static LogParserService? _logParser;
    private static CardTrackingService? _cardTracker;
    private static bool _isRunning = true;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("    ArenaGlass MVP - Console Demo");
        Console.WriteLine("  MTG Arena Deck Tracker v1.0");
        Console.WriteLine("=================================\n");

        // Initialize services
        _cardTracker = new CardTrackingService();
        _logParser = new LogParserService(_cardTracker);
        _gameMonitor = new GameMonitorService(_logParser);

        // Subscribe to events
        _gameMonitor.GameStateChanged += OnGameStateChanged;
        _cardTracker.LibraryChanged += OnLibraryChanged;

        Console.WriteLine("Available commands:");
        Console.WriteLine("  'start' - Start monitoring MTG Arena");
        Console.WriteLine("  'stop'  - Stop monitoring");
        Console.WriteLine("  'demo'  - Run a demo with sample cards");
        Console.WriteLine("  'draw <cardname>' - Simulate drawing a card");
        Console.WriteLine("  'clear' - Clear current deck");
        Console.WriteLine("  'status' - Show current status");
        Console.WriteLine("  'quit'  - Exit application");
        Console.WriteLine();

        // Start the command loop
        await RunCommandLoop();

        // Cleanup
        _gameMonitor?.StopMonitoring();
        _gameMonitor?.Dispose();

        Console.WriteLine("\nArenaGlass stopped. Goodbye!");
    }

    private static async Task RunCommandLoop()
    {
        while (_isRunning)
        {
            Console.Write("ArenaGlass> ");
            var input = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(input))
                continue;

            var parts = input.Split(' ', 2);
            var command = parts[0];
            var argument = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "start":
                    Console.WriteLine("Starting MTG Arena monitoring...");
                    _gameMonitor?.StartMonitoring();
                    break;

                case "stop":
                    Console.WriteLine("Stopping monitoring...");
                    _gameMonitor?.StopMonitoring();
                    break;

                case "demo":
                    Console.WriteLine("Running demo with sample cards...");
                    _logParser?.SimulateDeckParsing();
                    await Task.Delay(2000);
                    
                    // Simulate some card draws
                    Console.WriteLine("\nSimulating card draws...");
                    await Task.Delay(1000);
                    _logParser?.SimulateCardDraw("Lightning Bolt");
                    await Task.Delay(1000);
                    _logParser?.SimulateCardDraw("Island");
                    await Task.Delay(1000);
                    _logParser?.SimulateCardDraw("Lightning Bolt");
                    await Task.Delay(1000);
                    _logParser?.SimulateCardDraw("Counterspell");
                    break;

                case "draw":
                    if (!string.IsNullOrEmpty(argument))
                    {
                        _logParser?.SimulateCardDraw(argument);
                    }
                    else
                    {
                        Console.WriteLine("Usage: draw <cardname>");
                    }
                    break;

                case "clear":
                    _cardTracker?.ClearDeck();
                    break;

                case "status":
                    ShowStatus();
                    break;

                case "quit":
                case "exit":
                    _isRunning = false;
                    break;

                case "help":
                    ShowHelp();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                    break;
            }
        }
    }

    private static void OnGameStateChanged(object? sender, GameStateEventArgs e)
    {
        Console.WriteLine($"\n[EVENT] MTG Arena {(e.IsRunning ? "STARTED" : "STOPPED")}");
        if (e.IsRunning && !string.IsNullOrEmpty(e.LogFilePath))
        {
            Console.WriteLine($"[EVENT] Log file path: {e.LogFilePath}");
        }
        Console.Write("ArenaGlass> ");
    }

    private static void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
    {
        Console.WriteLine($"\n[EVENT] Library updated: {e.CardCount} cards remaining");
        Console.Write("ArenaGlass> ");
    }

    private static void ShowStatus()
    {
        Console.WriteLine("\n=== ArenaGlass Status ===");
        
        var cardCount = _cardTracker?.GetTotalCardCount() ?? 0;
        var uniqueCards = _cardTracker?.GetRemainingCards().Count ?? 0;
        
        Console.WriteLine($"Library: {cardCount} total cards, {uniqueCards} unique");
        Console.WriteLine("Monitoring: Active");
        Console.WriteLine("========================\n");
    }

    private static void ShowHelp()
    {
        Console.WriteLine("\n=== ArenaGlass Commands ===");
        Console.WriteLine("start                 - Start monitoring MTG Arena process");
        Console.WriteLine("stop                  - Stop monitoring");
        Console.WriteLine("demo                  - Run demonstration with sample cards");
        Console.WriteLine("draw <cardname>       - Simulate drawing a specific card");
        Console.WriteLine("clear                 - Clear current deck");
        Console.WriteLine("status                - Show current application status");
        Console.WriteLine("help                  - Show this help message");
        Console.WriteLine("quit/exit             - Exit the application");
        Console.WriteLine("===========================\n");
    }
}

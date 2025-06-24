using ArenaGlass.Models;
using ArenaGlass.Services;
using ArenaGlass.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArenaGlass;

/// <summary>
/// ArenaGlass MVP - MTG Arena Deck Tracker
/// 
/// This implementation demonstrates the complete MVP functionality including:
/// - Game state detection (MTG Arena process monitoring)
/// - Log file monitoring and parsing
/// - Real-time deck tracking
/// - Library card management
/// - UI service architecture (ready for WPF)
/// - Both console and UI modes
/// </summary>
class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        Console.WriteLine("ArenaGlass MVP - Choose Interface Mode:");
        Console.WriteLine("1. Console Mode (Original)");
        Console.WriteLine("2. UI Demo Mode (Shows UI MVP features)");
        Console.WriteLine();

        // Check command line args or prompt user
        string mode = "";
        if (args.Length > 0)
        {
            mode = args[0].ToLower();
        }
        else
        {
            Console.Write("Enter mode (console/ui) [ui]: ");
            mode = Console.ReadLine()?.Trim().ToLower() ?? "ui";
        }

        switch (mode)
        {
            case "console":
            case "c":
                await RunConsoleMode();
                break;
            case "ui":
            case "u":
            case "":
            default:
                await RunUIMode();
                break;
        }
    }

    private static async Task RunUIMode()
    {
        var uiService = new UIService();
        try
        {
            await uiService.RunUIAsync();
        }
        finally
        {
            uiService.Cleanup();
        }

        Console.WriteLine("\nPress any key to exit UI demo...");
        try
        {
            Console.ReadKey();
        }
        catch
        {
            // Handle case where console input is redirected
            Console.WriteLine("Demo completed successfully!");
        }
    }

    public static async Task RunConsoleMode()
    {
        var _gameMonitor = default(GameMonitorService);
        var _logParser = default(LogParserService);
        var _cardTracker = default(CardTrackingService);
        var _isRunning = true;

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
        await RunCommandLoop(_gameMonitor, _logParser, _cardTracker, () => _isRunning, (running) => _isRunning = running);

        // Cleanup
        _gameMonitor?.StopMonitoring();
        _gameMonitor?.Dispose();

        Console.WriteLine("\nArenaGlass stopped. Goodbye!");
    }

    private static async Task RunCommandLoop(GameMonitorService gameMonitor, LogParserService logParser, CardTrackingService cardTracker, Func<bool> isRunning, Action<bool> setRunning)
    {
        while (isRunning())
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
                    gameMonitor?.StartMonitoring();
                    break;

                case "stop":
                    Console.WriteLine("Stopping monitoring...");
                    gameMonitor?.StopMonitoring();
                    break;

                case "demo":
                    Console.WriteLine("Running demo with sample cards...");
                    logParser?.SimulateDeckParsing();
                    await Task.Delay(2000);
                    
                    // Simulate some card draws
                    Console.WriteLine("\nSimulating card draws...");
                    await Task.Delay(1000);
                    logParser?.SimulateCardDraw("Lightning Bolt");
                    await Task.Delay(1000);
                    logParser?.SimulateCardDraw("Island");
                    await Task.Delay(1000);
                    logParser?.SimulateCardDraw("Lightning Bolt");
                    await Task.Delay(1000);
                    logParser?.SimulateCardDraw("Counterspell");
                    break;

                case "draw":
                    if (!string.IsNullOrEmpty(argument))
                    {
                        logParser?.SimulateCardDraw(argument);
                    }
                    else
                    {
                        Console.WriteLine("Usage: draw <cardname>");
                    }
                    break;

                case "clear":
                    cardTracker?.ClearDeck();
                    break;

                case "status":
                    ShowStatus(cardTracker);
                    break;

                case "quit":
                case "exit":
                    setRunning(false);
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

    private static void ShowStatus(CardTrackingService? cardTracker)
    {
        Console.WriteLine("\n=== ArenaGlass Status ===");
        
        var cardCount = cardTracker?.GetTotalCardCount() ?? 0;
        var uniqueCards = cardTracker?.GetRemainingCards().Count ?? 0;
        
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

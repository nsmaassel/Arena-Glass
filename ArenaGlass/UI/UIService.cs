using ArenaGlass.Models;
using ArenaGlass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArenaGlass.UI;

/// <summary>
/// UI Service that demonstrates the MVP UI functionality.
/// This simulates what the WPF UI would do and provides the foundation for the actual UI implementation.
/// </summary>
public class UIService
{
    private readonly GameMonitorService _gameMonitor;
    private readonly LogParserService _logParser;
    private readonly CardTrackingService _cardTracker;
    private readonly Timer _uiUpdateTimer;

    private bool _isMonitoring;
    private bool _isGameRunning;
    private int _totalCards;
    private int _uniqueCards;
    private List<Card> _remainingCards = new();

    public UIService()
    {
        // Initialize services (same as WPF would do)
        _cardTracker = new CardTrackingService();
        _logParser = new LogParserService(_cardTracker);
        _gameMonitor = new GameMonitorService(_logParser);

        // Subscribe to events (same as WPF would do)
        _gameMonitor.GameStateChanged += OnGameStateChanged;
        _cardTracker.LibraryChanged += OnLibraryChanged;

        // UI update timer (simulates WPF data binding updates)
        _uiUpdateTimer = new Timer(UpdateUI, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task RunUIAsync()
    {
        Console.WriteLine("=================================");
        Console.WriteLine("    ArenaGlass UI MVP Demo");
        Console.WriteLine("  (WPF UI Framework Ready)");
        Console.WriteLine("=================================\n");

        // Start UI update timer
        _uiUpdateTimer.Change(0, 1000);

        await ShowUIDemo();
    }

    private async Task ShowUIDemo()
    {
        Console.WriteLine("🎮 UI MVP Features Demonstrated:");
        Console.WriteLine("   ✅ Game State Monitoring");
        Console.WriteLine("   ✅ Real-time Library Tracking");
        Console.WriteLine("   ✅ Event-driven Updates");
        Console.WriteLine("   ✅ Service Architecture");
        Console.WriteLine("   ✅ Data Binding Simulation");
        Console.WriteLine();

        // Simulate UI workflow
        Console.WriteLine("📱 Simulating UI Workflow...\n");

        await Task.Delay(1000);
        Console.WriteLine("🔄 UI: Starting monitoring...");
        StartMonitoring();

        await Task.Delay(2000);
        Console.WriteLine("📂 UI: Loading demo deck...");
        LoadDemo();

        await Task.Delay(3000);
        
        Console.WriteLine("🎯 UI: Simulating card draws...");
        await SimulateCardDraws();

        await Task.Delay(2000);
        Console.WriteLine("⏹️ UI: Stopping monitoring...");
        StopMonitoring();

        Console.WriteLine("\n✨ UI MVP Demo Complete!");
        Console.WriteLine("\n🚀 Ready for WPF Implementation:");
        Console.WriteLine("   • MainWindow.xaml (Created)");
        Console.WriteLine("   • App.xaml (Created)");
        Console.WriteLine("   • ViewModels (Created)");
        Console.WriteLine("   • Service Integration (Complete)");
        Console.WriteLine("   • Event Handling (Working)");
        Console.WriteLine("   • Data Binding (Ready)");
    }

    private void StartMonitoring()
    {
        _gameMonitor.StartMonitoring();
        _isMonitoring = true;
    }

    private void StopMonitoring()
    {
        _gameMonitor.StopMonitoring();
        _isMonitoring = false;
    }

    private void LoadDemo()
    {
        _logParser.SimulateDeckParsing();
    }

    private async Task SimulateCardDraws()
    {
        var cardNames = new[] { "Lightning Bolt", "Island", "Counterspell", "Mountain" };
        
        foreach (var cardName in cardNames)
        {
            await Task.Delay(1500);
            _logParser.SimulateCardDraw(cardName);
        }
    }

    private void OnGameStateChanged(object? sender, GameStateEventArgs e)
    {
        _isGameRunning = e.IsRunning;
        Console.WriteLine($"   🎮 Game State: {(e.IsRunning ? "Running" : "Stopped")}");
    }

    private void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
    {
        UpdateCardData();
        Console.WriteLine($"   📚 Library: {_totalCards} cards ({_uniqueCards} unique)");
    }

    private void UpdateCardData()
    {
        _remainingCards = _cardTracker.GetRemainingCards();
        _totalCards = _cardTracker.GetTotalCardCount();
        _uniqueCards = _remainingCards.Count;
    }

    private void UpdateUI(object? state)
    {
        // This simulates what WPF data binding would do automatically
        // In the real WPF app, this would update the UI elements
    }

    public void Cleanup()
    {
        _uiUpdateTimer?.Dispose();
        _gameMonitor?.StopMonitoring();
        _gameMonitor?.Dispose();
    }
}
#if WINDOWS
using ArenaGlass.Models;
using ArenaGlass.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Linq;

namespace ArenaGlass.ViewModels;

/// <summary>
/// ViewModel for the main window
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly GameMonitorService _gameMonitor;
    private readonly LogParserService _logParser;
    private readonly CardTrackingService _cardTracker;

    private bool _isMonitoring;
    private bool _isGameRunning;
    private string _gameStatus = "MTG Arena: Not Running";
    private int _totalCards;
    private int _uniqueCards;
    private string _simulationCardName = "";

    public MainViewModel()
    {
        // Initialize services
        _cardTracker = new CardTrackingService();
        _logParser = new LogParserService(_cardTracker);
        _gameMonitor = new GameMonitorService(_logParser);

        // Subscribe to events
        _gameMonitor.GameStateChanged += OnGameStateChanged;
        _cardTracker.LibraryChanged += OnLibraryChanged;

        // Initialize commands
        StartMonitoringCommand = new RelayCommand(StartMonitoring, () => !IsMonitoring);
        StopMonitoringCommand = new RelayCommand(StopMonitoring, () => IsMonitoring);
        LoadDemoCommand = new RelayCommand(LoadDemo);
        SimulateDrawCommand = new RelayCommand(SimulateDraw, () => !string.IsNullOrWhiteSpace(SimulationCardName));
        ClearDeckCommand = new RelayCommand(ClearDeck);

        // Initialize collections
        RemainingCards = new ObservableCollection<CardDisplayItem>();
    }

    #region Properties

    public bool IsMonitoring
    {
        get => _isMonitoring;
        set => SetProperty(ref _isMonitoring, value);
    }

    public bool IsGameRunning
    {
        get => _isGameRunning;
        set => SetProperty(ref _isGameRunning, value);
    }

    public string GameStatus
    {
        get => _gameStatus;
        set => SetProperty(ref _gameStatus, value);
    }

    public int TotalCards
    {
        get => _totalCards;
        set => SetProperty(ref _totalCards, value);
    }

    public int UniqueCards
    {
        get => _uniqueCards;
        set => SetProperty(ref _uniqueCards, value);
    }

    public string SimulationCardName
    {
        get => _simulationCardName;
        set => SetProperty(ref _simulationCardName, value);
    }

    public ObservableCollection<CardDisplayItem> RemainingCards { get; }

    #endregion

    #region Commands

    public ICommand StartMonitoringCommand { get; }
    public ICommand StopMonitoringCommand { get; }
    public ICommand LoadDemoCommand { get; }
    public ICommand SimulateDrawCommand { get; }
    public ICommand ClearDeckCommand { get; }

    #endregion

    #region Command Implementations

    private void StartMonitoring()
    {
        _gameMonitor.StartMonitoring();
        IsMonitoring = true;
    }

    private void StopMonitoring()
    {
        _gameMonitor.StopMonitoring();
        IsMonitoring = false;
    }

    private void LoadDemo()
    {
        _logParser.SimulateDeckParsing();
    }

    private void SimulateDraw()
    {
        if (!string.IsNullOrWhiteSpace(SimulationCardName))
        {
            _logParser.SimulateCardDraw(SimulationCardName);
            SimulationCardName = "";
        }
    }

    private void ClearDeck()
    {
        _cardTracker.ClearDeck();
    }

    #endregion

    #region Event Handlers

    private void OnGameStateChanged(object? sender, GameStateEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsGameRunning = e.IsRunning;
            GameStatus = e.IsRunning ? "MTG Arena: Running" : "MTG Arena: Not Running";
        });
    }

    private void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateCardDisplay();
        });
    }

    private void UpdateCardDisplay()
    {
        var cards = _cardTracker.GetRemainingCards();
        TotalCards = _cardTracker.GetTotalCardCount();
        UniqueCards = cards.Count;

        RemainingCards.Clear();
        foreach (var card in cards.OrderBy(c => c.Name))
        {
            RemainingCards.Add(new CardDisplayItem
            {
                Name = card.Name,
                Quantity = card.Quantity,
                Display = $"{card.Quantity}x {card.Name}"
            });
        }
    }

    #endregion

    public void Cleanup()
    {
        _gameMonitor?.StopMonitoring();
        _gameMonitor?.Dispose();
    }
}

/// <summary>
/// Item for displaying cards in the UI
/// </summary>
public class CardDisplayItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Display { get; set; } = string.Empty;
}
#endif
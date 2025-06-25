using ArenaGlass.Core.Models;

namespace ArenaGlass.Core.Services;

public interface IGameMonitorService
{
    bool IsMtgArenaRunning { get; }
    event EventHandler<bool>? GameStateChanged;
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    void StopMonitoring();
}

public interface ILogParserService
{
    event EventHandler<GameEvent>? GameEventParsed;
    Task StartParsingAsync(CancellationToken cancellationToken = default);
    void StopParsing();
}

public interface ICardTrackingService
{
    GameState CurrentGameState { get; }
    event EventHandler<GameState>? GameStateUpdated;
    void ProcessGameEvent(GameEvent gameEvent);
    void Reset();
}
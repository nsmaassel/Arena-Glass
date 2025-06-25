using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ArenaGlass.Core.Services;

public class GameMonitorService : IGameMonitorService
{
    private readonly ILogger<GameMonitorService> _logger;
    private bool _isMonitoring;
    private bool _isMtgArenaRunning;
    private Timer? _monitorTimer;

    public bool IsMtgArenaRunning
    {
        get => _isMtgArenaRunning;
        private set
        {
            if (_isMtgArenaRunning != value)
            {
                _isMtgArenaRunning = value;
                GameStateChanged?.Invoke(this, value);
                _logger.LogInformation("MTG Arena running state changed: {IsRunning}", value);
            }
        }
    }

    public event EventHandler<bool>? GameStateChanged;

    public GameMonitorService(ILogger<GameMonitorService> logger)
    {
        _logger = logger;
    }

    public Task StartMonitoringAsync(CancellationToken cancellationToken = default)
    {
        if (_isMonitoring) return Task.CompletedTask;

        _isMonitoring = true;
        _monitorTimer = new Timer(CheckGameStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        _logger.LogInformation("Started monitoring MTG Arena process");
        
        return Task.CompletedTask;
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring) return;

        _isMonitoring = false;
        _monitorTimer?.Dispose();
        _monitorTimer = null;
        _logger.LogInformation("Stopped monitoring MTG Arena process");
    }

    private void CheckGameStatus(object? state)
    {
        try
        {
            var processes = Process.GetProcessesByName("MTGA");
            IsMtgArenaRunning = processes.Length > 0;
            
            // Clean up process references
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking MTG Arena process status");
        }
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}
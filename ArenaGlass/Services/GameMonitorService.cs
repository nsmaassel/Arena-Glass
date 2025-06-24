using ArenaGlass.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArenaGlass.Services;

/// <summary>
/// Service responsible for monitoring MTG Arena process and log file.
/// </summary>
public class GameMonitorService
{
    private const string MTGA_PROCESS_NAME = "MTGA";
    private const string LOG_FILE_PATH = @"AppData\LocalLow\Wizards Of The Coast\MTGA\Player.log";
    
    private readonly LogParserService _logParser;
    private readonly Timer _processTimer;
    private bool _isGameRunning;
    private string _currentLogPath = string.Empty;

    public event EventHandler<GameStateEventArgs>? GameStateChanged;

    public GameMonitorService(LogParserService logParser)
    {
        _logParser = logParser;
        _processTimer = new Timer(CheckGameProcess, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void StartMonitoring()
    {
        Console.WriteLine("Starting MTG Arena monitoring...");
        // Check immediately and then every 5 seconds
        _processTimer.Change(0, 5000);
    }

    public void StopMonitoring()
    {
        Console.WriteLine("Stopping MTG Arena monitoring...");
        _processTimer.Change(Timeout.Infinite, Timeout.Infinite);
        if (_isGameRunning)
        {
            SetGameState(false);
        }
    }

    private void CheckGameProcess(object? state)
    {
        try
        {
            var processes = Process.GetProcessesByName(MTGA_PROCESS_NAME);
            bool gameFound = processes.Length > 0;

            if (gameFound && !_isGameRunning)
            {
                // Game started
                var logPath = GetLogFilePath();
                Console.WriteLine($"MTG Arena detected! Looking for log file at: {logPath}");
                
                if (File.Exists(logPath))
                {
                    _currentLogPath = logPath;
                    _logParser.StartWatching(logPath);
                    SetGameState(true);
                }
                else
                {
                    Console.WriteLine($"Log file not found at expected location: {logPath}");
                }
            }
            else if (!gameFound && _isGameRunning)
            {
                // Game stopped
                Console.WriteLine("MTG Arena process stopped.");
                _logParser.StopWatching();
                SetGameState(false);
            }

            // Clean up process handles
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking game process: {ex.Message}");
        }
    }

    private string GetLogFilePath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, LOG_FILE_PATH);
    }

    private void SetGameState(bool isRunning)
    {
        if (_isGameRunning != isRunning)
        {
            _isGameRunning = isRunning;
            Console.WriteLine($"Game state changed: {(isRunning ? "Running" : "Stopped")}");
            GameStateChanged?.Invoke(this, new GameStateEventArgs(isRunning, _currentLogPath));
        }
    }

    public void Dispose()
    {
        _processTimer?.Dispose();
        _logParser?.StopWatching();
    }
}
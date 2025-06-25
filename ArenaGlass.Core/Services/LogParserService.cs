using System.Text.Json;
using ArenaGlass.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArenaGlass.Core.Services;

public class LogParserService : ILogParserService
{
    private readonly ILogger<LogParserService> _logger;
    private bool _isParsing;
    private FileSystemWatcher? _fileWatcher;
    private string? _logFilePath;

    public event EventHandler<GameEvent>? GameEventParsed;

    public LogParserService(ILogger<LogParserService> logger)
    {
        _logger = logger;
    }

    public Task StartParsingAsync(CancellationToken cancellationToken = default)
    {
        if (_isParsing) return Task.CompletedTask;

        _logFilePath = GetPlayerLogPath();
        if (string.IsNullOrEmpty(_logFilePath) || !File.Exists(_logFilePath))
        {
            _logger.LogWarning("MTG Arena Player.log not found at expected location");
            return Task.CompletedTask;
        }

        _isParsing = true;
        SetupFileWatcher();
        _logger.LogInformation("Started parsing MTG Arena log file: {LogPath}", _logFilePath);
        
        return Task.CompletedTask;
    }

    public void StopParsing()
    {
        if (!_isParsing) return;

        _isParsing = false;
        _fileWatcher?.Dispose();
        _fileWatcher = null;
        _logger.LogInformation("Stopped parsing MTG Arena log file");
    }

    private string? GetPlayerLogPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var logPath = Path.Combine(userProfile, @"AppData\LocalLow\Wizards Of The Coast\MTGA\Player.log");
        return File.Exists(logPath) ? logPath : null;
    }

    private void SetupFileWatcher()
    {
        if (string.IsNullOrEmpty(_logFilePath)) return;

        var directory = Path.GetDirectoryName(_logFilePath);
        var fileName = Path.GetFileName(_logFilePath);

        if (string.IsNullOrEmpty(directory)) return;

        _fileWatcher = new FileSystemWatcher(directory, fileName);
        _fileWatcher.Changed += OnLogFileChanged;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            // Read the last few lines of the log file for new events
            var lines = ReadLastLines(_logFilePath!, 10);
            foreach (var line in lines)
            {
                if (TryParseLogLine(line, out var gameEvent))
                {
                    GameEventParsed?.Invoke(this, gameEvent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing log file changes");
        }
    }

    private List<string> ReadLastLines(string filePath, int lineCount)
    {
        var lines = new List<string>();
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);
            
            var allLines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                allLines.Add(line);
            }

            // Return the last N lines
            var startIndex = Math.Max(0, allLines.Count - lineCount);
            return allLines.Skip(startIndex).ToList();
        }
        catch
        {
            return lines;
        }
    }

    private bool TryParseLogLine(string line, out GameEvent gameEvent)
    {
        gameEvent = new GameEvent();
        
        try
        {
            // Simple parsing - look for JSON-like structures in the log
            if (line.Contains("\"type\":") && line.Contains("\"data\":"))
            {
                gameEvent.Timestamp = DateTime.Now;
                gameEvent.EventType = "LogEvent";
                gameEvent.Data = line;
                return true;
            }

            // Look for specific game events
            if (line.Contains("Draw") || line.Contains("Library"))
            {
                gameEvent.Timestamp = DateTime.Now;
                gameEvent.EventType = "DrawEvent";
                gameEvent.Data = line;
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse log line: {Line}", line);
        }

        return false;
    }

    public void Dispose()
    {
        StopParsing();
    }
}
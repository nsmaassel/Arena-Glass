using ArenaGlass.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace ArenaGlass.Services;

/// <summary>
/// Service responsible for parsing MTG Arena log files and extracting game events.
/// </summary>
public class LogParserService
{
    private readonly CardTrackingService _cardTracker;
    private FileSystemWatcher? _fileWatcher;
    private long _lastReadPosition;
    private bool _isWatching;

    // Regex patterns for parsing log events
    private static readonly Regex DecklistPattern = new Regex(
        @"""deck"":\s*{.*?""mainDeck"":\s*\[(.*?)\]", 
        RegexOptions.Compiled | RegexOptions.Singleline);
    
    private static readonly Regex CardPattern = new Regex(
        @"""quantity"":\s*(\d+),\s*""id"":\s*""(\d+)""", 
        RegexOptions.Compiled);
    
    private static readonly Regex CardDrawnPattern = new Regex(
        @"CardId=(\d+).*?Zone.*?Library.*?Hand", 
        RegexOptions.Compiled);
    
    private static readonly Regex CardPlayedPattern = new Regex(
        @"CardId=(\d+).*?Zone.*?Hand.*?Battlefield", 
        RegexOptions.Compiled);

    // Simple card ID to name mapping (in a real implementation, this would be loaded from a database)
    private static readonly Dictionary<string, string> CardIdToName = new Dictionary<string, string>
    {
        // Add some common card mappings for testing
        { "75426", "Lightning Bolt" },
        { "69917", "Island" },
        { "69915", "Mountain" },
        { "69913", "Forest" },
        { "69911", "Plains" },
        { "69919", "Swamp" },
        { "72638", "Counterspell" },
        { "72642", "Giant Growth" },
        { "69912", "Healing Salve" },
        { "69920", "Dark Ritual" },
        // This would be populated from a complete card database
    };

    public LogParserService(CardTrackingService cardTracker)
    {
        _cardTracker = cardTracker;
    }

    public void StartWatching(string logFilePath)
    {
        if (_isWatching)
        {
            StopWatching();
        }

        try
        {
            _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(logFilePath)!, Path.GetFileName(logFilePath));
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            _fileWatcher.Changed += OnLogFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
            _isWatching = true;

            // Read existing content
            _lastReadPosition = 0;
            ReadLogUpdates(logFilePath);

            Console.WriteLine($"Started watching log file: {logFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting log file watcher: {ex.Message}");
        }
    }

    public void StopWatching()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }
        _isWatching = false;
        Console.WriteLine("Stopped watching log file");
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            ReadLogUpdates(e.FullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading log file updates: {ex.Message}");
        }
    }

    private void ReadLogUpdates(string filePath)
    {
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(_lastReadPosition, SeekOrigin.Begin);
            
            using var reader = new StreamReader(fileStream);
            string? line;
            var newContent = new List<string>();
            
            while ((line = reader.ReadLine()) != null)
            {
                newContent.Add(line);
            }
            
            _lastReadPosition = fileStream.Position;
            
            if (newContent.Count > 0)
            {
                ProcessLogLines(newContent);
            }
        }
        catch (IOException)
        {
            // File might be locked, try again later
        }
    }

    private void ProcessLogLines(List<string> lines)
    {
        var content = string.Join("\n", lines);
        
        // Look for new game/deck events
        if (content.Contains("\"method\":\"Event_Join\"") || content.Contains("\"method\":\"Event_GetDeck\""))
        {
            ParseDeckList(content);
        }
        
        // Look for card events
        foreach (var line in lines)
        {
            if (line.Contains("Zone_Library") && line.Contains("Zone_Hand"))
            {
                ParseCardDraw(line);
            }
            else if (line.Contains("Zone_Hand") && (line.Contains("Zone_Battlefield") || line.Contains("Zone_Graveyard")))
            {
                ParseCardPlay(line);
            }
        }
    }

    private void ParseDeckList(string content)
    {
        try
        {
            var deckMatch = DecklistPattern.Match(content);
            if (deckMatch.Success)
            {
                var deckContent = deckMatch.Groups[1].Value;
                var cardMatches = CardPattern.Matches(deckContent);
                
                var deck = new List<Card>();
                foreach (Match cardMatch in cardMatches)
                {
                    var quantity = int.Parse(cardMatch.Groups[1].Value);
                    var cardId = cardMatch.Groups[2].Value;
                    var cardName = GetCardName(cardId);
                    
                    if (!string.IsNullOrEmpty(cardName))
                    {
                        deck.Add(new Card(cardName, quantity));
                    }
                }
                
                if (deck.Count > 0)
                {
                    _cardTracker.SetStartingDeck(deck);
                    Console.WriteLine($"Parsed deck with {deck.Count} unique cards from log");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing deck list: {ex.Message}");
        }
    }

    private void ParseCardDraw(string line)
    {
        try
        {
            var match = CardDrawnPattern.Match(line);
            if (match.Success)
            {
                var cardId = match.Groups[1].Value;
                var cardName = GetCardName(cardId);
                
                if (!string.IsNullOrEmpty(cardName))
                {
                    _cardTracker.CardDrawn(cardName);
                    Console.WriteLine($"Card drawn from log: {cardName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing card draw: {ex.Message}");
        }
    }

    private void ParseCardPlay(string line)
    {
        try
        {
            var match = CardPlayedPattern.Match(line);
            if (match.Success)
            {
                var cardId = match.Groups[1].Value;
                var cardName = GetCardName(cardId);
                
                if (!string.IsNullOrEmpty(cardName))
                {
                    Console.WriteLine($"Card played from hand: {cardName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing card play: {ex.Message}");
        }
    }

    private string GetCardName(string cardId)
    {
        return CardIdToName.TryGetValue(cardId, out var name) ? name : $"Unknown Card ({cardId})";
    }

    /// <summary>
    /// Simulates parsing a deck for testing purposes.
    /// </summary>
    public void SimulateDeckParsing()
    {
        Console.WriteLine("Simulating deck parsing from log file...");
        _cardTracker.AddSampleCards();
    }

    /// <summary>
    /// Simulates drawing cards for testing purposes.
    /// </summary>
    public void SimulateCardDraw(string cardName)
    {
        Console.WriteLine($"Simulating card draw: {cardName}");
        _cardTracker.CardDrawn(cardName);
    }
}
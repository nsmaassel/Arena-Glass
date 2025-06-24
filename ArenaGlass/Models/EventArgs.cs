using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaGlass.Models;

/// <summary>
/// Event arguments for game state changes.
/// </summary>
public class GameStateEventArgs : EventArgs
{
    public bool IsRunning { get; }
    public string LogFilePath { get; }

    public GameStateEventArgs(bool isRunning, string logFilePath = "")
    {
        IsRunning = isRunning;
        LogFilePath = logFilePath;
    }
}

/// <summary>
/// Event arguments for library changes.
/// </summary>
public class LibraryChangedEventArgs : EventArgs
{
    public List<Card> RemainingCards { get; }
    public int CardCount { get; }

    public LibraryChangedEventArgs(List<Card> remainingCards)
    {
        RemainingCards = remainingCards;
        CardCount = remainingCards.Sum(c => c.Quantity);
    }
}
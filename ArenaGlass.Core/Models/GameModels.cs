namespace ArenaGlass.Core.Models;

public class Card
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ManaCost { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class GameState
{
    public List<Card> Library { get; set; } = new();
    public int TotalCardsInLibrary { get; set; }
    public bool IsGameActive { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class GameEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
using System;

namespace ArenaGlass.Models;

/// <summary>
/// Represents a Magic: The Gathering card.
/// </summary>
public class Card
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ManaCost { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int ConvertedManaCost { get; set; }

    public Card() { }

    public Card(string name, int quantity = 1)
    {
        Name = name;
        Quantity = quantity;
    }

    public override string ToString()
    {
        return Quantity > 1 ? $"{Quantity}x {Name}" : Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is Card other && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
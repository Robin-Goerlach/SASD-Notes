namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Beschreibt eine Überschrift aus einer Markdown-Notiz.
/// </summary>
public sealed class OutlineItem
{
    /// <summary>
    /// Ebene der Überschrift, abgeleitet aus der Anzahl der führenden Rauten.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Sichtbarer Text der Überschrift.
    /// </summary>
    public required string HeadingText { get; init; }

    /// <summary>
    /// Startindex der Überschrift im Text.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Liefert eine lesbare Darstellung für Listen in der Oberfläche.
    /// </summary>
    public override string ToString()
    {
        return $"{new string(' ', Math.Max(0, (Level - 1) * 2))}{HeadingText}";
    }

}

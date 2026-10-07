namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Beschreibt einen Rückverweis auf eine Notiz.
/// </summary>
public sealed class BacklinkInfo
{
    /// <summary>
    /// Titel der verweisenden Quellnotiz.
    /// </summary>
    public required string SourceTitle { get; init; }

    /// <summary>
    /// Relativer Pfad der verweisenden Quellnotiz.
    /// </summary>
    public required string SourceRelativePath { get; init; }

    /// <summary>
    /// Kurzer Kontextausschnitt des Verweises.
    /// </summary>
    public required string ContextSnippet { get; init; }

    /// <summary>
    /// Liefert eine lesbare Darstellung für Listen in der Oberfläche.
    /// </summary>
    public override string ToString()
    {
        return $"{SourceTitle} — {ContextSnippet}";
    }

}

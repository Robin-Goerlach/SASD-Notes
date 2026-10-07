namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Beschreibt einen Suchtreffer innerhalb des Vaults.
/// </summary>
public sealed class SearchHit
{
    /// <summary>
    /// Titel der betroffenen Notiz.
    /// </summary>
    public required string NoteTitle { get; init; }

    /// <summary>
    /// Relativer Pfad der betroffenen Notiz.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Kurzer Vorschautext um die Trefferstelle herum.
    /// </summary>
    public required string PreviewText { get; init; }

    /// <summary>
    /// Einfacher Score zur Sortierung der Treffer.
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// Gefundene Position im Dokument, sofern bekannt.
    /// </summary>
    public int MatchIndex { get; init; }

    /// <summary>
    /// Liefert eine lesbare Darstellung für Listen in der Oberfläche.
    /// </summary>
    public override string ToString()
    {
        return $"{NoteTitle} — {PreviewText}";
    }

}

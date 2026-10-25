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
    /// Bereiche der Notiz, in denen die Suchanfrage gefunden wurde.
    /// </summary>
    public SearchMatchKind MatchKinds { get; init; }

    /// <summary>
    /// Einfacher Score zur Sortierung der Treffer.
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// Gefundene UTF-16-Position im Originaldokument oder -1 bei einem reinen Titel-/Pfadtreffer.
    /// </summary>
    public int MatchIndex { get; init; } = -1;

    /// <summary>
    /// Länge des tatsächlichen Inhaltstreffers in UTF-16-Zeichenpositionen; 0 bei reinen Titel-/Pfadtreffern.
    /// </summary>
    public int MatchLength { get; init; }

    /// <summary>
    /// Exakter Inhaltstreffer in der ursprünglichen Groß-/Kleinschreibung; leer bei Titel- oder Pfadtreffern.
    /// </summary>
    public string MatchedText { get; init; } = string.Empty;

    /// <summary>
    /// Liefert eine lesbare Darstellung für Listen in der Oberfläche.
    /// </summary>
    public override string ToString()
    {
        return $"{NoteTitle} — {RelativePath} — {PreviewText}";
    }

}

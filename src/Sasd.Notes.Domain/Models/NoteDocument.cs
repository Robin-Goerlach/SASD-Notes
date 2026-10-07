namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Repräsentiert eine einzelne Markdown-Notiz innerhalb des Vaults.
/// </summary>
public sealed class NoteDocument
{
    /// <summary>
    /// Eindeutige Kennung der Notiz innerhalb des Vaults.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Titel der Notiz.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Vollständiger Dateisystempfad zur Markdown-Datei.
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>
    /// Relativer Pfad der Notiz innerhalb des Vaults.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gesamter Markdown-Inhalt der Notiz.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Liste der im Inhalt erkannten Wiki-Links.
    /// </summary>
    public IReadOnlyList<WikiLink> Links { get; set; } = Array.Empty<WikiLink>();

    /// <summary>
    /// Zeitpunkt der letzten Dateisystemänderung in UTC.
    /// </summary>
    public DateTime LastModifiedUtc { get; set; }

    /// <summary>
    /// Kennzeichnet, ob der aktuelle Editorstand vom gespeicherten Stand abweicht.
    /// </summary>
    public bool IsDirty { get; set; }
}

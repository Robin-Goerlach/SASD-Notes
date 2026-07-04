using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Domain.Interfaces;

/// <summary>
/// Beschreibt einen Parser für Markdown-Überschriften.
/// </summary>
public interface IOutlineParser
{
    /// <summary>
    /// Liest die Gliederung aus dem angegebenen Markdown-Text.
    /// </summary>
    /// <param name="markdownContent">Zu analysierender Markdown-Text.</param>
    /// <returns>Gefundene Überschriften.</returns>
    IReadOnlyList<OutlineItem> Parse(string markdownContent);
}

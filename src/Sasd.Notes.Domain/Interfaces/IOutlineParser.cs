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
    /// <remarks>
    /// Eingefasste Codeblöcke werden ausgelassen. Positionen beziehen sich unverändert auf den Originaltext.
    /// Die unterstützte Markdown-Teilmenge ist in spec/markdown-code-blocks.md beschrieben.
    /// </remarks>
    IReadOnlyList<OutlineItem> Parse(string markdownContent);
}

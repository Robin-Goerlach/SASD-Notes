using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Domain.Interfaces;

/// <summary>
/// Beschreibt einen Parser für Wiki-Links in Markdown-Texten.
/// </summary>
public interface IWikiLinkParser
{
    /// <summary>
    /// Extrahiert alle Wiki-Links aus dem angegebenen Markdown-Text.
    /// </summary>
    /// <param name="markdownContent">Zu analysierender Markdown-Text.</param>
    /// <returns>Gefundene Wiki-Links.</returns>
    /// <remarks>
    /// Eingefasste Codeblöcke und einzeilige Inline-Code-Spans werden ausgelassen.
    /// Positionen beziehen sich unverändert auf den Originaltext.
    /// Die unterstützte Markdown-Teilmenge ist in spec/markdown-code-blocks.md beschrieben.
    /// </remarks>
    IReadOnlyList<WikiLink> Parse(string markdownContent);
}

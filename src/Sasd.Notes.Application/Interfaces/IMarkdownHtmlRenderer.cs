namespace Sasd.Notes.Application.Interfaces;

/// <summary>
/// Rendert Markdown als eigenständiges HTML-Dokument für eine schreibgeschützte Leseansicht.
/// </summary>
/// <remarks>
/// Die Schnittstelle hält die Application-Schicht unabhängig von der konkreten
/// Markdown-Bibliothek und vom verwendeten Browser-Control.
/// </remarks>
public interface IMarkdownHtmlRenderer
{
    /// <summary>
    /// Erstellt aus Markdown ein vollständiges, lokal darstellbares HTML-Dokument.
    /// </summary>
    /// <param name="markdown">Unveränderter Markdown-Quelltext.</param>
    /// <returns>Vollständiges HTML-Dokument einschließlich Sicherheitsrichtlinie und Darstellung.</returns>
    string Render(string markdown);
}

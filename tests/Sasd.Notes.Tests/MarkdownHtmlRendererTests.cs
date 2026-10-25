using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Infrastructure.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft die sichere und deterministische Markdown-zu-HTML-Transformation.
/// </summary>
internal static class MarkdownHtmlRendererTests
{
    /// <summary>
    /// Führt Darstellungs-, Unicode- und Sicherheitsprüfungen aus.
    /// </summary>
    public static void Run()
    {
        IMarkdownHtmlRenderer renderer = new MarkdigMarkdownHtmlRenderer();

        string html = renderer.Render(
            "# Überschrift 😀\n\n**Fett** und ~~durchgestrichen~~.\n\n- [ ] Aufgabe");

        AssertEx.True(html.Contains("<h1", StringComparison.Ordinal),
            "Eine Markdown-Überschrift muss als HTML-Überschrift gerendert werden.");
        AssertEx.True(html.Contains("<strong>Fett</strong>", StringComparison.Ordinal),
            "Fettschrift muss semantisches HTML erzeugen.");
        AssertEx.True(html.Contains("<del>durchgestrichen</del>", StringComparison.Ordinal),
            "Die erweiterte Pipeline muss Durchstreichung darstellen.");
        AssertEx.True(html.Contains("Überschrift 😀", StringComparison.Ordinal),
            "Unicode-Inhalte müssen unverändert im gerenderten Dokument ankommen.");
        AssertEx.True(html.Contains("Content-Security-Policy", StringComparison.Ordinal),
            "Jedes Dokument benötigt eine eingebettete Content-Security-Policy.");

        string unsafeHtml = renderer.Render(
            "<script>alert('nicht ausführen')</script>\n\n<div onclick=\"evil()\">Text</div>");
        AssertEx.False(unsafeHtml.Contains("<script>", StringComparison.OrdinalIgnoreCase),
            "Markdown darf keine ausführbaren Script-Tags in das Ergebnis übernehmen.");
        AssertEx.False(unsafeHtml.Contains("<div onclick=", StringComparison.OrdinalIgnoreCase),
            "Eingebettete HTML-Ereignishandler dürfen nicht als ausführbares HTML in die Vorschau gelangen.");

        string emptyHtml = renderer.Render(string.Empty);
        AssertEx.True(emptyHtml.Contains("<main class=\"markdown-body\">", StringComparison.Ordinal),
            "Auch ein leeres Dokument muss einen gültigen Darstellungsrahmen erhalten.");
    }
}

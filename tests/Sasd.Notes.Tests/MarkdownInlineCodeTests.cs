using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft einzeilige Inline-Codebereiche, Literalmarker und unveränderte Positionen echter Wiki-Links.
/// </summary>
internal static class MarkdownInlineCodeTests
{
    /// <summary>
    /// Führt Syntax-, Escape-, Zeilengrenzen- und Positionsprüfungen aus.
    /// </summary>
    public static void Run()
    {
        AssertTargets("Text `[[Beispiel]]` [[Echt]]", "Echt");
        AssertTargets("Text ``[[Beispiel ` innen]]`` [[Echt]]", "Echt");
        AssertTargets("Text ``[[A]]` [[B]]`` [[C]]", "C");
        AssertTargets("Text `[[A]]`` [[B]]` [[C]]", "C");
        AssertTargets("Text `[[A]]`` [[B]]", "A", "B");
        AssertTargets("`[[Echt]]", "Echt");
        AssertTargets("Text ``[[A]]` [[B]]", "A", "B");

        // Ein offener Marker verhindert keine späteren, unabhängig passenden Markergruppen.
        AssertTargets("Text ` [[A]] ``[[B]]`` [[C]]", "A", "C");

        // Escape-Verhalten außerhalb des Codes; innerhalb eines Code-Spans bleiben Backslashes wörtlich.
        AssertTargets(@"\`[[Echt]]", "Echt");
        AssertTargets(@"\\`[[Beispiel]]`");
        AssertTargets(@"\\\`[[Echt]]", "Echt");
        AssertTargets(@"\``[[Beispiel]]`");
        AssertTargets(@"Text `[[Beispiel]]\` [[Echt]]", "Echt");

        // Die vollständige Wiki-Link-Syntax darf auch nicht teilweise mit Code überlappen.
        AssertTargets("[[Vor`Code`handen]] [[Echt]]", "Echt");
        AssertTargets("`Beispiel`[[Echt]]", "Echt");
        AssertTargets("~[[Echt]]~", "Echt");
        AssertTargets(string.Empty);

        foreach (string newline in new[] { "\n", "\r\n" })
        {
            string markdown = "😀 `[[Beispiel]]`" + newline + "Ω [[Echt|Alias]]";
            IReadOnlyList<WikiLink> links = new WikiLinkParser().Parse(markdown);
            AssertEx.Equal(1, links.Count, "Nur der echte Link darf nach dem Inline-Code übrig bleiben.");
            AssertEx.Equal("Alias", links[0].Alias, "Der Alias eines echten Links muss erhalten bleiben.");
            AssertEx.Equal(markdown.IndexOf("[[Echt", StringComparison.Ordinal), links[0].StartIndex,
                "Die Position muss Unicode und LF/CRLF im Originaltext berücksichtigen.");
            AssertEx.Equal(links[0].RawText, markdown.Substring(links[0].StartIndex, links[0].Length),
                "Der gemeldete Bereich muss den Original-Linktext exakt abdecken.");

            // Die bewusst einzeilige V1-Erkennung darf keine Backticks über verschiedene Zeilen hinweg paaren.
            AssertTargets("Text `[[A]]" + newline + "[[B]]` [[C]]", "A", "B", "C");

            // Fence-Inhalte werden vor dem Inline-Scan abgegrenzt und dürfen dessen Paarbildung nicht beeinflussen.
            AssertTargets("Text `[[A]]`" + newline + "```markdown" + newline +
                "`[[B]]" + newline + "```" + newline + "[[C]]`", "C");

            IReadOnlyList<OutlineItem> outline = new MarkdownOutlineParser().Parse(
                "# Titel `[[Beispiel]]`" + newline + "## Abschnitt");
            AssertEx.Equal(2, outline.Count, "Inline-Code im Titel darf eine echte Überschrift nicht aus der Gliederung entfernen.");
        }

        // Während des Schreibens werden offene Marker bei jeder Auswertung erneut als Text behandelt.
        var parser = new WikiLinkParser();
        AssertEx.Equal(1, parser.Parse("Text `[[Echt]]").Count, "Ein nicht geschlossener Marker darf keinen Codebereich vortäuschen.");
        AssertEx.Equal(0, parser.Parse("Text `[[Echt]]`").Count, "Ein nachträglich geschlossener Marker muss den Codebereich aktivieren.");
        AssertEx.Equal(1, parser.Parse("Text `[[Echt]]").Count, "Nach Entfernen des Abschlusses darf kein alter Parserzustand erhalten bleiben.");
    }

    private static void AssertTargets(string markdown, params string[] expectedTargets)
    {
        IReadOnlyList<WikiLink> links = new WikiLinkParser().Parse(markdown);
        AssertEx.Equal(expectedTargets.Length, links.Count,
            "Die Anzahl der Wiki-Links muss zu den echten Zielen außerhalb des Inline-Codes passen.");
        for (int index = 0; index < expectedTargets.Length; index++)
        {
            AssertEx.Equal(expectedTargets[index], links[index].Target,
                "Die echten Wiki-Link-Ziele müssen in ihrer ursprünglichen Reihenfolge erhalten bleiben.");
        }
    }
}

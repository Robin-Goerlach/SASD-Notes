using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Tests;

/// <summary>
/// Kleines paketfreies Test-Harness-Projekt, damit die Solution ohne externe Testpakete gebaut werden kann.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Führt einige Kernprüfungen der Fachlogik aus.
    /// </summary>
    private static int Main()
    {
        try
        {
            RunWikiLinkParserTests();
            RunOutlineParserTests();
            RunSearchServiceTests();
            RunEditorFormattingTests();
            RunLinkResolverTests();

            Console.WriteLine("Alle SASD Notes Kernprüfungen wurden erfolgreich ausgeführt.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Testfehler: " + ex.Message);
            return 1;
        }
    }

    private static void RunWikiLinkParserTests()
    {
        var parser = new WikiLinkParser();
        IReadOnlyList<WikiLink> links = parser.Parse("Siehe [[Projektplan]] und [[Ordner/Notiz|Alias]].");
        AssertEx.Equal(2, links.Count, "WikiLinkParser sollte zwei Links finden.");
        AssertEx.Equal("Projektplan", links[0].Target, "Erster Wiki-Link sollte korrekt erkannt werden.");
        AssertEx.Equal("Alias", links[1].Alias, "Alias des zweiten Links sollte erkannt werden.");
    }

    private static void RunOutlineParserTests()
    {
        var parser = new MarkdownOutlineParser();
        IReadOnlyList<OutlineItem> outline = parser.Parse("# Titel

## Abschnitt
Text
### Unterpunkt");
        AssertEx.Equal(3, outline.Count, "Gliederungsparser sollte drei Überschriften finden.");
        AssertEx.Equal(1, outline[0].Level, "Erste Überschrift sollte Ebene 1 haben.");
        AssertEx.Equal("Unterpunkt", outline[2].HeadingText, "Dritte Überschrift sollte korrekt gelesen werden.");
    }

    private static void RunSearchServiceTests()
    {
        var searchService = new SearchService();
        var notes = new List<NoteDocument>
        {
            new()
            {
                Id = "eins",
                Title = "Projektplan",
                FullPath = "C:/Vault/Projektplan.md",
                RelativePath = "Projektplan.md",
                Content = "Dies ist der Projektplan für SASD Notes.",
                LastModifiedUtc = DateTime.UtcNow
            },
            new()
            {
                Id = "zwei",
                Title = "Meeting",
                FullPath = "C:/Vault/Meeting.md",
                RelativePath = "Meeting.md",
                Content = "Offene Punkte und Aufgaben.",
                LastModifiedUtc = DateTime.UtcNow
            }
        };

        IReadOnlyList<SearchHit> hits = searchService.Search(notes, "Projektplan");
        AssertEx.True(hits.Count >= 1, "Suche sollte mindestens einen Treffer liefern.");
        AssertEx.Equal("Projektplan", hits[0].NoteTitle, "Treffer sollte die richtige Notiz priorisieren.");
    }

    private static void RunEditorFormattingTests()
    {
        var formattingService = new EditorFormattingService();
        TextSelectionTransformResult result = formattingService.Apply("Hallo Welt", 6, 4, MarkdownFormatKind.Bold);
        AssertEx.Equal("Hallo **Welt**", result.UpdatedText, "Bold-Formatierung sollte die Auswahl umschließen.");
    }

    private static void RunLinkResolverTests()
    {
        var resolver = new LinkResolver();
        var notes = new List<NoteDocument>
        {
            new()
            {
                Id = "eins",
                Title = "Projektplan",
                FullPath = "C:/Vault/Projekte/Projektplan.md",
                RelativePath = "Projekte/Projektplan.md",
                Content = string.Empty,
                LastModifiedUtc = DateTime.UtcNow
            }
        };

        NoteDocument? resolved = resolver.Resolve("Projekte/Projektplan", notes);
        AssertEx.True(resolved is not null, "LinkResolver sollte vorhandene Notizen auflösen.");
    }
}

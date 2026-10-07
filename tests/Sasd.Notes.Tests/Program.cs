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
            RunBacklinkServiceTests();

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
        IReadOnlyList<OutlineItem> outline = parser.Parse(
            "# Titel\n\n## Abschnitt\nText\n### Unterpunkt");
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

        TextSelectionTransformResult bold = formattingService.Apply(
            "Hallo Welt",
            6,
            4,
            MarkdownFormatKind.Bold);
        AssertEx.Equal("Hallo **Welt**", bold.UpdatedText, "Bold-Formatierung sollte die Auswahl umschließen.");
        AssertEx.Equal(8, bold.SelectionStart, "Die Auswahl sollte hinter dem Bold-Präfix beginnen.");
        AssertEx.Equal(4, bold.SelectionLength, "Die Auswahl sollte den ursprünglichen Text markieren.");

        TextSelectionTransformResult emptySelection = formattingService.Apply(
            string.Empty,
            50,
            20,
            MarkdownFormatKind.WikiLink);
        AssertEx.Equal("[[Notiz]]", emptySelection.UpdatedText, "Eine leere Auswahl sollte einen Wiki-Link-Platzhalter erzeugen.");
        AssertEx.Equal(2, emptySelection.SelectionStart, "Der Wiki-Link-Platzhalter sollte ohne Klammern markiert werden.");

        TextSelectionTransformResult multiLine = formattingService.Apply(
            "Erste\nZweite",
            2,
            8,
            MarkdownFormatKind.BulletList);
        AssertEx.Equal("- Erste\n- Zweite", multiLine.UpdatedText, "Eine Auswahl über mehrere Zeilen sollte jede Zeile formatieren.");

        TextSelectionTransformResult crlf = formattingService.Apply(
            "Erste\r\nZweite",
            0,
            13,
            MarkdownFormatKind.NumberedList);
        AssertEx.Equal("1. Erste\r\n2. Zweite", crlf.UpdatedText, "CRLF-Zeilenumbrüche müssen erhalten bleiben.");

        TextSelectionTransformResult unicode = formattingService.Apply(
            "Äpfel 😀",
            -10,
            500,
            MarkdownFormatKind.Italic);
        AssertEx.Equal("*Äpfel 😀*", unicode.UpdatedText, "Unicode und ungültige Auswahlgrenzen müssen verarbeitet werden.");
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

    private static void RunBacklinkServiceTests()
    {
        var resolver = new LinkResolver();
        var backlinkService = new BacklinkService(resolver);
        var target = new NoteDocument
        {
            Id = "ziel",
            Title = "Ziel",
            FullPath = "C:/Vault/Ziel.md",
            RelativePath = "Ziel.md",
            Content = string.Empty,
            LastModifiedUtc = DateTime.UtcNow
        };
        var source = new NoteDocument
        {
            Id = "quelle",
            Title = "Quelle",
            FullPath = "C:/Vault/Quelle.md",
            RelativePath = "Quelle.md",
            Content = "Siehe [[Ziel]].",
            Links = new WikiLinkParser().Parse("Siehe [[Ziel]]."),
            LastModifiedUtc = DateTime.UtcNow
        };

        IReadOnlyList<BacklinkInfo> backlinks = backlinkService.GetBacklinks(target, new[] { target, source });
        AssertEx.Equal(1, backlinks.Count, "BacklinkService sollte den Rückverweis finden.");
        AssertEx.Equal("Quelle.md", backlinks[0].SourceRelativePath, "Der Quellpfad des Backlinks sollte erhalten bleiben.");
    }
}

using Sasd.Notes.Application.Services;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.Tests;

/// <summary>
/// Kleines paketfreies Test-Harness-Projekt, damit die Solution ohne externe Testpakete gebaut werden kann.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Führt einige Kernprüfungen der Fachlogik aus.
    /// </summary>
    private static async Task<int> Main()
    {
        try
        {
            RunWikiLinkParserTests();
            RunOutlineParserTests();
            RunSearchServiceTests();
            RunEditorFormattingTests();
            RunLinkResolverTests();
            RunBacklinkServiceTests();
            await RunUnresolvedLinkTests();
            await RunFileSystemRepositoryTests();

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

    private static async Task RunUnresolvedLinkTests()
    {
        string temporaryVault = Path.Combine(Path.GetTempPath(), "sasd-notes-link-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryVault);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(temporaryVault, "Quelle.md"),
                "[[Vorhanden]] und [[Fehlt]].");
            await File.WriteAllTextAsync(Path.Combine(temporaryVault, "Vorhanden.md"), "# Vorhanden");

            var parser = new WikiLinkParser();
            var resolver = new LinkResolver();
            var repository = new FileSystemVaultRepository(parser);
            var workspace = new NotesWorkspaceService(
                repository,
                new InMemorySettingsRepository(),
                parser,
                new MarkdownOutlineParser(),
                resolver,
                new SearchService(),
                new BacklinkService(resolver));

            await workspace.InitializeAsync();
            await workspace.OpenVaultAsync(temporaryVault);

            NoteDocument sourceNote = workspace.LoadedNotes.Single(note => note.Title == "Quelle");
            IReadOnlyList<WikiLink> unresolved = workspace.GetUnresolvedLinks(sourceNote);

            AssertEx.Equal(1, unresolved.Count, "Nicht vorhandene Wiki-Links sollten erkannt werden.");
            AssertEx.Equal("Fehlt", unresolved[0].Target, "Das nicht auflösbare Ziel sollte erhalten bleiben.");
        }
        finally
        {
            // Der Test-Vault wird nach dem Test vollständig entfernt.
            if (Directory.Exists(temporaryVault))
            {
                Directory.Delete(temporaryVault, recursive: true);
            }
        }
    }

    private sealed class InMemorySettingsRepository : ISettingsRepository
    {
        private readonly AppSettings _settings = new();

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_settings);
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private static async Task RunFileSystemRepositoryTests()
    {
        string temporaryVault = Path.Combine(Path.GetTempPath(), "sasd-notes-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryVault);

        try
        {
            string notePath = Path.Combine(temporaryVault, "Notiz.md");
            await File.WriteAllTextAsync(notePath, "# Ursprünglich\n\nInhalt");

            var repository = new FileSystemVaultRepository(new WikiLinkParser());
            IReadOnlyList<NoteDocument> notes = await repository.LoadAllNotesAsync(temporaryVault);
            AssertEx.Equal(1, notes.Count, "Das Repository sollte die Markdown-Datei laden.");

            NoteDocument note = notes[0];
            note.Content = "# Aktualisiert\n\nÄnderung 😀";
            await repository.SaveNoteAsync(temporaryVault, note);
            AssertEx.Equal(note.Content, await File.ReadAllTextAsync(notePath), "Der atomare Speichervorgang sollte den vollständigen Inhalt schreiben.");

            await File.WriteAllTextAsync(notePath, "# Extern geändert");
            File.SetLastWriteTimeUtc(notePath, note.LastModifiedUtc.AddSeconds(1));
            note.Content = "# Lokale Änderung";

            bool conflictDetected = false;
            try
            {
                await repository.SaveNoteAsync(temporaryVault, note);
            }
            catch (IOException)
            {
                conflictDetected = true;
            }

            AssertEx.True(conflictDetected, "Externe Änderungen dürfen nicht stillschweigend überschrieben werden.");
        }
        finally
        {
            // Der Test hinterlässt keine temporären Benutzerdaten.
            if (Directory.Exists(temporaryVault))
            {
                Directory.Delete(temporaryVault, recursive: true);
            }
        }
    }
}

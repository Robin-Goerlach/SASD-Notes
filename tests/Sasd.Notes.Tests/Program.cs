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
            RunFileNameHelperTests();
            RunWikiLinkParserTests();
            RunOutlineParserTests();
            MarkdownCodeBlockTests.Run();
            RunSearchServiceTests();
            RunDisplayFormattingTests();
            RunEditorFormattingTests();
            RunFormatterLineBoundaryTests();
            RunLinkResolverTests();
            LinkResolutionTests.Run();
            RunBacklinkServiceTests();
            await RunUnresolvedLinkTests();
            await RunFolderTreeTests();
            await RunReloadNoteTests();
            await NoteReloadTests.RunAsync();
            await RunFileSystemRepositoryTests();
            await RunConcurrentNoteCreationTests();
            await WorkspaceSwitchTests.RunAsync();
            await WorkspaceTransitionTests.RunAsync();
            await WorkspaceSaveTests.RunAsync();

            Console.WriteLine("Alle SASD Notes Kernprüfungen wurden erfolgreich ausgeführt.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Testfehler: " + ex.Message);
            return 1;
        }
    }

    private static void RunFileNameHelperTests()
    {
        AssertEx.Equal("Projektplan", FileNameHelper.ToSafeFileName(" Projektplan "), "Normale Dateinamen sollten lesbar normalisiert werden.");
        AssertEx.Equal("Notiz", FileNameHelper.ToSafeFileName("Notiz..."), "Abschließende Punkte sollten entfernt werden.");
        AssertEx.Equal("CON_", FileNameHelper.ToSafeFileName("CON"), "Reservierte Windows-Gerätenamen müssen entschärft werden.");
        AssertEx.Equal("Neue Notiz", FileNameHelper.ToSafeFileName("///"), "Ein vollständig ungültiger Titel benötigt einen sicheren Fallback.");
        AssertEx.Equal("CON_.md", FileNameHelper.ToSafeFileName("CON.md"), "Der sichere Zusatz muss vor der Dateiendung stehen.");
        AssertEx.Equal("Projekt Plan", FileNameHelper.ToSafeFileName("Projekt:Plan"), "Windows-ungültige Zeichen müssen auf jedem Testsystem ersetzt werden.");

        // Auch Gerätenamen mit Erweiterungen dürfen niemals unverändert zum Dateisystem gelangen.
        var reservedNames = new List<string> { "CON", "PRN", "AUX", "NUL" };
        for (int number = 1; number <= 9; number++)
        {
            reservedNames.Add($"COM{number}");
            reservedNames.Add($"LPT{number}");
        }

        foreach (string reservedName in reservedNames)
        {
            string lowerCaseName = reservedName.ToLowerInvariant();
            AssertEx.Equal(lowerCaseName + "_.md", FileNameHelper.ToSafeFileName(lowerCaseName + ".md"),
                "Reservierte Namen müssen unabhängig von der Großschreibung vor der Erweiterung entschärft werden.");
        }

        AssertEx.Equal("COM10.md", FileNameHelper.ToSafeFileName("COM10.md"), "Nicht reservierte ähnliche Namen sollen erhalten bleiben.");
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

        TextSelectionTransformResult strike = formattingService.Apply(
            "Text",
            0,
            4,
            MarkdownFormatKind.Strikethrough);
        AssertEx.Equal("~~Text~~", strike.UpdatedText, "Durchstreichen sollte den Text mit Markdown-Tilden umschließen.");

        TextSelectionTransformResult quote = formattingService.Apply(
            "Zitat",
            0,
            5,
            MarkdownFormatKind.Quote);
        AssertEx.Equal("> Zitat", quote.UpdatedText, "Zitat-Formatierung sollte einen Markdown-Zeilenpräfix setzen.");

        TextSelectionTransformResult numbered = formattingService.Apply(
            "Erster\nZweiter",
            0,
            14,
            MarkdownFormatKind.NumberedList);
        AssertEx.Equal("1. Erster\n2. Zweiter", numbered.UpdatedText, "Nummerierte Listen sollten fortlaufend nummeriert werden.");

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

    /// <summary>
    /// Prüft exklusive Auswahlenden und den unveränderten Erhalt angrenzender Zeilenumbrüche.
    /// </summary>
    private static void RunFormatterLineBoundaryTests()
    {
        var formatter = new EditorFormattingService();

        TextSelectionTransformResult insideCrlf = formatter.Apply(
            "Erste\r\nZweite", 6, 0, MarkdownFormatKind.Quote);
        AssertEx.Equal("> Erste\r\nZweite", insideCrlf.UpdatedText,
            "Ein Cursor zwischen CR und LF darf den Zeilenumbruch nicht aufteilen.");

        foreach (string newline in new[] { "\n", "\r\n" })
        {
            string text = "Erste" + newline + "Zweite" + newline + "Dritte";
            int secondLineStart = "Erste".Length + newline.Length;

            TextSelectionTransformResult firstLine = formatter.Apply(
                text, 0, secondLineStart, MarkdownFormatKind.BulletList);
            AssertEx.Equal("- Erste" + newline + "Zweite" + newline + "Dritte", firstLine.UpdatedText,
                "Eine Auswahl bis zum nächsten Zeilenanfang darf die nächste Zeile nicht formatieren.");
            AssertEx.Equal(0, firstLine.SelectionStart, "Die Auswahl soll am formatierten Zeilenanfang beginnen.");
            AssertEx.Equal(7, firstLine.SelectionLength, "Die Auswahl soll den unveränderten folgenden Zeilenumbruch ausschließen.");

            TextSelectionTransformResult caret = formatter.Apply(
                text, 2, 0, MarkdownFormatKind.Quote);
            AssertEx.Equal("> Erste" + newline + "Zweite" + newline + "Dritte", caret.UpdatedText,
                "Formatieren ohne Auswahl darf keinen zusätzlichen Zeilenumbruch erzeugen.");

            TextSelectionTransformResult secondLine = formatter.Apply(
                text, secondLineStart, 3, MarkdownFormatKind.Heading2);
            AssertEx.Equal("Erste" + newline + "## Zweite" + newline + "Dritte", secondLine.UpdatedText,
                "Nur die ausgewählte mittlere Zeile soll formatiert werden.");

            string terminatedText = "Erste" + newline + "Zweite" + newline;
            TextSelectionTransformResult numbered = formatter.Apply(
                terminatedText, 0, terminatedText.Length, MarkdownFormatKind.NumberedList);
            AssertEx.Equal("1. Erste" + newline + "2. Zweite" + newline, numbered.UpdatedText,
                "Nummerierte Listen müssen den abschließenden Zeilenumbruch exakt erhalten.");
        }
    }

    private static void RunDisplayFormattingTests()
    {
        var searchHit = new SearchHit
        {
            NoteTitle = "Projektplan",
            RelativePath = "Projekte/Projektplan.md",
            PreviewText = "MVP",
            Score = 100,
            MatchIndex = 0
        };
        AssertEx.True(searchHit.ToString().Contains("Projekte/Projektplan.md", StringComparison.Ordinal), "Suchtreffer sollten den relativen Pfad anzeigen.");

        var backlink = new BacklinkInfo
        {
            SourceTitle = "Quelle",
            SourceRelativePath = "Inbox/Quelle.md",
            ContextSnippet = "[[Projektplan]]"
        };
        AssertEx.True(backlink.ToString().Contains("Inbox/Quelle.md", StringComparison.Ordinal), "Backlinks sollten den relativen Quellpfad anzeigen.");
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
            Directory.CreateDirectory(Path.Combine(temporaryVault, "Projekte"));
            Directory.CreateDirectory(Path.Combine(temporaryVault, "Archiv"));
            await File.WriteAllTextAsync(Path.Combine(temporaryVault, "Projekte", "Plan.md"), "# Projektplan");
            await File.WriteAllTextAsync(Path.Combine(temporaryVault, "Archiv", "Plan.md"), "# Archivplan");

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

            // Navigation, Info-Bereich und Backlinks müssen beim selben aktuellen Editorinhalt übereinstimmen.
            const string linkedContent = "[[Projekte/Plan.md|Projektplan]] [[Fehlt/Plan]] [[Plan]]";
            workspace.UpdateWorkingContent(sourceNote, linkedContent);
            NoteDocument projectPlan = workspace.LoadedNotes.Single(note => note.RelativePath.Replace('\\', '/') == "Projekte/Plan.md");
            NoteDocument archivePlan = workspace.LoadedNotes.Single(note => note.RelativePath.Replace('\\', '/') == "Archiv/Plan.md");
            AssertEx.True(ReferenceEquals(projectPlan, workspace.TryResolveLink(sourceNote, 2, null)),
                "Die Navigation muss den expliziten Pfad mit Endung auflösen.");
            AssertEx.True(workspace.TryResolveLink(sourceNote, linkedContent.IndexOf("[[Fehlt", StringComparison.Ordinal) + 2, null) is null,
                "Die Navigation darf einen falschen Ordner nicht ignorieren.");
            AssertEx.True(workspace.TryResolveLink(sourceNote, linkedContent.IndexOf("[[Plan]]", StringComparison.Ordinal), "[[Plan]]") is null,
                "Auch ein markierter mehrdeutiger Kurzlink darf nicht beliebig navigieren.");
            IReadOnlyList<WikiLink> pathUnresolved = workspace.GetUnresolvedLinks(sourceNote);
            AssertEx.Equal(2, pathUnresolved.Count, "Fehlender Pfad und mehrdeutiger Kurzlink müssen beide unaufgelöst bleiben.");
            AssertEx.Equal("Fehlt/Plan", pathUnresolved[0].Target, "Der fehlende Pfad muss im Info-Bereich erhalten bleiben.");
            AssertEx.Equal("Plan", pathUnresolved[1].Target, "Der mehrdeutige Kurzlink muss im Info-Bereich erhalten bleiben.");
            AssertEx.Equal(1, workspace.GetBacklinks(projectPlan).Count, "Nur der explizite Pfad darf den Projekt-Backlink liefern.");
            AssertEx.Equal(0, workspace.GetBacklinks(archivePlan).Count, "Die Archivnotiz darf keine falschen Backlinks erhalten.");

            // Derselbe Parserfilter muss auch auf dem aktuellen, noch ungespeicherten Editorinhalt wirken.
            const string codeExample = "```markdown\n# Beispiel\n[[Fehlt]] [[Projekte/Plan.md]]\n```\n# Echt\n[[Vorhanden]]";
            workspace.UpdateWorkingContent(sourceNote, codeExample);
            AssertEx.Equal(0, workspace.GetUnresolvedLinks(sourceNote).Count,
                "Nicht existente Ziele in Codebeispielen dürfen keine Warnungen auslösen.");
            AssertEx.True(workspace.TryResolveLink(sourceNote, codeExample.IndexOf("[[Projekte", StringComparison.Ordinal) + 2, null) is null,
                "Die Cursornavigation darf einen Wiki-Link innerhalb eines Codeblocks nicht öffnen.");
            AssertEx.Equal(0, workspace.GetBacklinks(projectPlan).Count, "Codebeispiele dürfen keine Workspace-Backlinks erzeugen.");
            AssertEx.True(workspace.TryResolveLink(sourceNote, codeExample.IndexOf("[[Projekte", StringComparison.Ordinal), "[[Projekte/Plan.md]]") is null,
                "Auch eine vollständige Auswahl im Codeblock darf den Dokumentkontext nicht umgehen.");
            NoteDocument existingNote = workspace.LoadedNotes.Single(note => note.Title == "Vorhanden");
            AssertEx.True(ReferenceEquals(existingNote, workspace.TryResolveLink(
                sourceNote, codeExample.IndexOf("[[Vorhanden]]", StringComparison.Ordinal), "[[Vorhanden]]")),
                "Die vollständige Auswahl eines echten Links muss weiterhin navigierbar bleiben.");
            AssertEx.Equal(1, workspace.GetOutline(sourceNote).Count, "Der Workspace darf nur die echte Überschrift anzeigen.");
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

    private static async Task RunFolderTreeTests()
    {
        string temporaryVault = Path.Combine(Path.GetTempPath(), "sasd-notes-folder-tree-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(temporaryVault, "Thema"));

        try
        {
            var parser = new WikiLinkParser();
            var resolver = new LinkResolver();
            var workspace = new NotesWorkspaceService(
                new FileSystemVaultRepository(parser),
                new InMemorySettingsRepository(),
                parser,
                new MarkdownOutlineParser(),
                resolver,
                new SearchService(),
                new BacklinkService(resolver));

            await workspace.InitializeAsync();
            await workspace.OpenVaultAsync(temporaryVault);

            VaultTreeItem? emptyFolder = workspace.BuildTreeItems()
                .FirstOrDefault(item => item.ItemType == TreeItemType.Folder && item.RelativePath == "Thema");
            AssertEx.True(emptyFolder is not null, "Leere Vault-Ordner sollten im fachlichen Baum enthalten sein.");

            string nestedFolder = await workspace.CreateFolderAsync("Unterordner", "Thema");
            AssertEx.Equal("Thema/Unterordner", nestedFolder, "Der Workspace sollte verschachtelte Ordner korrekt anlegen.");

            VaultTreeItem? nestedItem = workspace.BuildTreeItems()
                .FirstOrDefault(item => item.ItemType == TreeItemType.Folder && item.RelativePath == nestedFolder);
            AssertEx.True(nestedItem is not null, "Neu angelegte Ordner sollten ohne vollständiges Neuladen im Baum erscheinen.");
        }
        finally
        {
            // Der Test-Vault wird nach Abschluss vollständig entfernt.
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

    private static async Task RunReloadNoteTests()
    {
        string temporaryVault = Path.Combine(Path.GetTempPath(), "sasd-notes-reload-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryVault);
        string notePath = Path.Combine(temporaryVault, "Notiz.md");

        try
        {
            await File.WriteAllTextAsync(notePath, "# Original");

            var parser = new WikiLinkParser();
            var resolver = new LinkResolver();
            var workspace = new NotesWorkspaceService(
                new FileSystemVaultRepository(parser),
                new InMemorySettingsRepository(),
                parser,
                new MarkdownOutlineParser(),
                resolver,
                new SearchService(),
                new BacklinkService(resolver));

            await workspace.InitializeAsync();
            await workspace.OpenVaultAsync(temporaryVault);
            NoteDocument loadedNote = workspace.LoadedNotes.Single();

            await File.WriteAllTextAsync(notePath, "# Extern geändert");
            NoteDocument reloadedNote = await workspace.ReloadNoteAsync(loadedNote);

            AssertEx.Equal("# Extern geändert", reloadedNote.Content, "Das Neuladen sollte den aktuellen Dateistand übernehmen.");
            AssertEx.False(reloadedNote.IsDirty, "Eine neu geladene Notiz darf nicht als ungespeichert markiert sein.");
            AssertEx.Equal("# Extern geändert", workspace.LoadedNotes.Single().Content, "Der Workspace sollte den geladenen Stand ersetzen.");
        }
        finally
        {
            // Der Test-Vault wird nach Abschluss vollständig entfernt.
            if (Directory.Exists(temporaryVault))
            {
                Directory.Delete(temporaryVault, recursive: true);
            }
        }
    }

    /// <summary>
    /// Prüft Namenskollisionen und Abbruch an realen Dateien, ohne auf Zeitgeber oder Sleeps angewiesen zu sein.
    /// </summary>
    private static async Task RunConcurrentNoteCreationTests()
    {
        string temporaryVault = Path.Combine(Path.GetTempPath(), "sasd-notes-create-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryVault);

        try
        {
            var repository = new FileSystemVaultRepository(new WikiLinkParser());
            string originalPath = Path.Combine(temporaryVault, "Gemeinsam.md");
            const string originalContent = "Unveränderte ursprüngliche Notiz – 😀";
            await File.WriteAllTextAsync(originalPath, originalContent);

            // Mehrere Aufträge konkurrieren um dieselben freien Kandidaten und müssen sich jeweils eine eigene Datei sichern.
            Task<NoteDocument>[] requests = Enumerable.Range(0, 12)
                .Select(_ => repository.CreateNoteAsync(temporaryVault, null, "Gemeinsam"))
                .ToArray();
            NoteDocument[] createdNotes = await Task.WhenAll(requests);

            AssertEx.Equal(12, createdNotes.Select(note => note.FullPath).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                "Gleichzeitige Neuerzeugungen dürfen niemals denselben Zielpfad zurückgeben.");
            AssertEx.Equal(originalContent, await File.ReadAllTextAsync(originalPath),
                "Die bereits vorhandene Notiz muss nach konkurrierenden Neuerzeugungen unverändert sein.");

            foreach (NoteDocument note in createdNotes)
            {
                AssertEx.Equal(note.Content, await File.ReadAllTextAsync(note.FullPath),
                    "Jede erzeugte Notiz muss unter ihrem eigenen Pfad vollständig erhalten bleiben.");
                AssertEx.Equal(Path.GetFileNameWithoutExtension(note.FullPath), note.Title,
                    "Der Titel muss zum tatsächlich gesicherten Dateinamen gehören.");
            }

            AssertEx.Equal(13, Directory.GetFiles(temporaryVault, "*.md").Length,
                "Für jeden erfolgreichen Auftrag muss genau eine Datei bestehen.");

            // Auch ein Verzeichnis mit Markdown-Endung belegt den Namen und darf nicht überschrieben werden.
            Directory.CreateDirectory(Path.Combine(temporaryVault, "Ordner.md"));
            NoteDocument folderCollision = await repository.CreateNoteAsync(temporaryVault, null, "Ordner");
            AssertEx.Equal("Ordner 2.md", Path.GetFileName(folderCollision.FullPath),
                "Ein vorhandener Ordner muss bei der Namensvergabe berücksichtigt werden.");
            AssertEx.True(Directory.Exists(Path.Combine(temporaryVault, "Ordner.md")),
                "Das kollidierende Verzeichnis muss unverändert erhalten bleiben.");

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            bool cancelled = false;
            try
            {
                await repository.CreateNoteAsync(temporaryVault, "NichtAnlegen", "Abgebrochen", cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            AssertEx.True(cancelled, "Ein abgebrochener Erstellungsauftrag muss als Abbruch gemeldet werden.");
            AssertEx.False(Directory.Exists(Path.Combine(temporaryVault, "NichtAnlegen")),
                "Ein bereits abgebrochener Auftrag darf keinen Zielordner erzeugen.");
            AssertEx.Equal(0, Directory.GetFiles(temporaryVault, "*.sasd-tmp-*", SearchOption.AllDirectories).Length,
                "Nach Kollisionen und Abbrüchen dürfen keine temporären Notizdateien zurückbleiben.");
        }
        finally
        {
            // Nur der eigens angelegte Test-Vault wird entfernt; produktive Benutzerdateien bleiben unberührt.
            Directory.Delete(temporaryVault, recursive: true);
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

            string createdFolder = await repository.CreateFolderAsync(temporaryVault, null, "Projekte");
            AssertEx.Equal("Projekte", createdFolder, "Ein neuer Unterordner sollte relativ zum Vault zurückgegeben werden.");
            AssertEx.True(Directory.Exists(Path.Combine(temporaryVault, "Projekte")), "Der neue Unterordner sollte im Vault angelegt werden.");

            string nestedFolder = await repository.CreateFolderAsync(temporaryVault, "Projekte", "Archiv");
            AssertEx.Equal("Projekte/Archiv", nestedFolder, "Ein verschachtelter Unterordner sollte korrekt normalisiert werden.");

            bool duplicateFolderRejected = false;
            try
            {
                await repository.CreateFolderAsync(temporaryVault, null, "Projekte");
            }
            catch (IOException)
            {
                duplicateFolderRejected = true;
            }

            AssertEx.True(duplicateFolderRejected, "Ein bereits vorhandener Ordner darf nicht stillschweigend wiederverwendet werden.");

            bool traversalRejected = false;
            try
            {
                await repository.CreateFolderAsync(temporaryVault, "..", "Außerhalb");
            }
            catch (InvalidOperationException)
            {
                traversalRejected = true;
            }

            AssertEx.True(traversalRejected, "Ein übergeordneter Pfad außerhalb des Vaults muss abgelehnt werden.");

            IReadOnlyList<NoteDocument> notes = await repository.LoadAllNotesAsync(temporaryVault);
            AssertEx.Equal(1, notes.Count, "Das Repository sollte die Markdown-Datei laden.");

            NoteDocument note = notes[0];
            note.Content = "# Aktualisiert\n\nÄnderung 😀";
            await repository.SaveNoteAsync(temporaryVault, note);
            AssertEx.Equal(note.Content, await File.ReadAllTextAsync(notePath), "Der atomare Speichervorgang sollte den vollständigen Inhalt schreiben.");

            AssertEx.False(repository.HasChangedSinceLoaded(note), "Eine unveränderte gespeicherte Notiz sollte keinen Änderungsalarm auslösen.");

            NoteDocument createdNote = await repository.CreateNoteAsync(temporaryVault, null, "Neue Notiz");
            AssertEx.Equal("Neue Notiz", createdNote.Title, "Eine neue Notiz sollte den bereinigten Titel erhalten.");
            AssertEx.Equal(createdNote.Content, await File.ReadAllTextAsync(createdNote.FullPath), "Eine neue Notiz sollte vollständig geschrieben werden.");

            // Prüft den tatsächlich erzeugten Windows-Dateinamen einschließlich Erweiterung.
            NoteDocument deviceNamedNote = await repository.CreateNoteAsync(temporaryVault, null, "CON.md");
            AssertEx.Equal("CON_.md", Path.GetFileName(deviceNamedNote.FullPath),
                "Eine Notiz mit Gerätenamen und Erweiterung muss einen nutzbaren Dateinamen erhalten.");
            AssertEx.Equal(deviceNamedNote.Content, await File.ReadAllTextAsync(deviceNamedNote.FullPath),
                "Die bereinigte Notiz muss vollständig lesbar sein.");

            await File.WriteAllTextAsync(notePath, "# Extern geändert");
            File.SetLastWriteTimeUtc(notePath, note.LastModifiedUtc.AddSeconds(1));
            note.Content = "# Lokale Änderung";

            AssertEx.True(repository.HasChangedSinceLoaded(note), "Eine extern geänderte Notiz sollte erkannt werden.");

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

            File.Delete(notePath);
            AssertEx.True(repository.HasChangedSinceLoaded(note), "Eine gelöschte Notiz sollte als externe Änderung gelten.");
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

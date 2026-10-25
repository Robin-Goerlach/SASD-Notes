using System.Diagnostics;
using Sasd.Notes.App.WinForms;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Models;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;
using Sasd.Notes.Infrastructure.Services;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Paketfreie Windows-Regressionstests für asynchrones Schließen und Laden eines Vaults.
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            using var form = new CloseTestForm();
            form.Show();

            // Eine noch nicht abgeschlossene Speicherung darf das echte Fenster nicht schließen.
            form.Close();
            Require(!form.IsDisposed && form.Visible, "Das Fenster wurde vor Abschluss der Bestätigung geschlossen.");
            Require(!form.Enabled, "Während der Bestätigung muss die Bearbeitung gesperrt sein.");
            form.Close();
            Require(form.ConfirmationCount == 1, "Ein weiterer Schließversuch darf keine zweite Bestätigung starten.");

            // Abbrechen oder fehlgeschlagenes Speichern liefert false und hält das Fenster offen.
            form.PendingConfirmation.SetResult(false);
            PumpUntil(() => form.Enabled);
            Require(!form.IsDisposed && form.Visible, "Nach abgelehnter Bestätigung muss das Fenster offen bleiben.");

            form.PendingConfirmation = new TaskCompletionSource<bool>();
            form.Close();
            Require(form.ConfirmationCount == 2, "Ein erneuter Versuch muss nach einem Abbruch möglich sein.");
            Require(!form.IsDisposed, "Auch beim zweiten Versuch muss das Fenster auf den Abschluss warten.");

            form.PendingConfirmation.SetResult(true);
            PumpUntil(() => form.IsDisposed);
            Require(form.ConfirmationCount == 2, "Der genehmigte zweite Schließvorgang darf nicht erneut nachfragen.");

            RunVaultLoadingTest();
            RunStartupTargetTests();
            RunTextPositionMappingTest();
            RunDelayedSettingsStartupTest();
            RunExternalChangeTimerTest();
            ResearchHighlightScriptBuilderTests.Run();
            ResearchOutlineScriptBuilderTests.Run();
            Console.WriteLine("Alle WinForms-Schließ-, Vault-Lade-, Startziel- und Rechercheprüfungen wurden erfolgreich ausgeführt.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    /// <summary>
    /// Prüft mit einem echten Fenster, dass ein offener Vault-Ladevorgang weder Bearbeitung noch Schließen zulässt.
    /// </summary>
    private static void RunVaultLoadingTest()
    {
        string vaultPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-winforms-loading"));
        var repository = new LoadingVaultRepository();
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            repository, new InMemorySettingsRepository(vaultPath), parser,
            new MarkdownOutlineParser(), resolver, new SearchService(), new BacklinkService(resolver));
        using var form = new VaultLoadTestForm(workspace);
        form.Show();

        // OnLoad führt den produktiven automatischen Vault-Start aus; nur der Infrastrukturabschluss bleibt offen.
        Require(repository.LoadStarted, "Der reale Fensterstart muss den konfigurierten Vault laden.");
        Require(!form.Enabled && form.UseWaitCursor, "Während des Ladens müssen Bedienung und Warteanzeige gesperrt sein.");
        form.Close();
        Require(!form.IsDisposed && form.Visible, "Das Fenster darf vor dem Ladeabschluss nicht freigegeben werden.");
        Require(form.ConfirmationCount == 1, "Ein Schließversuch während des Ladens darf keine weitere Speicherabfrage starten.");

        repository.Completion.SetResult(Array.Empty<NoteDocument>());
        PumpUntil(() => form.Enabled);
        Require(workspace.CurrentVault?.RootPath == vaultPath, "Nach dem Ladeabschluss muss der richtige Vault aktiv sein.");
        Require(!form.UseWaitCursor, "Die Warteanzeige muss nach dem Laden wieder aufgehoben sein.");

        // Nach dem Übergang muss der normale, bereits separat geprüfte Schließablauf wieder möglich sein.
        form.Close();
        PumpUntil(() => form.IsDisposed);
        Require(form.ConfirmationCount == 2, "Nach dem Laden muss Schließen wieder über die normale Bestätigung laufen.");
    }

    /// <summary>
    /// Prüft Ordner-, Datei- und Standardstart mit realen Markdown-Dateien und dem produktiven Fenster-Ladeablauf.
    /// </summary>
    private static void RunStartupTargetTests()
    {
        string temporaryRoot = Path.Combine(Path.GetTempPath(), "sasd-winforms-startup-" + Guid.NewGuid().ToString("N"));
        string requestedVault = Path.Combine(temporaryRoot, "Startordner Ω");
        string rememberedVault = Path.Combine(temporaryRoot, "Gemerkter Vault");
        Directory.CreateDirectory(requestedVault);
        Directory.CreateDirectory(rememberedVault);
        string notePath = Path.Combine(requestedVault, "Start Notiz 😀.md");
        const string expectedContent = "# Gewählte Startnotiz 😀";
        try
        {
            File.WriteAllText(notePath, expectedContent);
            File.WriteAllText(Path.Combine(rememberedVault, "Alt.md"), "# Andere Notiz");

            // Dieselbe Argument- und Zielauflösung wie im Composition Root; nur die Einstellungen sind im Speicher.
            RunStartupCase(Array.Empty<string>(), temporaryRoot, rememberedVault, rememberedVault, null, null);
            RunStartupCase(new[] { Path.GetFileName(requestedVault) }, temporaryRoot, rememberedVault, requestedVault, null, null);
            RunStartupCase(new[] { Path.GetRelativePath(temporaryRoot, notePath) }, temporaryRoot,
                rememberedVault, requestedVault, notePath, expectedContent);
            Require(File.ReadAllText(notePath) == expectedContent, "Ein Dateistart darf den Notizinhalt nicht verändern.");
        }
        finally
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    /// <summary>
    /// Prüft einen Startfall einschließlich TreeView-Auswahl und Editorinhalt, ohne private Felder auszulesen.
    /// </summary>
    private static void RunStartupCase(
        IReadOnlyList<string> arguments,
        string workingDirectory,
        string rememberedVault,
        string expectedVault,
        string? expectedNotePath,
        string? expectedContent)
    {
        string? parsedPath = StartupArgumentsParser.Parse(arguments, workingDirectory);
        StartupTarget? startupTarget = parsedPath is null ? null : new FileSystemStartupTargetResolver().Resolve(parsedPath);
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            new FileSystemVaultRepository(parser), new InMemorySettingsRepository(rememberedVault),
            parser, new MarkdownOutlineParser(), resolver, new SearchService(), new BacklinkService(resolver));
        using var form = new VaultLoadTestForm(workspace, startupTarget);
        RichTextBox editor = form.Controls.Find("richTextBoxEditor", searchAllChildren: true).OfType<RichTextBox>().Single();
        TreeView tree = form.Controls.Find("treeViewVault", searchAllChildren: true).OfType<TreeView>().Single();
        form.Show();

        PumpUntil(() => form.Enabled && workspace.CurrentVault?.RootPath == expectedVault &&
            (expectedNotePath is null || (tree.SelectedNode?.Tag as VaultTreeItem)?.FullPath == expectedNotePath));
        Require(workspace.CurrentVault?.RootPath == expectedVault, "Der Programmstart muss den erwarteten Vault aktivieren.");
        Require(workspace.GetLastOpenedVaultPath() == expectedVault, "Der erfolgreich geöffnete Start-Vault muss als zuletzt verwendet gelten.");
        if (expectedNotePath is null)
        {
            Require(editor.TextLength == 0, "Ein Ordnerstart darf keine beliebige Notiz automatisch öffnen.");
        }
        else
        {
            Require(editor.Text == expectedContent, "Beim Dateistart muss die angeforderte Notiz im Editor stehen.");
            Require((tree.SelectedNode?.Tag as VaultTreeItem)?.FullPath == expectedNotePath,
                "Beim Dateistart muss der richtige Dateiknoten im Baum ausgewählt sein.");
            Require(workspace.FindNoteByFullPath(expectedNotePath)?.IsDirty == false,
                "Eine nur beim Start geöffnete Datei darf nicht dirty werden.");
        }

        form.Close();
        PumpUntil(() => form.IsDisposed);
    }

    /// <summary>
    /// Verhindert Schließen bereits während der Einstellungen-Initialisierung, noch vor einem möglichen Vault-Scan.
    /// </summary>
    private static void RunDelayedSettingsStartupTest()
    {
        var settings = new DelayedStartupSettingsRepository();
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            new FileSystemVaultRepository(parser), settings, parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
        using var form = new VaultLoadTestForm(workspace);
        form.Show();
        Require(!form.Enabled, "Während der Einstellungen-Initialisierung muss die Bedienung gesperrt sein.");
        form.Close();
        Require(!form.IsDisposed && form.Visible, "Das Fenster darf nicht vor dem asynchronen Startabschluss geschlossen werden.");

        settings.Completion.SetResult(new AppSettings());
        PumpUntil(() => form.Enabled);
        Require(workspace.CurrentVault is null, "Ohne Startziel und gemerkten Vault muss die Anwendung leer starten.");
        Require(!form.UseWaitCursor, "Nach dem Programmstart muss der Wartecursor zurückgesetzt sein.");
        form.Close();
        PumpUntil(() => form.IsDisposed);
    }

    /// <summary>
    /// Prüft die Zuordnung mit dem tatsächlich von RichTextBox zurückgegebenen Text.
    /// </summary>
    private static void RunTextPositionMappingTest()
    {
        foreach (string newline in new[] { "\r\n", "\n" })
        {
            string source = "Vorher 😀" + newline + "# Titel" + newline + "[[Ziel]]";
            using var editor = new RichTextBox();
            editor.CreateControl();
            editor.Text = source;
            int linkStart = source.IndexOf("[[Ziel]]", StringComparison.Ordinal);
            var range = EditorTextPositionMapper.Map(source, editor.Text, linkStart, 8);
            Require(range.HasValue, "Der Originaltext muss auf die tatsächliche RichTextBox-Darstellung abbildbar sein.");
            editor.Select(range.GetValueOrDefault().Start, range.GetValueOrDefault().Length);
            Require(editor.SelectedText == "[[Ziel]]", "Die RichTextBox muss nach der Zuordnung exakt den Link markieren.");
            var reverse = EditorTextPositionMapper.Map(editor.Text, source, editor.SelectionStart, editor.SelectionLength);
            Require(reverse.HasValue && reverse.Value.Start == linkStart && reverse.Value.Length == 8,
                "Die Editorauswahl muss auf den vollständigen Original-Link zurückführen.");
        }
    }

    /// <summary>
    /// Prüft den echten Windows-Timer mit Zugriffsfehler, erneuter Prüfung und anschließender Erholung.
    /// </summary>
    private static void RunExternalChangeTimerTest()
    {
        string vaultPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-timer-tests"));
        var note = new NoteDocument
        {
            Id = "Notiz", Title = "Notiz", RelativePath = "Notiz.md",
            FullPath = Path.Combine(vaultPath, "Notiz.md"), Content = "Entwurf 😀"
        };
        var repository = new LoadingVaultRepository { CheckError = new IOException("Simulierter Zugriffsfehler") };
        repository.Completion.SetResult(new[] { note });
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            repository, new InMemorySettingsRepository(), parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
        using var form = new VaultLoadTestForm(workspace, new StartupTarget { VaultPath = vaultPath, NotePath = note.FullPath });
        form.Show();
        PumpUntil(() => form.Enabled);
        RichTextBox editor = form.Controls.Find("richTextBoxEditor", true).OfType<RichTextBox>().Single();
        StatusStrip status = form.Controls.OfType<StatusStrip>().Single();

        bool HasStatus(string text) => status.Items.OfType<ToolStripStatusLabel>()
            .Any(item => item.Text?.Contains(text, StringComparison.Ordinal) == true);

        // Das produktive Tick-Ereignis läuft über die Windows-Nachrichtenpumpe, ohne private Handler aufzurufen.
        PumpUntil(() => repository.CheckCount >= 2 && HasStatus("derzeit nicht prüfbar"));
        Require(form.Visible && editor.Text == note.Content && !note.IsDirty,
            "Ein Dateiprüffehler darf das Fenster nicht schließen oder den Editor verändern.");
        repository.CheckError = null;
        PumpUntil(() => HasStatus("Dateiprüfung wieder möglich"));
        repository.ExternalChange = true;
        PumpUntil(() => HasStatus("extern geändert oder gelöscht"));
        Require(editor.Text == "Entwurf 😀", "Auch die Änderungswarnung darf keine Datei automatisch neu laden.");
        form.Close();
        PumpUntil(() => form.IsDisposed);
    }

    /// <summary>
    /// Verarbeitet Windows-Nachrichten bis zur erwarteten Zustandsänderung, mit begrenzter Laufzeit.
    /// </summary>
    private static void PumpUntil(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(10))
            {
                throw new TimeoutException("Die erwartete Fensterzustandsänderung ist ausgeblieben.");
            }

            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Ersetzt nur die Benutzerentscheidung; der reale FormClosing-Ablauf bleibt unverändert.
    /// </summary>
    private sealed class CloseTestForm : MainForm
    {
        public CloseTestForm()
            : base(CreateWorkspace(), new EditorFormattingService())
        {
            ShowInTaskbar = false;
        }

        public TaskCompletionSource<bool> PendingConfirmation { get; set; } = new();

        public int ConfirmationCount { get; private set; }

        protected override Task<bool> ConfirmSaveChangesIfRequiredAsync()
        {
            ConfirmationCount++;
            return PendingConfirmation.Task;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Kein automatischer Vault-Start: Der Test darf keine lokalen Benutzereinstellungen lesen.
        }

        private static NotesWorkspaceService CreateWorkspace()
        {
            var parser = new WikiLinkParser();
            var resolver = new LinkResolver();
            return new NotesWorkspaceService(
                new FileSystemVaultRepository(parser),
                new InMemorySettingsRepository(),
                parser,
                new MarkdownOutlineParser(),
                resolver,
                new SearchService(),
                new BacklinkService(resolver));
        }
    }

    /// <summary>
    /// Ersetzt die Benutzerentscheidung, behält aber den realen Load- und Closing-Ablauf des Hauptfensters.
    /// </summary>
    private sealed class VaultLoadTestForm : MainForm
    {
        /// <summary>Initialisiert das Fenster mit dem kontrollierten Test-Workspace.</summary>
        /// <param name="workspace">Workspace mit realer oder kontrollierter Infrastruktur.</param>
        /// <param name="startupTarget">Optionales explizites Startziel.</param>
        public VaultLoadTestForm(NotesWorkspaceService workspace, StartupTarget? startupTarget = null)
            : base(workspace, new EditorFormattingService(), startupTarget)
        {
            ShowInTaskbar = false;
        }

        /// <summary>Anzahl angeforderter Speicherbestätigungen.</summary>
        public int ConfirmationCount { get; private set; }

        /// <inheritdoc />
        protected override Task<bool> ConfirmSaveChangesIfRequiredAsync()
        {
            ConfirmationCount++;
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Simuliert einen noch nicht abgeschlossenen Vault-Scan ohne Zugriff auf Benutzerdateien.
    /// </summary>
    private sealed class LoadingVaultRepository : IVaultRepository
    {
        /// <summary>Kontrollierter Abschluss des Scans.</summary>
        public TaskCompletionSource<IReadOnlyList<NoteDocument>> Completion { get; } = new();
        /// <summary>Ob der produktive Ladeablauf den Scan bereits angefordert hat.</summary>
        public bool LoadStarted { get; private set; }

        /// <inheritdoc />
        public bool VaultExists(string vaultPath) => true;
        /// <inheritdoc />
        public bool HasChangedSinceLoaded(NoteDocument note)
        {
            CheckCount++;
            if (CheckError is not null)
            {
                throw CheckError;
            }
            return ExternalChange;
        }
        /// <summary>Kontrollierter Dateizugriffsfehler.</summary>
        public Exception? CheckError { get; set; }
        /// <summary>Kontrollierte externe Änderung.</summary>
        public bool ExternalChange { get; set; }
        /// <summary>Anzahl produktiv ausgelöster Dateiprüfungen.</summary>
        public int CheckCount { get; private set; }
        /// <inheritdoc />
        public IReadOnlyList<string> LoadFolderPaths(string vaultPath, CancellationToken cancellationToken = default) => Array.Empty<string>();
        /// <inheritdoc />
        public Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
        {
            LoadStarted = true;
            return Completion.Task;
        }

        /// <inheritdoc />
        public Task<NoteDocument> LoadNoteAsync(string vaultPath, string fullNotePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        /// <inheritdoc />
        public Task SaveNoteAsync(string vaultPath, NoteDocument note, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        /// <inheritdoc />
        public Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        /// <inheritdoc />
        public Task<string> CreateFolderAsync(string vaultPath, string? relativeParentPath, string folderName, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    /// <summary>
    /// Verzögert ausschließlich das Lesen der Starteinstellungen, ohne Benutzerdaten anzufassen.
    /// </summary>
    private sealed class DelayedStartupSettingsRepository : ISettingsRepository
    {
        /// <summary>Kontrollierter Abschluss der Initialisierung.</summary>
        public TaskCompletionSource<AppSettings> Completion { get; } = new();
        /// <inheritdoc />
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Completion.Task;
        /// <inheritdoc />
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class InMemorySettingsRepository : ISettingsRepository
    {
        private readonly string? _lastOpenedVaultPath;

        /// <summary>Initialisiert ausschließlich im Arbeitsspeicher gehaltene Testeinstellungen.</summary>
        public InMemorySettingsRepository(string? lastOpenedVaultPath = null)
        {
            _lastOpenedVaultPath = lastOpenedVaultPath;
        }

        /// <inheritdoc />
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppSettings { LastOpenedVaultPath = _lastOpenedVaultPath });
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

using System.Diagnostics;
using Sasd.Notes.App.WinForms;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;

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
            Console.WriteLine("Alle WinForms-Schließ- und Vault-Ladeprüfungen wurden erfolgreich ausgeführt.");
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
        public VaultLoadTestForm(NotesWorkspaceService workspace)
            : base(workspace, new EditorFormattingService())
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
        public bool HasChangedSinceLoaded(NoteDocument note) => false;
        /// <inheritdoc />
        public IReadOnlyList<string> LoadFolderPaths(string vaultPath) => Array.Empty<string>();
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

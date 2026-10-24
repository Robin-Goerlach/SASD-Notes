using Sasd.Notes.Application.Models;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft sich überschneidende Workspace-Befehle mit kontrolliert offenen Repository-Aufträgen.
/// </summary>
internal static class WorkspaceTransitionTests
{
    /// <summary>
    /// Prüft exklusive Vault-Übergänge und die Freigabe nach Erfolg, Fehler und Abbruch.
    /// </summary>
    public static async Task RunAsync()
    {
        var repository = new ControlledVaultRepository();
        var settings = new ControlledSettingsRepository();
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            repository, settings, parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
        string firstPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-transition-first"));
        string nextPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-transition-next"));
        await workspace.OpenVaultAsync(firstPath);
        NoteDocument oldNote = workspace.LoadedNotes.Single();
        RunExternalChangeChecks(workspace, repository, oldNote);
        workspace.UpdateWorkingContent(oldNote, "Lokale Änderung 😀");

        repository.PauseLoad = true;
        Task<VaultInfo> opening = workspace.OpenVaultAsync(nextPath);
        await AssertBlockedDuringOpenAsync(workspace, repository, oldNote, firstPath);
        repository.LoadCompletion.SetException(new IOException("Simulierter Lesefehler."));
        await ExpectFailureAsync<IOException>(() => opening);
        Require(ReferenceEquals(oldNote, workspace.LoadedNotes.Single()), "Nach Lesefehlern muss das alte Modell aktiv bleiben.");
        workspace.UpdateWorkingContent(oldNote, "Nach Lesefehler");

        // Die Sperre muss über den Scan hinaus bis zum Abschluss der Einstellungsspeicherung reichen.
        repository.PauseLoad = false;
        settings.PauseSave = true;
        Task<VaultInfo> waitingForSettings = workspace.OpenVaultAsync(nextPath);
        await AssertBlockedDuringOpenAsync(workspace, repository, oldNote, firstPath);
        settings.SaveCompletion.SetException(new IOException("Simulierter Einstellungsfehler."));
        await ExpectFailureAsync<IOException>(() => waitingForSettings);
        Require(workspace.GetLastOpenedVaultPath() == firstPath, "Ein fehlgeschlagener Übergang darf Recent Folders nicht umstellen.");
        workspace.UpdateWorkingContent(oldNote, "Nach Einstellungsfehler");

        settings.PauseSave = false;
        repository.PauseLoad = true;
        using var cancellation = new CancellationTokenSource();
        Task<VaultInfo> cancelled = workspace.OpenVaultAsync(nextPath, cancellation.Token);
        cancellation.Cancel();
        repository.LoadCompletion.SetCanceled(cancellation.Token);
        await ExpectFailureAsync<OperationCanceledException>(() => cancelled);
        Require(workspace.TryDiscardWorkingChanges(oldNote) && oldNote.Content == "# Geladen",
            "Abbruch muss sowohl die Übergangssperre freigeben als auch die Verwerfen-Basis erhalten.");

        repository.PauseLoad = false;
        await workspace.OpenVaultAsync(nextPath);
        NoteDocument currentNote = workspace.LoadedNotes.Single();

        // Ein schon laufender Schreibauftrag darf nicht durch einen Vault-Wechsel überholt werden.
        Task saving = workspace.SaveNoteAsync(currentNote, "Gespeicherter Text");
        Require(workspace.CheckExternalChange(currentNote) == ExternalChangeStatus.NotChecked,
            "Eine eigene laufende Speicherung darf keine externe Warnung erzeugen.");
        int scansBefore = repository.ScanCount;
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.OpenVaultAsync(firstPath));
        Require(repository.ScanCount == scansBefore, "Bei laufendem Speichern darf kein neuer Scan beginnen.");
        repository.SaveCompletion.SetResult(true);
        await saving;
        await workspace.OpenVaultAsync(firstPath);

        // Zwei gleichzeitig offene Schreiboperationen benötigen eine Zählung, nicht nur ein einzelnes Busy-Flag.
        Task<NoteDocument> creating = workspace.CreateNoteAsync("Neu", null);
        Task<string> creatingFolder = workspace.CreateFolderAsync("Ordner", null);
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.OpenVaultAsync(nextPath));
        repository.CreateCompletion.SetResult(CreateNote(firstPath, "Neu.md"));
        await creating;
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.OpenVaultAsync(nextPath));
        repository.FolderCompletion.SetException(new IOException("Simulierter Ordnerfehler."));
        await ExpectFailureAsync<IOException>(() => creatingFolder);
        await workspace.OpenVaultAsync(nextPath);

        // Auch ein abgebrochener Schreibauftrag muss den nächsten Wechsel wieder ermöglichen.
        using var writeCancellation = new CancellationTokenSource();
        Task<NoteDocument> cancelledCreation = workspace.CreateNoteAsync("Abbruch", null, writeCancellation.Token);
        writeCancellation.Cancel();
        repository.CreateCompletion.SetCanceled(writeCancellation.Token);
        await ExpectFailureAsync<OperationCanceledException>(() => cancelledCreation);
        await workspace.OpenVaultAsync(firstPath);
        Require(workspace.CurrentVault!.RootPath == firstPath, "Nach allen Fehlerfällen muss ein normaler Wechsel weiterhin funktionieren.");
    }

    /// <summary>
    /// Prüft Abweisung vor jedem Repository-Schreibzugriff und Erhalt des aktiven Arbeitsstands.
    /// </summary>
    private static async Task AssertBlockedDuringOpenAsync(
        NotesWorkspaceService workspace,
        ControlledVaultRepository repository,
        NoteDocument oldNote,
        string expectedPath)
    {
        int scansBefore = repository.ScanCount;
        int writesBefore = repository.WriteCount;
        int checksBefore = repository.CheckCount;
        Require(workspace.CheckExternalChange(oldNote) == ExternalChangeStatus.NotChecked && repository.CheckCount == checksBefore,
            "Während eines Vault-Übergangs darf kein Dateivergleich starten.");
        string contentBefore = oldNote.Content;
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.OpenVaultAsync(expectedPath));
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.SaveNoteAsync(oldNote, "Nicht übernehmen"));
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.CreateNoteAsync("Nicht anlegen", null));
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.CreateFolderAsync("Nicht anlegen", null));
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.ReloadNoteAsync(oldNote));
        await ExpectFailureAsync<InvalidOperationException>(() =>
        {
            workspace.UpdateWorkingContent(oldNote, "Nicht übernehmen");
            return Task.CompletedTask;
        });
        Require(!workspace.TryDiscardWorkingChanges(oldNote), "Verwerfen darf nicht mit einem offenen Vault-Übergang konkurrieren.");
        Require(oldNote.Content == contentBefore && oldNote.IsDirty, "Abgewiesene Befehle müssen die lokalen Änderungen erhalten.");
        Require(ReferenceEquals(oldNote, workspace.LoadedNotes.Single()) && workspace.CurrentVault!.RootPath == expectedPath,
            "Während der Vorbereitung muss der alte Workspace aktiv bleiben.");
        Require(repository.ScanCount == scansBefore && repository.WriteCount == writesBefore,
            "Abgewiesene Befehle dürfen keine zusätzlichen Scans oder Schreibaufträge auslösen.");
    }

    /// <summary>
    /// Prüft Fehlerklassifizierung, Wiederholung und Erhalt lokaler Änderungen ohne echte Zugriffsrechte zu verändern.
    /// </summary>
    private static void RunExternalChangeChecks(
        NotesWorkspaceService workspace, ControlledVaultRepository repository, NoteDocument note)
    {
        workspace.UpdateWorkingContent(note, "Lokaler Entwurf 😀");
        string content = note.Content;
        DateTime timestamp = note.LastModifiedUtc;
        Require(workspace.CheckExternalChange(note) == ExternalChangeStatus.Unchanged, "Unverändert muss ausdrücklich erkennbar sein.");
        repository.ExternalChange = true;
        Require(workspace.CheckExternalChange(note) == ExternalChangeStatus.Changed, "Eine gemeldete Änderung muss erhalten bleiben.");

        foreach (Exception error in new Exception[] { new IOException("E/A"), new UnauthorizedAccessException("Zugriff") })
        {
            repository.CheckError = error;
            Require(workspace.CheckExternalChange(note) == ExternalChangeStatus.Unavailable,
                "Erwartete Dateizugriffsfehler dürfen nicht aus der periodischen Prüfung entweichen.");
            Require(note.Content == content && note.IsDirty && note.LastModifiedUtc == timestamp,
                "Die Prüfung muss Entwurf, Dirty-State und Ladezeitpunkt unverändert lassen.");
        }

        repository.CheckError = null;
        repository.ExternalChange = false;
        Require(workspace.CheckExternalChange(note) == ExternalChangeStatus.Unchanged,
            "Nach vorübergehenden Fehlern muss die nächste Prüfung wieder funktionieren.");
        int checksBefore = repository.CheckCount;
        Require(workspace.CheckExternalChange(CreateNote(Path.GetTempPath())) == ExternalChangeStatus.NotChecked
            && repository.CheckCount == checksBefore, "Ein fremdes Modell darf keinen Dateizugriff auslösen.");

        // Programmierfehler sind keine Dateizugriffsfehler und dürfen nicht still verschluckt werden.
        repository.CheckError = new InvalidOperationException("Programmierfehler");
        bool propagated = false;
        try
        {
            workspace.CheckExternalChange(note);
        }
        catch (InvalidOperationException)
        {
            propagated = true;
        }
        Require(propagated, "Unerwartete Fehler müssen für die Diagnose sichtbar bleiben.");
        repository.CheckError = null;
        Require(workspace.TryDiscardWorkingChanges(note) && note.Content == "# Geladen",
            "Die Prüfung darf auch die Verwerfen-Basis nicht verändern.");
    }

    private static NoteDocument CreateNote(string vaultPath, string name = "Notiz.md") => new()
    {
        Id = name, Title = name[..^3], RelativePath = name,
        FullPath = Path.Combine(vaultPath, name), Content = "# Geladen"
    };

    private static async Task ExpectFailureAsync<TException>(Func<Task> action) where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }
        throw new InvalidOperationException($"Erwartete Ausnahme fehlt: {typeof(TException).Name}");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Hält ausschließlich die jeweils zu prüfende Infrastrukturphase offen.
    /// </summary>
    private sealed class ControlledVaultRepository : IVaultRepository
    {
        /// <summary>Ob vollständige Scans auf einen Testabschluss warten.</summary>
        public bool PauseLoad { get; set; }
        /// <summary>Anzahl gestarteter Scans.</summary>
        public int ScanCount { get; private set; }
        /// <summary>Anzahl gestarteter Schreibaufträge.</summary>
        public int WriteCount { get; private set; }
        /// <summary>Kontrollierter Abschluss des Scans.</summary>
        public TaskCompletionSource<IReadOnlyList<NoteDocument>> LoadCompletion { get; private set; } = new();
        /// <summary>Kontrollierter Abschluss des Speicherns.</summary>
        public TaskCompletionSource<bool> SaveCompletion { get; private set; } = new();
        /// <summary>Kontrollierter Abschluss der Neuerzeugung.</summary>
        public TaskCompletionSource<NoteDocument> CreateCompletion { get; private set; } = new();
        /// <summary>Kontrollierter Abschluss der Ordnererzeugung.</summary>
        public TaskCompletionSource<string> FolderCompletion { get; private set; } = new();

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

        /// <summary>Für die Prüfung gemeldete externe Änderung.</summary>
        public bool ExternalChange { get; set; }
        /// <summary>Kontrollierter Fehler beim Dateivergleich.</summary>
        public Exception? CheckError { get; set; }
        /// <summary>Anzahl tatsächlich ausgeführter Dateivergleiche.</summary>
        public int CheckCount { get; private set; }
        /// <inheritdoc />
        public IReadOnlyList<string> LoadFolderPaths(string vaultPath) => Array.Empty<string>();

        /// <inheritdoc />
        public Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
        {
            ScanCount++;
            if (PauseLoad)
            {
                LoadCompletion = new TaskCompletionSource<IReadOnlyList<NoteDocument>>();
                return LoadCompletion.Task;
            }
            return Task.FromResult<IReadOnlyList<NoteDocument>>(new[] { CreateNote(vaultPath) });
        }

        /// <inheritdoc />
        public Task SaveNoteAsync(string vaultPath, NoteDocument note, CancellationToken cancellationToken = default)
        {
            WriteCount++;
            SaveCompletion = new TaskCompletionSource<bool>();
            return SaveCompletion.Task;
        }

        /// <inheritdoc />
        public Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default)
        {
            WriteCount++;
            CreateCompletion = new TaskCompletionSource<NoteDocument>();
            return CreateCompletion.Task;
        }

        /// <inheritdoc />
        public Task<string> CreateFolderAsync(string vaultPath, string? relativeParentPath, string folderName, CancellationToken cancellationToken = default)
        {
            WriteCount++;
            FolderCompletion = new TaskCompletionSource<string>();
            return FolderCompletion.Task;
        }

        /// <inheritdoc />
        public Task<NoteDocument> LoadNoteAsync(string vaultPath, string fullNotePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class ControlledSettingsRepository : ISettingsRepository
    {
        /// <summary>Ob die Einstellungsspeicherung auf den Testabschluss wartet.</summary>
        public bool PauseSave { get; set; }
        /// <summary>Kontrollierter Abschluss der Einstellungsspeicherung.</summary>
        public TaskCompletionSource<bool> SaveCompletion { get; private set; } = new();
        /// <inheritdoc />
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        /// <inheritdoc />
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            if (!PauseSave)
            {
                return Task.CompletedTask;
            }
            SaveCompletion = new TaskCompletionSource<bool>();
            return SaveCompletion.Task;
        }
    }
}

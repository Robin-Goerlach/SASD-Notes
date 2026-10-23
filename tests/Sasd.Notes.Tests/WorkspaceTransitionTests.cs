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
        public bool HasChangedSinceLoaded(NoteDocument note) => false;
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

using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft gezieltes Neuladen und den Zustandserhalt bei verspäteten oder fehlgeschlagenen Leseaufträgen.
/// </summary>
internal static class NoteReloadTests
{
    /// <summary>
    /// Führt Dateisystemprüfungen sowie kontrolliert verzögerte Application-Prüfungen aus.
    /// </summary>
    public static async Task RunAsync()
    {
        await RunFileSystemTestsAsync();
        await RunDelayedReloadTestsAsync();
    }

    private static NotesWorkspaceService CreateWorkspace(IVaultRepository repository)
    {
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        return new NotesWorkspaceService(
            repository, new MemorySettingsRepository(), parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
    }

    /// <summary>
    /// Eine gesperrte Nachbardatei darf das Lesen der gewünschten Notiz nicht verhindern.
    /// </summary>
    private static async Task RunFileSystemTestsAsync()
    {
        string temporaryRoot = Path.Combine(Path.GetTempPath(), "sasd-reload-" + Guid.NewGuid().ToString("N"));
        string vaultPath = Path.Combine(temporaryRoot, "Vault");
        Directory.CreateDirectory(vaultPath);
        string notePath = Path.Combine(vaultPath, "Ziel.md");
        string otherPath = Path.Combine(vaultPath, "Andere.md");
        string outsidePath = Path.Combine(temporaryRoot, "Ausserhalb.md");
        try
        {
            await File.WriteAllTextAsync(notePath, "# Original");
            await File.WriteAllTextAsync(otherPath, "# Andere");
            await File.WriteAllTextAsync(outsidePath, "# Privat");
            var repository = new FileSystemVaultRepository(new WikiLinkParser());
            NotesWorkspaceService workspace = CreateWorkspace(repository);
            await workspace.OpenVaultAsync(vaultPath);
            NoteDocument note = workspace.FindNoteByFullPath(notePath)!;
            NoteDocument other = workspace.FindNoteByFullPath(otherPath)!;
            workspace.UpdateWorkingContent(other, "Lokale Änderung der anderen Notiz");
            await File.WriteAllTextAsync(notePath, "# Extern 😀\r\n[[Andere]]");

            // FileShare.None bildet einen echten fremden Zugriffskonflikt nach.
            using (var lockedOther = new FileStream(otherPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                await ExpectFailureAsync<IOException>(() => File.ReadAllTextAsync(otherPath));
                NoteDocument reloaded = await workspace.ReloadNoteAsync(note);
                Require(reloaded.Content == "# Extern 😀\r\n[[Andere]]" && !reloaded.IsDirty,
                    "Gezieltes Neuladen muss trotz gesperrter Nachbarn den gewünschten Text übernehmen.");
                Require(reloaded.Links.Single().Target == "Andere", "Links müssen beim Einzelzugriff ebenfalls geparst werden.");
                Require(reloaded.Id == "Ziel.md" && reloaded.RelativePath == "Ziel.md" && reloaded.Title == "Ziel",
                    "Einzelzugriff und Vault-Scan müssen dieselben Notizmetadaten erzeugen.");
                note = reloaded;
            }

            Require(ReferenceEquals(other, workspace.FindNoteByFullPath(otherPath)) && other.IsDirty &&
                other.Content == "Lokale Änderung der anderen Notiz",
                "Unbeteiligte Notizen müssen Identität und lokalen Inhalt behalten.");

            await ExpectFailureAsync<InvalidOperationException>(() => repository.LoadNoteAsync(vaultPath, outsidePath));
            await ExpectFailureAsync<InvalidOperationException>(() => repository.LoadNoteAsync(
                vaultPath, Path.Combine(vaultPath, "..", "Ausserhalb.md")));
            await ExpectFailureAsync<InvalidOperationException>(() => repository.LoadNoteAsync(
                vaultPath, Path.Combine(vaultPath, "Unzulässig.txt")));
            await ExpectFailureAsync<ArgumentException>(() => repository.LoadNoteAsync(vaultPath, "Ziel.md"));

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await ExpectFailureAsync<OperationCanceledException>(() => repository.LoadNoteAsync(
                vaultPath, notePath, cancellation.Token));

            workspace.UpdateWorkingContent(note, "Lokaler Text vor Löschung");
            File.Delete(notePath);
            await ExpectFailureAsync<FileNotFoundException>(() => workspace.ReloadNoteAsync(note));
            Require(ReferenceEquals(note, workspace.FindNoteByFullPath(notePath)) && note.IsDirty,
                "Eine fehlende Datei darf das bisherige Arbeitsmodell nicht entfernen.");
            Require(workspace.TryDiscardWorkingChanges(note) && note.Content == "# Extern 😀\r\n[[Andere]]",
                "Ein fehlgeschlagenes Neuladen muss die bisherige Verwerfen-Basis erhalten.");
        }
        finally
        {
            // Ausschließlich das eindeutig benannte Testverzeichnis wird entfernt.
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    /// <summary>
    /// Erzwingt Änderungen zwischen Ladebeginn und Abschluss ohne zeitabhängige Sleeps.
    /// </summary>
    private static async Task RunDelayedReloadTestsAsync()
    {
        var repository = new DelayedReadRepository();
        NotesWorkspaceService workspace = CreateWorkspace(repository);
        string firstVault = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-reload-first"));
        string nextVault = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-reload-next"));
        await workspace.OpenVaultAsync(firstVault);
        NoteDocument note = workspace.LoadedNotes.Single();

        Task<NoteDocument> pending = workspace.ReloadNoteAsync(note);
        Require(!pending.IsCompleted && repository.ScanCount == 1, "Neuladen muss den Einzelzugriff statt eines zweiten Scans verwenden.");
        workspace.UpdateWorkingContent(note, "Während Laden geschrieben 😀");
        repository.CompleteRead();
        await ExpectFailureAsync<InvalidOperationException>(() => pending);
        Require(ReferenceEquals(note, workspace.LoadedNotes.Single()) && note.Content == "Während Laden geschrieben 😀" && note.IsDirty,
            "Verspätete Leseergebnisse dürfen neuere lokale Änderungen nicht ersetzen.");
        Require(workspace.TryDiscardWorkingChanges(note) && note.Content == "# Geladen",
            "Auch bei abgewiesener Übernahme muss die ursprüngliche Verwerfen-Basis erhalten bleiben.");

        using var cancellation = new CancellationTokenSource();
        Task<NoteDocument> cancelled = workspace.ReloadNoteAsync(note, cancellation.Token);
        cancellation.Cancel();
        // Simuliert einen Adapter, der einen zu spät eintreffenden Abbruch selbst nicht beobachtet hat.
        repository.CompleteRead();
        await ExpectFailureAsync<OperationCanceledException>(() => cancelled);
        Require(ReferenceEquals(note, workspace.LoadedNotes.Single()), "Ein beobachteter Abbruch darf kein neues Modell übernehmen.");

        Task<NoteDocument> firstRead = workspace.ReloadNoteAsync(note);
        Task<NoteDocument> secondRead = workspace.ReloadNoteAsync(note);
        repository.CompleteRead(0);
        NoteDocument replacement = await firstRead;
        repository.CompleteRead(0);
        await ExpectFailureAsync<InvalidOperationException>(() => secondRead);
        Require(ReferenceEquals(replacement, workspace.LoadedNotes.Single()), "Ein späterer Abschluss darf ein bereits ersetztes Modell nicht überschreiben.");

        Task<NoteDocument> oldVaultRead = workspace.ReloadNoteAsync(replacement);
        await workspace.OpenVaultAsync(nextVault);
        NoteDocument nextNote = workspace.LoadedNotes.Single();
        repository.CompleteRead();
        await ExpectFailureAsync<InvalidOperationException>(() => oldVaultRead);
        Require(ReferenceEquals(nextNote, workspace.LoadedNotes.Single()) && workspace.CurrentVault!.RootPath == nextVault,
            "Ein Leseabschluss des alten Vaults darf den neuen Vault nicht verändern.");
        int readsBefore = repository.ReadCount;
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.ReloadNoteAsync(replacement));
        Require(repository.ReadCount == readsBefore, "Ein bereits veraltetes Modell darf keinen weiteren Leseauftrag starten.");
    }

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
    /// Liefert Vault-Scans sofort, hält einzelne Notizlesevorgänge jedoch kontrolliert offen.
    /// </summary>
    private sealed class DelayedReadRepository : IVaultRepository
    {
        private readonly List<(string Path, TaskCompletionSource<NoteDocument> Completion)> _pendingReads = new();

        /// <summary>Anzahl vollständiger Vault-Scans.</summary>
        public int ScanCount { get; private set; }
        /// <summary>Anzahl angeforderter Einzelzugriffe.</summary>
        public int ReadCount { get; private set; }

        /// <summary>Beendet einen ausgewählten Einzelzugriff mit einem neuen Dateistand.</summary>
        public void CompleteRead(int index = 0)
        {
            var pending = _pendingReads[index];
            _pendingReads.RemoveAt(index);
            pending.Completion.SetResult(CreateNote(pending.Path, "# Neu geladen"));
        }

        /// <inheritdoc />
        public bool VaultExists(string vaultPath) => true;
        /// <inheritdoc />
        public IReadOnlyList<string> LoadFolderPaths(string vaultPath, CancellationToken cancellationToken = default) => Array.Empty<string>();
        /// <inheritdoc />
        public bool HasChangedSinceLoaded(NoteDocument note) => false;

        /// <inheritdoc />
        public Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
        {
            ScanCount++;
            IReadOnlyList<NoteDocument> notes = new[] { CreateNote(Path.Combine(vaultPath, "Notiz.md"), "# Geladen") };
            return Task.FromResult(notes);
        }

        /// <inheritdoc />
        public Task<NoteDocument> LoadNoteAsync(string vaultPath, string fullNotePath, CancellationToken cancellationToken = default)
        {
            ReadCount++;
            var completion = new TaskCompletionSource<NoteDocument>();
            _pendingReads.Add((fullNotePath, completion));
            return completion.Task;
        }

        private static NoteDocument CreateNote(string fullPath, string content) => new()
        {
            Id = "Notiz.md", Title = "Notiz", RelativePath = "Notiz.md",
            FullPath = fullPath, Content = content
        };

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

    private sealed class MemorySettingsRepository : ISettingsRepository
    {
        /// <inheritdoc />
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        /// <inheritdoc />
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

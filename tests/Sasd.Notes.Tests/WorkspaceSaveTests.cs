using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft Speicherzustände mit gezielt verzögerten Repository-Antworten ohne Dateisystemzugriffe.
/// </summary>
internal static class WorkspaceSaveTests
{
    /// <summary>
    /// Prüft neuere Editoränderungen, parallele Befehle, Fehler, Abbruch und erneutes Speichern.
    /// </summary>
    public static async Task RunAsync()
    {
        var repository = new DelayedVaultRepository();
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            repository, new MemorySettingsRepository(), parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
        string vaultPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-save-tests"));
        await workspace.OpenVaultAsync(vaultPath);
        NoteDocument note = workspace.LoadedNotes.Single();
        DateTime originalTimestamp = note.LastModifiedUtc;

        // Der erste Auftrag bleibt offen, während ein neuer Editorstand entsteht.
        Task saving = workspace.SaveNoteAsync(note, "[[Gespeichert]] 😀\r\n");
        Require(!saving.IsCompleted, "Der Test muss einen tatsächlich offenen Schreibauftrag prüfen.");
        NoteDocument firstSnapshot = repository.PendingNote!;
        Require(!ReferenceEquals(note, firstSnapshot), "Das Repository darf nicht das veränderliche Arbeitsmodell erhalten.");
        workspace.UpdateWorkingContent(note, "[[Neu]] Ω\n");
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.SaveNoteAsync(note, "Doppelauftrag"));
        Require(repository.SaveCount == 1, "Doppelte Speicherbefehle dürfen keinen zweiten Schreibauftrag erzeugen.");
        Require(note.Content == "[[Neu]] Ω\n", "Ein abgewiesener Auftrag darf den Editorstand nicht verändern.");
        Require(firstSnapshot.Content == "[[Gespeichert]] 😀\r\n", "Weiteres Tippen darf den Speicherschnappschuss nicht verändern.");
        Require(firstSnapshot.Links.Single().Target == "Gespeichert", "Der Schnappschuss muss seine eigenen Links besitzen.");

        repository.CompleteSave();
        await saving;
        Require(note.IsDirty, "Neuere Änderungen müssen nach Abschluss des älteren Schreibauftrags dirty bleiben.");
        Require(note.Content == "[[Neu]] Ω\n" && note.Links.Single().Target == "Neu",
            "Der Abschluss darf weder neuen Text noch neue Links überschreiben.");
        Require(note.LastModifiedUtc == repository.SavedTimestamp, "Der erfolgreiche Zeitstempel muss übernommen werden.");

        // Ein unveränderter zweiter Speicherstand darf erst nach erfolgreichem Abschluss sauber werden.
        Task retry = workspace.SaveNoteAsync(note, note.Content);
        Require(repository.PendingNote!.LastModifiedUtc == repository.SavedTimestamp,
            "Der Folgeauftrag muss vom zuletzt erfolgreich gespeicherten Zeitstempel ausgehen.");
        repository.CompleteSave();
        await retry;
        Require(!note.IsDirty, "Ein vollständig gespeicherter aktueller Stand muss clean werden.");

        DateTime successfulTimestamp = note.LastModifiedUtc;
        Task failing = workspace.SaveNoteAsync(note, "Fehlerstand");
        repository.FailSave(new IOException("Simulierter Schreibfehler."));
        await ExpectFailureAsync<IOException>(() => failing);
        Require(note.IsDirty && note.Content == "Fehlerstand", "Bei einem Schreibfehler muss der angeforderte Text dirty erhalten bleiben.");
        Require(note.LastModifiedUtc == successfulTimestamp, "Ein Fehler darf den gültigen Zeitstempel nicht verändern.");

        using var cancellation = new CancellationTokenSource();
        Task cancelled = workspace.SaveNoteAsync(note, "Abbruchstand", cancellation.Token);
        cancellation.Cancel();
        repository.CancelSave(cancellation.Token);
        await ExpectFailureAsync<OperationCanceledException>(() => cancelled);
        Require(note.IsDirty && note.Content == "Abbruchstand", "Ein Abbruch muss den ungespeicherten Text erhalten.");
        Require(note.LastModifiedUtc == successfulTimestamp, "Ein Abbruch darf den Zeitstempel nicht verändern.");

        int saveCount = repository.SaveCount;
        await ExpectFailureAsync<OperationCanceledException>(() => workspace.SaveNoteAsync(note, "Nicht übernehmen", cancellation.Token));
        Require(repository.SaveCount == saveCount && note.Content == "Abbruchstand",
            "Ein bereits abgebrochener Auftrag darf weder Repository noch Arbeitsmodell verändern.");

        // Auch nach Fehler und Abbruch muss die Auftragssperre wieder freigegeben sein.
        Task finalSave = workspace.SaveNoteAsync(note, string.Empty);
        repository.CompleteSave();
        await finalSave;
        Require(!note.IsDirty && note.Content == string.Empty, "Auch ein leeres Dokument muss nach Wiederholung speicherbar bleiben.");
        Require(note.LastModifiedUtc != originalTimestamp, "Der Test muss einen neuen gespeicherten Zeitstempel erhalten haben.");
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
    /// Hält jeden Schreibauftrag bis zur ausdrücklichen Testentscheidung offen.
    /// </summary>
    private sealed class DelayedVaultRepository : IVaultRepository
    {
        private TaskCompletionSource<bool> _completion = new();
        /// <summary>Der zuletzt angeforderte Speicherschnappschuss.</summary>
        public NoteDocument? PendingNote { get; private set; }
        /// <summary>Anzahl tatsächlich gestarteter Schreibaufträge.</summary>
        public int SaveCount { get; private set; }
        /// <summary>Simulierter Zeitstempel des erfolgreichen Dateistands.</summary>
        public DateTime SavedTimestamp { get; private set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>Bestätigt den offenen Auftrag mit einem neuen Zeitstempel.</summary>
        public void CompleteSave()
        {
            SavedTimestamp = SavedTimestamp.AddSeconds(1);
            PendingNote!.LastModifiedUtc = SavedTimestamp;
            _completion.SetResult(true);
        }

        /// <summary>Beendet den Auftrag mit dem angegebenen Fehler.</summary>
        public void FailSave(Exception exception) => _completion.SetException(exception);
        /// <summary>Beendet den Auftrag als abgebrochen.</summary>
        public void CancelSave(CancellationToken token) => _completion.SetCanceled(token);

        /// <inheritdoc />
        public bool VaultExists(string vaultPath) => true;
        /// <inheritdoc />
        public IReadOnlyList<string> LoadFolderPaths(string vaultPath) => Array.Empty<string>();
        /// <inheritdoc />
        public bool HasChangedSinceLoaded(NoteDocument note) => false;

        /// <inheritdoc />
        public Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<NoteDocument> notes = new[]
            {
                new NoteDocument
                {
                    Id = "Notiz.md",
                    Title = "Notiz",
                    FullPath = Path.Combine(vaultPath, "Notiz.md"),
                    RelativePath = "Notiz.md",
                    Content = "# Original",
                    LastModifiedUtc = SavedTimestamp
                }
            };
            return Task.FromResult(notes);
        }

        /// <inheritdoc />
        public Task SaveNoteAsync(string vaultPath, NoteDocument note, CancellationToken cancellationToken = default)
        {
            PendingNote = note;
            SaveCount++;
            _completion = new TaskCompletionSource<bool>();
            return _completion.Task;
        }

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

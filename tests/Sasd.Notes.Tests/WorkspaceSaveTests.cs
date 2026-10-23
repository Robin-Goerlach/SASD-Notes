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

        // Mehrere Bearbeitungsschritte müssen immer denselben geladenen Ausgangstext wiederherstellen.
        workspace.UpdateWorkingContent(note, "[[Erste Änderung]] 😀\r\n");
        workspace.UpdateWorkingContent(note, "[[Zweite Änderung]] Ω\n");
        Require(workspace.TryDiscardWorkingChanges(note), "Bekannte lokale Änderungen müssen verwerfbar sein.");
        Require(note.Content == "# Original" && !note.IsDirty && note.Links.Count == 0,
            "Verwerfen muss Ausgangstext, Links und Dirty-State gemeinsam wiederherstellen.");
        Require(ReferenceEquals(note, workspace.FindNoteByFullPath(note.FullPath)),
            "Auch beim erneuten Öffnen muss das geladene Modell den verworfenen Stand verlieren.");
        Require(repository.SaveCount == 0 && note.LastModifiedUtc == originalTimestamp,
            "Verwerfen darf weder schreiben noch den Dateizeitstempel verändern.");
        Require(workspace.TryDiscardWorkingChanges(note), "Erneutes Verwerfen einer sauberen Notiz muss harmlos sein.");

        // Der erste Auftrag bleibt offen, während ein neuer Editorstand entsteht.
        Task saving = workspace.SaveNoteAsync(note, "[[Gespeichert]] 😀\r\n");
        Require(!saving.IsCompleted, "Der Test muss einen tatsächlich offenen Schreibauftrag prüfen.");
        NoteDocument firstSnapshot = repository.PendingNote!;
        await ExpectFailureAsync<InvalidOperationException>(() => workspace.ReloadNoteAsync(note));
        Require(!workspace.TryDiscardWorkingChanges(note), "Während einer Speicherung darf nicht verworfen werden.");
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

        // Ein neuerer Editorstand wird zum gerade gespeicherten Text verworfen, nicht zum ursprünglichen Ladestand.
        Require(workspace.TryDiscardWorkingChanges(note), "Nach Schreibabschluss müssen neuere Änderungen verwerfbar sein.");
        Require(note.Content == "[[Gespeichert]] 😀\r\n" && note.Links.Single().Target == "Gespeichert" && !note.IsDirty,
            "Verwerfen muss nach asynchronem Speichern den zuletzt gespeicherten Stand liefern.");
        workspace.UpdateWorkingContent(note, "[[Neu]] Ω\n");

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

        // Ein erfolgreicher Leertext-Speichervorgang ersetzt auch die Verwerfen-Basis früherer Fehler.
        workspace.UpdateWorkingContent(note, "Nach leerem Speichern");
        Require(workspace.TryDiscardWorkingChanges(note) && note.Content == string.Empty && !note.IsDirty,
            "Auch ein leerer gespeicherter Ausgangstext muss wiederherstellbar sein.");

        workspace.UpdateWorkingContent(note, "Nicht speicherbar");
        Task anotherFailure = workspace.SaveNoteAsync(note, note.Content);
        repository.FailSave(new IOException("Erneuter Schreibfehler."));
        await ExpectFailureAsync<IOException>(() => anotherFailure);
        Require(workspace.TryDiscardWorkingChanges(note) && note.Content == string.Empty,
            "Ein Schreibfehler darf die letzte erfolgreiche Verwerfen-Basis nicht ersetzen.");

        workspace.UpdateWorkingContent(note, "Abgebrochene Änderung");
        using var anotherCancellation = new CancellationTokenSource();
        Task anotherCancel = workspace.SaveNoteAsync(note, note.Content, anotherCancellation.Token);
        anotherCancellation.Cancel();
        repository.CancelSave(anotherCancellation.Token);
        await ExpectFailureAsync<OperationCanceledException>(() => anotherCancel);
        Require(workspace.TryDiscardWorkingChanges(note) && note.Content == string.Empty,
            "Auch ein abgebrochener Schreibauftrag muss zur letzten erfolgreichen Basis zurückführen.");

        // Neuladen ersetzt das Modell. Dessen erste Bearbeitung muss den neuen Ladestand als Basis verwenden.
        workspace.UpdateWorkingContent(note, "Vor Neuladen");
        NoteDocument reloadedNote = await workspace.ReloadNoteAsync(note);
        workspace.UpdateWorkingContent(reloadedNote, "Nach Neuladen");
        Require(workspace.TryDiscardWorkingChanges(reloadedNote) && reloadedNote.Content == "# Original",
            "Die Verwerfen-Basis eines neu geladenen Modells muss dessen eigener Ladestand sein.");

        var unknownDirtyNote = new NoteDocument
        {
            Id = "Unbekannt.md", Title = "Unbekannt", RelativePath = "Unbekannt.md",
            FullPath = Path.Combine(vaultPath, "Unbekannt.md"), Content = "Unbekannte Änderungen", IsDirty = true
        };
        Require(!workspace.TryDiscardWorkingChanges(unknownDirtyNote) && unknownDirtyNote.IsDirty,
            "Ohne bekannte Basis dürfen Änderungen nicht als verworfen gelten.");
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
        public Task<NoteDocument> LoadNoteAsync(string vaultPath, string fullNotePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new NoteDocument
            {
                Id = "Notiz.md", Title = "Notiz", FullPath = fullNotePath,
                RelativePath = "Notiz.md", Content = "# Original", LastModifiedUtc = SavedTimestamp
            });
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

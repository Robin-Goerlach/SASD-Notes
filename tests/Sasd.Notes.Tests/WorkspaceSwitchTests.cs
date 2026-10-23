using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft den vollständigen Zustandserhalt bei fehlgeschlagenen Vault-Wechseln.
/// </summary>
internal static class WorkspaceSwitchTests
{
    /// <summary>
    /// Führt Fehler-, Abbruch- und Erfolgsfälle mit kontrollierten Repository-Adaptern aus.
    /// </summary>
    public static async Task RunAsync()
    {
        string oldPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-switch-old"));
        string newPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-switch-new"));
        var vaultRepository = new SwitchVaultRepository();
        var settingsRepository = new SwitchSettingsRepository();
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var workspace = new NotesWorkspaceService(
            vaultRepository, settingsRepository, parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
        await workspace.InitializeAsync();

        // Auch ein Fehler beim ersten Öffnen darf keinen teilweise geöffneten Vault hinterlassen.
        vaultRepository.FailFolders = true;
        await ExpectIOExceptionAsync(() => workspace.OpenVaultAsync(oldPath));
        AssertEx.True(workspace.CurrentVault is null, "Nach fehlgeschlagenem Erstöffnen darf kein Vault aktiv sein.");
        AssertEx.Equal(0, workspace.LoadedNotes.Count, "Nach fehlgeschlagenem Erstöffnen dürfen keine Notizen übernommen sein.");
        AssertEx.True(workspace.GetLastOpenedVaultPath() is null, "Ein fehlgeschlagenes Erstöffnen darf nicht als zuletzt geöffnet gelten.");

        vaultRepository.FailFolders = false;
        await workspace.OpenVaultAsync(oldPath);
        NoteDocument oldNote = workspace.LoadedNotes.Single();
        workspace.UpdateWorkingContent(oldNote, "Ungespeicherte lokale Änderung 😀");
        AppSettings previousSettings = settingsRepository.StoredSettings;
        vaultRepository.Folders = new[] { "NeuerOrdner" };

        vaultRepository.FailNotes = true;
        await ExpectIOExceptionAsync(() => workspace.OpenVaultAsync(newPath));
        AssertOldState(workspace, oldPath, oldNote);
        vaultRepository.FailNotes = false;

        // Der Ordnerfehler tritt erst auf, nachdem der neue Notizbestand bereits eingelesen wurde.
        vaultRepository.FailFolders = true;
        await ExpectIOExceptionAsync(() => workspace.OpenVaultAsync(newPath));
        AssertOldState(workspace, oldPath, oldNote);
        vaultRepository.FailFolders = false;

        settingsRepository.FailSave = true;
        await ExpectIOExceptionAsync(() => workspace.OpenVaultAsync(newPath));
        AssertOldState(workspace, oldPath, oldNote);
        AssertEx.True(ReferenceEquals(previousSettings, settingsRepository.StoredSettings),
            "Ein fehlgeschlagener Einstellungs-Speichervorgang muss den bisherigen Stand erhalten.");
        AssertEx.Equal(oldPath, previousSettings.LastOpenedVaultPath, "Bereits gültige Einstellungen dürfen nicht vorab mutiert werden.");
        AssertEx.True(!ReferenceEquals(previousSettings, settingsRepository.LastAttemptedSettings),
            "Das Repository muss einen separaten Einstellungskandidaten erhalten.");
        AssertEx.True(!ReferenceEquals(previousSettings.RecentFolders[0], settingsRepository.LastAttemptedSettings!.RecentFolders[1]),
            "Auch Recent-Folder-Einträge des Kandidaten müssen vom aktiven Stand unabhängig sein.");
        settingsRepository.FailSave = false;

        using var cancellation = new CancellationTokenSource();
        // Der Abbruch erfolgt erst nach eingelesenen Notizen und Ordnern, unmittelbar vor der Übernahme.
        vaultRepository.AfterFolderScan = cancellation.Cancel;
        bool wasCancelled = false;
        try
        {
            await workspace.OpenVaultAsync(newPath, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            wasCancelled = true;
        }

        AssertEx.True(wasCancelled, "Ein abgebrochener Wechsel muss als Abbruch gemeldet werden.");
        AssertOldState(workspace, oldPath, oldNote);
        vaultRepository.AfterFolderScan = null;

        // Auch die Verwerfen-Basis muss einen fehlgeschlagenen Wechsel unverändert überstehen.
        AssertEx.True(workspace.TryDiscardWorkingChanges(oldNote),
            "Nach fehlgeschlagenen Wechseln muss Verwerfen weiterhin möglich sein.");
        AssertEx.Equal("# Geladene Notiz", oldNote.Content,
            "Ein fehlgeschlagener Wechsel darf die ursprüngliche Verwerfen-Basis nicht ersetzen.");
        workspace.UpdateWorkingContent(oldNote, "Ungespeicherte lokale Änderung 😀");

        // Nach Fehlern muss ein normaler Wechsel weiterhin den vollständigen neuen Zustand übernehmen können.
        await workspace.OpenVaultAsync(newPath);
        AssertEx.Equal(newPath, workspace.CurrentVault!.RootPath, "Der erfolgreiche Wechsel muss den neuen Vault aktivieren.");
        AssertEx.Equal(Path.Combine(newPath, "Notiz.md"), workspace.LoadedNotes.Single().FullPath,
            "Der neue Vault muss seinen eigenen Notizbestand erhalten.");
        AssertEx.False(workspace.LoadedNotes.Single().IsDirty, "Die frisch geladene Notiz darf nicht dirty sein.");
        AssertEx.True(workspace.BuildTreeItems().Any(item => item.RelativePath == "NeuerOrdner"),
            "Der erfolgreiche Wechsel muss die neuen Ordner übernehmen.");
        AssertEx.False(workspace.BuildTreeItems().Any(item => item.RelativePath == "AlterOrdner"),
            "Ordner des vorherigen Vaults dürfen nach erfolgreichem Wechsel nicht zurückbleiben.");
        AssertEx.Equal(newPath, workspace.GetLastOpenedVaultPath(), "Die Einstellungen müssen zum neuen Vault gehören.");
        AssertEx.Equal(newPath, settingsRepository.StoredSettings.LastOpenedVaultPath,
            "Auch der gespeicherte Einstellungskandidat muss den neuen Vault referenzieren.");
        AssertEx.Equal(2, workspace.GetRecentFolders().Count, "Beide erfolgreich geöffneten Vaults müssen in Recent Folders stehen.");
        AssertEx.Equal("Ungespeicherte lokale Änderung 😀", oldNote.Content,
            "Der Wechsel darf den alten Notizgegenstand nicht nachträglich verändern.");
    }

    /// <summary>
    /// Prüft gemeinsam Pfad, Notizidentität, ungespeicherten Inhalt, Ordnerbaum und Recent Folders.
    /// </summary>
    private static void AssertOldState(NotesWorkspaceService workspace, string oldPath, NoteDocument oldNote)
    {
        AssertEx.Equal(oldPath, workspace.CurrentVault!.RootPath, "Bei Fehlern muss der bisherige Vault aktiv bleiben.");
        AssertEx.True(ReferenceEquals(oldNote, workspace.LoadedNotes.Single()), "Der bisherige Notizgegenstand muss erhalten bleiben.");
        AssertEx.Equal("Ungespeicherte lokale Änderung 😀", oldNote.Content, "Lokale Änderungen müssen erhalten bleiben.");
        AssertEx.True(oldNote.IsDirty, "Der Dirty-State muss bei fehlgeschlagenem Wechsel erhalten bleiben.");
        AssertEx.True(workspace.BuildTreeItems().Any(item => item.RelativePath == "AlterOrdner"),
            "Der bisherige Ordnerbaum muss erhalten bleiben.");
        AssertEx.False(workspace.BuildTreeItems().Any(item => item.RelativePath == "NeuerOrdner"),
            "Der Kandidatenbaum darf bei Fehlern nicht sichtbar werden.");
        AssertEx.Equal(oldPath, workspace.GetLastOpenedVaultPath(), "Last Vault darf bei Fehlern nicht wechseln.");
        AssertEx.Equal(1, workspace.GetRecentFolders().Count, "Fehlgeschlagene Öffnungen gehören nicht in Recent Folders.");
        AssertEx.Equal(oldPath, workspace.GetRecentFolders().Single().Path, "Der vorherige Recent-Folder-Eintrag muss erhalten bleiben.");
    }

    private static async Task ExpectIOExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (IOException)
        {
            return;
        }

        throw new InvalidOperationException("Der simulierte Repository-Fehler wurde nicht weitergereicht.");
    }

    /// <summary>
    /// Simuliert Dateifehler exakt an den Application-Schnittstellen, ohne produktive Dateien anzufassen.
    /// </summary>
    private sealed class SwitchVaultRepository : IVaultRepository
    {
        public bool FailNotes { get; set; }
        public bool FailFolders { get; set; }
        public Action? AfterFolderScan { get; set; }
        public IReadOnlyList<string> Folders { get; set; } = new[] { "AlterOrdner" };

        public bool VaultExists(string vaultPath) => true;

        public Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (FailNotes)
            {
                return Task.FromException<IReadOnlyList<NoteDocument>>(new IOException("Simulierter Notizlesefehler."));
            }

            IReadOnlyList<NoteDocument> notes = new[]
            {
                new NoteDocument
                {
                    Id = "Notiz.md",
                    Title = "Notiz",
                    FullPath = Path.Combine(vaultPath, "Notiz.md"),
                    RelativePath = "Notiz.md",
                    Content = "# Geladene Notiz"
                }
            };
            return Task.FromResult(notes);
        }

        public IReadOnlyList<string> LoadFolderPaths(string vaultPath)
        {
            if (FailFolders)
            {
                throw new IOException("Simulierter Ordnerlesefehler.");
            }

            AfterFolderScan?.Invoke();
            return Folders;
        }

        // Nicht verwendete Einzelzugriffe und Schreiboperationen schlagen bei unbeabsichtigten Aufrufen bewusst fehl.
        /// <inheritdoc />
        public Task<NoteDocument> LoadNoteAsync(string vaultPath, string fullNotePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task SaveNoteAsync(string vaultPath, NoteDocument note, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public bool HasChangedSinceLoaded(NoteDocument note) => false;
        public Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<string> CreateFolderAsync(string vaultPath, string? relativeParentPath, string folderName, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class SwitchSettingsRepository : ISettingsRepository
    {
        public bool FailSave { get; set; }
        public AppSettings StoredSettings { get; private set; } = new();
        public AppSettings? LastAttemptedSettings { get; private set; }

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(StoredSettings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastAttemptedSettings = settings;
            if (FailSave)
            {
                return Task.FromException(new IOException("Simulierter Einstellungs-Schreibfehler."));
            }

            StoredSettings = settings;
            return Task.CompletedTask;
        }
    }
}

using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft die syntaktische Normalisierung und Begrenzung zuletzt verwendeter Vaults.
/// </summary>
internal static class RecentFolderTests
{
    /// <summary>
    /// Prüft Altbestände, Dubletten, Wurzelpfade und die Speicherung nach erfolgreichem Öffnen.
    /// </summary>
    public static async Task RunAsync()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sasd-recent-" + Guid.NewGuid().ToString("N")));
        string duplicatePath = Path.Combine(root, "Doppelt");
        DateTime older = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        DateTime newer = older.AddHours(1);
        var initialSettings = new AppSettings
        {
            LastOpenedVaultPath = duplicatePath + Path.DirectorySeparatorChar,
            RecentFolders = new List<RecentFolderEntry>
            {
                new() { Path = duplicatePath + Path.DirectorySeparatorChar, LastOpenedUtc = older },
                new() { Path = Path.Combine(duplicatePath, "."), LastOpenedUtc = newer },
                new() { Path = "relativer-pfad", LastOpenedUtc = newer.AddHours(1) },
                new() { Path = "\0", LastOpenedUtc = newer.AddHours(2) }
            }
        };

        // Mehr als zehn gültige Altwerte prüfen zugleich die feste Begrenzung.
        for (int index = 0; index < 12; index++)
        {
            initialSettings.RecentFolders.Add(new RecentFolderEntry
            {
                Path = Path.Combine(root, "Archiv " + index),
                LastOpenedUtc = older.AddMinutes(-index)
            });
        }

        var settingsRepository = new MemorySettingsRepository(initialSettings);
        var vaultRepository = new AcceptingVaultRepository();
        NotesWorkspaceService workspace = CreateWorkspace(vaultRepository, settingsRepository);
        await workspace.InitializeAsync();

        IReadOnlyList<RecentFolderEntry> recent = workspace.GetRecentFolders();
        AssertEx.Equal(10, recent.Count, "Recent Folders müssen auf zehn eindeutige gültige Pfade begrenzt sein.");
        AssertEx.Equal(Path.TrimEndingDirectorySeparator(duplicatePath), recent[0].Path,
            "Abschlusszeichen und Punktsegmente dürfen keine doppelten Einträge erzeugen.");
        AssertEx.Equal(newer, recent[0].LastOpenedUtc, "Von Dubletten muss der jüngste Eintrag erhalten bleiben.");
        AssertEx.Equal(Path.TrimEndingDirectorySeparator(duplicatePath), workspace.GetLastOpenedVaultPath(),
            "Auch der zuletzt geöffnete Vault muss syntaktisch normalisiert werden.");
        AssertEx.False(ReferenceEquals(recent[0], workspace.GetRecentFolders()[0]),
            "Aufrufer müssen unabhängige Recent-Folder-Schnappschüsse erhalten.");

        string requested = Path.Combine(root, "Neu") + Path.DirectorySeparatorChar;
        VaultInfo opened = await workspace.OpenVaultAsync(requested);
        string normalizedRequested = Path.TrimEndingDirectorySeparator(Path.GetFullPath(requested));
        AssertEx.Equal(normalizedRequested, opened.RootPath, "Der aktive Vault benötigt einen kanonischen Pfad ohne Abschlusszeichen.");
        AssertEx.Equal("Neu", opened.DisplayName, "Ein normaler Vault muss seinen letzten Pfadbestandteil anzeigen.");
        AssertEx.Equal(normalizedRequested, settingsRepository.Stored.LastOpenedVaultPath,
            "Der kanonische Pfad muss als letzter Vault gespeichert werden.");
        AssertEx.Equal(10, settingsRepository.Stored.RecentFolders.Count,
            "Die gespeicherte Historie muss einschließlich des neuen Vaults auf zehn Einträge begrenzt bleiben.");
        AssertEx.Equal(1, settingsRepository.Stored.RecentFolders.Count(item =>
                string.Equals(item.Path, duplicatePath, StringComparison.OrdinalIgnoreCase)),
            "Ein zuvor doppelt gespeicherter Vault darf nur einmal übernommen werden.");

        string pathRoot = Path.GetPathRoot(root)
            ?? throw new InvalidOperationException("Der Testpfad besitzt keine Wurzel.");
        VaultInfo rootVault = await workspace.OpenVaultAsync(pathRoot);
        AssertEx.Equal(Path.TrimEndingDirectorySeparator(pathRoot), rootVault.RootPath,
            "Die Pfadnormalisierung darf eine Laufwerks- oder Dateisystemwurzel nicht beschädigen.");
        AssertEx.Equal(rootVault.RootPath, rootVault.DisplayName,
            "Eine Wurzel ohne letzten Namen muss ihren vollständigen Pfad als Anzeigenamen verwenden.");

        var invalidLastSettings = new MemorySettingsRepository(new AppSettings
        {
            LastOpenedVaultPath = "relativ",
            RecentFolders = new List<RecentFolderEntry>()
        });
        NotesWorkspaceService invalidWorkspace = CreateWorkspace(new AcceptingVaultRepository(), invalidLastSettings);
        await invalidWorkspace.InitializeAsync();
        AssertEx.True(invalidWorkspace.GetLastOpenedVaultPath() is null,
            "Ein relativer Altwert darf nicht vom aktuellen Arbeitsverzeichnis abhängig werden.");
    }

    private static NotesWorkspaceService CreateWorkspace(
        IVaultRepository vaultRepository,
        ISettingsRepository settingsRepository)
    {
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        return new NotesWorkspaceService(
            vaultRepository, settingsRepository, parser, new MarkdownOutlineParser(),
            resolver, new SearchService(), new BacklinkService(resolver));
    }

    /// <summary>
    /// Akzeptiert Pfade ohne echte Dateisystemzugriffe und liefert einen leeren Vault.
    /// </summary>
    private sealed class AcceptingVaultRepository : IVaultRepository
    {
        public bool VaultExists(string vaultPath) => true;
        public Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(
            string vaultPath, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<NoteDocument>>(Array.Empty<NoteDocument>());
        public IReadOnlyList<string> LoadFolderPaths(string vaultPath, CancellationToken cancellationToken = default) => Array.Empty<string>();
        public Task<NoteDocument> LoadNoteAsync(
            string vaultPath, string fullNotePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task SaveNoteAsync(
            string vaultPath, NoteDocument note, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public bool HasChangedSinceLoaded(NoteDocument note) => false;
        public Task<NoteDocument> CreateNoteAsync(
            string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<string> CreateFolderAsync(
            string vaultPath, string? relativeParentPath, string folderName, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    /// <summary>
    /// Hält Testeinstellungen und den zuletzt gespeicherten Kandidaten vollständig im Speicher.
    /// </summary>
    private sealed class MemorySettingsRepository : ISettingsRepository
    {
        public MemorySettingsRepository(AppSettings stored)
        {
            Stored = stored;
        }

        public AppSettings Stored { get; private set; }

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Stored);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stored = settings;
            return Task.CompletedTask;
        }
    }
}

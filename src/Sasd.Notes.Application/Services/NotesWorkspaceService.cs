using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Orchestriert die wichtigsten Anwendungsfälle rund um Vault, Notizen und Benutzereinstellungen.
/// </summary>
public sealed class NotesWorkspaceService
{
    private readonly IVaultRepository _vaultRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IWikiLinkParser _wikiLinkParser;
    private readonly IOutlineParser _outlineParser;
    private readonly ILinkResolver _linkResolver;
    private readonly SearchService _searchService;
    private readonly BacklinkService _backlinkService;
    private AppSettings _settings = new();
    private readonly List<NoteDocument> _loadedNotes = new();
    private readonly List<string> _loadedFolderPaths = new();

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="NotesWorkspaceService"/>-Klasse.
    /// </summary>
    public NotesWorkspaceService(
        IVaultRepository vaultRepository,
        ISettingsRepository settingsRepository,
        IWikiLinkParser wikiLinkParser,
        IOutlineParser outlineParser,
        ILinkResolver linkResolver,
        SearchService searchService,
        BacklinkService backlinkService)
    {
        _vaultRepository = vaultRepository;
        _settingsRepository = settingsRepository;
        _wikiLinkParser = wikiLinkParser;
        _outlineParser = outlineParser;
        _linkResolver = linkResolver;
        _searchService = searchService;
        _backlinkService = backlinkService;
    }

    /// <summary>
    /// Informationen über den aktuell geöffneten Vault.
    /// </summary>
    public VaultInfo? CurrentVault { get; private set; }

    /// <summary>
    /// Im Arbeitsspeicher bekannte Notizen des aktuell geöffneten Vaults.
    /// </summary>
    public IReadOnlyList<NoteDocument> LoadedNotes => _loadedNotes;

    /// <summary>
    /// Lädt die Einstellungen beim Programmstart.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Einstellungen werden früh geladen, damit Recent Folders sofort verfügbar sind.
        _settings = await _settingsRepository.LoadAsync(cancellationToken);
    }

    /// <summary>
    /// Gibt die zuletzt verwendeten Vaults in neuer-zu-alter-Reihenfolge zurück.
    /// </summary>
    public IReadOnlyList<RecentFolderEntry> GetRecentFolders()
    {
        return _settings.RecentFolders
            .OrderByDescending(item => item.LastOpenedUtc)
            .ToList();
    }

    /// <summary>
    /// Liefert den Pfad des zuletzt geöffneten Vaults.
    /// </summary>
    public string? GetLastOpenedVaultPath()
    {
        return _settings.LastOpenedVaultPath;
    }

    /// <summary>
    /// Prüft über das Repository, ob ein Vault geöffnet werden kann.
    /// </summary>
    /// <param name="vaultPath">Zu prüfender Vault-Pfad.</param>
    /// <returns><see langword="true"/>, wenn der Vault-Ordner vorhanden ist.</returns>
    public bool VaultExists(string vaultPath)
    {
        return !string.IsNullOrWhiteSpace(vaultPath) && _vaultRepository.VaultExists(vaultPath);
    }

    /// <summary>
    /// Öffnet einen Vault und übernimmt seinen Zustand erst nach erfolgreicher Vorbereitung und Einstellungsspeicherung.
    /// </summary>
    /// <param name="vaultPath">Pfad zum gewünschten Vault-Wurzelverzeichnis.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der Vorbereitung.</param>
    /// <returns>Informationen über den erfolgreich geöffneten Vault.</returns>
    /// <remarks>Bei einem Fehler oder beobachteten Abbruch bleibt der bisherige Workspace im Arbeitsspeicher erhalten.</remarks>
    public async Task<VaultInfo> OpenVaultAsync(string vaultPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultPath))
        {
            throw new ArgumentException("Der Vault-Pfad darf nicht leer sein.", nameof(vaultPath));
        }

        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        if (!_vaultRepository.VaultExists(normalizedVaultPath))
        {
            throw new DirectoryNotFoundException($"Der Vault-Ordner wurde nicht gefunden: {vaultPath}");
        }

        var vaultInfo = new VaultInfo
        {
            DisplayName = Path.GetFileName(normalizedVaultPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            RootPath = normalizedVaultPath
        };

        // Alle potenziell fehlschlagenden Leseoperationen arbeiten zunächst auf einem separaten Kandidaten.
        // Insbesondere darf ein fehlgeschlagener Ordnerscan die bisherigen Notizen nicht bereits ersetzen.
        IReadOnlyList<NoteDocument> notes = await _vaultRepository.LoadAllNotesAsync(normalizedVaultPath, cancellationToken);
        List<NoteDocument> preparedNotes = notes
            .OrderBy(note => note.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        List<string> preparedFolders = _vaultRepository.LoadFolderPaths(normalizedVaultPath).ToList();

        cancellationToken.ThrowIfCancellationRequested();
        AppSettings preparedSettings = CreateSettingsWithRecentFolder(normalizedVaultPath);

        // Auch die Einstellungen werden zuerst gespeichert. Scheitert dies, bleibt der bisherige Workspace vollständig aktiv.
        await _settingsRepository.SaveAsync(preparedSettings, cancellationToken);

        // Erst jetzt wird der vollständige neue Zustand ohne weiteren await übernommen.
        // Nach erfolgreichem Speichern wird kein neuer Abbruchpunkt mehr eingefügt, damit Einstellungen und Workspace zusammenpassen.
        _loadedNotes.Clear();
        _loadedNotes.AddRange(preparedNotes);
        _loadedFolderPaths.Clear();
        _loadedFolderPaths.AddRange(preparedFolders);
        _settings = preparedSettings;
        CurrentVault = vaultInfo;

        return vaultInfo;
    }

    /// <summary>
    /// Baut den fachlichen Navigationsbaum für den aktuellen Vault auf.
    /// </summary>
    public IReadOnlyList<VaultTreeItem> BuildTreeItems()
    {
        if (CurrentVault is null)
        {
            return Array.Empty<VaultTreeItem>();
        }

        var result = new List<VaultTreeItem>
        {
            new()
            {
                DisplayText = CurrentVault.DisplayName,
                FullPath = CurrentVault.RootPath,
                RelativePath = string.Empty,
                ItemType = TreeItemType.Root
            }
        };

        var knownFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Zuerst werden alle Ordner eingetragen, damit auch leere Ordner dargestellt werden.
        foreach (string folderPath in _loadedFolderPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            AddFolderTreeItems(result, knownFolders, folderPath);
        }

        foreach (NoteDocument note in _loadedNotes)
        {
            result.Add(new VaultTreeItem
            {
                DisplayText = Path.GetFileName(note.RelativePath),
                FullPath = note.FullPath,
                RelativePath = note.RelativePath,
                ItemType = TreeItemType.Note
            });
        }

        return result;
    }

    /// <summary>
    /// Fügt einen Ordner und seine Zwischenordner einmalig in den fachlichen Baum ein.
    /// </summary>
    private void AddFolderTreeItems(
        ICollection<VaultTreeItem> result,
        ISet<string> knownFolders,
        string relativeFolderPath)
    {
        string normalizedPath = relativeFolderPath.Replace('\\', '/');
        string[] segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string currentFolder = string.Empty;

        for (int index = 0; index < segments.Length; index++)
        {
            currentFolder = string.IsNullOrEmpty(currentFolder)
                ? segments[index]
                : currentFolder + "/" + segments[index];

            if (!knownFolders.Add(currentFolder))
            {
                continue;
            }

            result.Add(new VaultTreeItem
            {
                DisplayText = segments[index],
                FullPath = Path.Combine(CurrentVault!.RootPath, currentFolder.Replace('/', Path.DirectorySeparatorChar)),
                RelativePath = currentFolder,
                ItemType = TreeItemType.Folder
            });
        }
    }

    /// <summary>
    /// Speichert eine Notiz, aktualisiert erkannte Links und den In-Memory-Zustand.
    /// </summary>
    public async Task SaveNoteAsync(NoteDocument note, string updatedContent, CancellationToken cancellationToken = default)
    {
        note.Content = updatedContent;
        note.Links = _wikiLinkParser.Parse(updatedContent);

        if (CurrentVault is null)
        {
            throw new InvalidOperationException("Es ist kein Vault geöffnet.");
        }

        await _vaultRepository.SaveNoteAsync(CurrentVault.RootPath, note, cancellationToken);
        note.IsDirty = false;
    }

    /// <summary>
    /// Erstellt eine neue Notiz im aktuellen Vault.
    /// </summary>
    public async Task<NoteDocument> CreateNoteAsync(string title, string? relativeFolderPath, CancellationToken cancellationToken = default)
    {
        if (CurrentVault is null)
        {
            throw new InvalidOperationException("Es ist kein Vault geöffnet.");
        }

        NoteDocument createdNote = await _vaultRepository.CreateNoteAsync(CurrentVault.RootPath, relativeFolderPath, title, cancellationToken);
        createdNote.Links = _wikiLinkParser.Parse(createdNote.Content);

        _loadedNotes.Add(createdNote);
        SortNotes();

        return createdNote;
    }

    /// <summary>
    /// Erstellt einen neuen Unterordner im aktuellen Vault und liefert dessen relativen Pfad.
    /// </summary>
    /// <param name="folderName">Name des neuen Ordners.</param>
    /// <param name="relativeParentPath">Relativer übergeordneter Ordner oder <see langword="null"/>.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Der relative Pfad des neu erstellten Ordners.</returns>
    public async Task<string> CreateFolderAsync(
        string folderName,
        string? relativeParentPath,
        CancellationToken cancellationToken = default)
    {
        if (CurrentVault is null)
        {
            throw new InvalidOperationException("Es ist kein Vault geöffnet.");
        }

        // Die Pfad- und Namensprüfung bleibt im Infrastrukturadapter, damit alle Aufrufer dieselben Regeln verwenden.
        string createdFolderPath = await _vaultRepository.CreateFolderAsync(
            CurrentVault.RootPath,
            relativeParentPath,
            folderName,
            cancellationToken);

        // Der neue Ordner wird sofort in den Arbeitsspeicher übernommen, ohne den gesamten Vault neu zu laden.
        if (!_loadedFolderPaths.Contains(createdFolderPath, StringComparer.OrdinalIgnoreCase))
        {
            _loadedFolderPaths.Add(createdFolderPath);
        }

        return createdFolderPath;
    }

    /// <summary>
    /// Ermittelt Suchtreffer innerhalb des geladenen Vaults.
    /// </summary>
    public IReadOnlyList<SearchHit> Search(string query)
    {
        return _searchService.Search(_loadedNotes, query);
    }

    /// <summary>
    /// Ermittelt Rückverweise auf eine Notiz.
    /// </summary>
    public IReadOnlyList<BacklinkInfo> GetBacklinks(NoteDocument note)
    {
        NoteDocument currentSnapshot = CreateWorkingSnapshot(note);
        return _backlinkService.GetBacklinks(currentSnapshot, ReplaceOrInjectSnapshot(currentSnapshot));
    }

    /// <summary>
    /// Ermittelt Wiki-Links der Notiz, deren Ziele im aktuellen Vault nicht existieren.
    /// </summary>
    /// <param name="note">Notiz, deren Links geprüft werden sollen.</param>
    /// <returns>Die nicht auflösbaren Wiki-Links in ihrer ursprünglichen Reihenfolge.</returns>
    public IReadOnlyList<WikiLink> GetUnresolvedLinks(NoteDocument note)
    {
        NoteDocument currentSnapshot = CreateWorkingSnapshot(note);
        IReadOnlyList<NoteDocument> notes = ReplaceOrInjectSnapshot(currentSnapshot);

        // Die Prüfung verwendet bewusst denselben Resolver wie die Navigation und die Backlink-Suche.
        return currentSnapshot.Links
            .Where(link => _linkResolver.Resolve(link.Target, notes) is null)
            .ToList();
    }

    /// <summary>
    /// Liefert geladene Notizen, deren Dateien extern geändert oder gelöscht wurden.
    /// </summary>
    /// <returns>Notizen mit einer erkannten externen Änderung.</returns>
    public IReadOnlyList<NoteDocument> GetExternallyChangedNotes()
    {
        // Die Dateisystemprüfung bleibt vollständig hinter dem Repository-Interface verborgen.
        return _loadedNotes
            .Where(_vaultRepository.HasChangedSinceLoaded)
            .ToList();
    }

    /// <summary>
    /// Lädt eine Notiz erneut aus dem Dateisystem und ersetzt ihren Arbeitsspeicherstand.
    /// </summary>
    /// <param name="note">Die erneut zu ladende Notiz.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Der frisch geladene Notizstand.</returns>
    /// <exception cref="FileNotFoundException">Wenn die Datei nicht mehr existiert.</exception>
    public async Task<NoteDocument> ReloadNoteAsync(
        NoteDocument note,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (CurrentVault is null)
        {
            throw new InvalidOperationException("Es ist kein Vault geöffnet.");
        }

        // Der Repository-Scan verwendet dieselben Lade- und Parserregeln wie das normale Öffnen eines Vaults.
        IReadOnlyList<NoteDocument> refreshedNotes =
            await _vaultRepository.LoadAllNotesAsync(CurrentVault.RootPath, cancellationToken);

        NoteDocument? refreshedNote = refreshedNotes.FirstOrDefault(candidate =>
            string.Equals(candidate.FullPath, note.FullPath, StringComparison.OrdinalIgnoreCase));

        if (refreshedNote is null)
        {
            throw new FileNotFoundException("Die Markdown-Datei existiert nicht mehr.", note.FullPath);
        }

        int noteIndex = _loadedNotes.FindIndex(candidate =>
            string.Equals(candidate.FullPath, note.FullPath, StringComparison.OrdinalIgnoreCase));

        if (noteIndex >= 0)
        {
            _loadedNotes[noteIndex] = refreshedNote;
        }
        else
        {
            _loadedNotes.Add(refreshedNote);
            SortNotes();
        }

        return refreshedNote;
    }

    /// <summary>
    /// Ermittelt die Gliederung einer Notiz.
    /// </summary>
    public IReadOnlyList<OutlineItem> GetOutline(NoteDocument note)
    {
        return _outlineParser.Parse(note.Content);
    }

    /// <summary>
    /// Versucht, einen Wiki-Link an der Cursorposition oder aus dem ausgewählten Text aufzulösen.
    /// </summary>
    public NoteDocument? TryResolveLink(NoteDocument note, int caretPosition, string? selectedText)
    {
        IReadOnlyList<WikiLink> links = _wikiLinkParser.Parse(note.Content);

        if (!string.IsNullOrWhiteSpace(selectedText) && selectedText.StartsWith("[[") && selectedText.EndsWith("]]"))
        {
            links = _wikiLinkParser.Parse(selectedText);
            WikiLink? selectedLink = links.FirstOrDefault();
            return selectedLink is null ? null : _linkResolver.Resolve(selectedLink.Target, ReplaceOrInjectSnapshot(note));
        }

        WikiLink? linkAtPosition = links.FirstOrDefault(link =>
            caretPosition >= link.StartIndex && caretPosition < link.StartIndex + link.Length);
        return linkAtPosition is null ? null : _linkResolver.Resolve(linkAtPosition.Target, ReplaceOrInjectSnapshot(note));
    }

    /// <summary>
    /// Sucht eine Notiz anhand ihres vollständigen Dateipfads.
    /// </summary>
    public NoteDocument? FindNoteByFullPath(string fullPath)
    {
        return _loadedNotes.FirstOrDefault(note =>
            string.Equals(note.FullPath, fullPath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Lädt den Arbeitszustand für den aktuell bearbeiteten Editortext neu in das Modell ein.
    /// </summary>
    public void UpdateWorkingContent(NoteDocument note, string workingContent)
    {
        note.Content = workingContent;
        note.Links = _wikiLinkParser.Parse(workingContent);
        note.IsDirty = true;
    }

    /// <summary>
    /// Liefert den relativen Ordnerpfad für den angegebenen Baumknoten.
    /// </summary>
    public string? GetRelativeFolderPathForTreeItem(VaultTreeItem? item)
    {
        if (item is null)
        {
            return null;
        }

        return item.ItemType switch
        {
            TreeItemType.Root => null,
            TreeItemType.Folder => item.RelativePath,
            TreeItemType.Note => Path.GetDirectoryName(item.RelativePath)?.Replace('\\', '/'),
            _ => null
        };
    }

    /// <summary>
    /// Bereitet neue Recent-Folder-Einstellungen vor, ohne den aktuell gültigen Stand zu verändern.
    /// </summary>
    /// <param name="vaultPath">Vollständiger Pfad des erfolgreich eingelesenen Vault-Kandidaten.</param>
    /// <returns>Ein unabhängiger Einstellungskandidat für den bevorstehenden Wechsel.</returns>
    private AppSettings CreateSettingsWithRecentFolder(string vaultPath)
    {
        var recentFolders = new List<RecentFolderEntry>
        {
            new()
            {
                Path = vaultPath,
                LastOpenedUtc = DateTime.UtcNow
            }
        };

        // Eigene Einträge verhindern, dass ein fehlgeschlagener Speicherversuch die aktiven Einstellungen verändert.
        recentFolders.AddRange(_settings.RecentFolders
            .Where(item => !string.Equals(item.Path, vaultPath, StringComparison.OrdinalIgnoreCase))
            .Take(9)
            .Select(item => new RecentFolderEntry
            {
                Path = item.Path,
                LastOpenedUtc = item.LastOpenedUtc
            }));

        return new AppSettings
        {
            LastOpenedVaultPath = vaultPath,
            RecentFolders = recentFolders
        };
    }

    /// <summary>
    /// Sortiert die geladenen Notizen nach relativem Pfad.
    /// </summary>
    private void SortNotes()
    {
        _loadedNotes.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.RelativePath, right.RelativePath));
    }

    /// <summary>
    /// Erzeugt eine bearbeitbare Schnappschuss-Notiz für Auswertungen auf dem aktuellen Editorzustand.
    /// </summary>
    private NoteDocument CreateWorkingSnapshot(NoteDocument note)
    {
        return new NoteDocument
        {
            Id = note.Id,
            Title = note.Title,
            FullPath = note.FullPath,
            RelativePath = note.RelativePath,
            Content = note.Content,
            Links = _wikiLinkParser.Parse(note.Content),
            LastModifiedUtc = note.LastModifiedUtc,
            IsDirty = note.IsDirty
        };
    }

    /// <summary>
    /// Erstellt eine Arbeitsliste, in der die aktuelle Notiz durch ihren neuesten Stand ersetzt ist.
    /// </summary>
    private IReadOnlyList<NoteDocument> ReplaceOrInjectSnapshot(NoteDocument note)
    {
        var notes = _loadedNotes.Select(existing =>
            string.Equals(existing.FullPath, note.FullPath, StringComparison.OrdinalIgnoreCase)
                ? CreateWorkingSnapshot(note)
                : existing).ToList();

        if (!notes.Any(existing => string.Equals(existing.FullPath, note.FullPath, StringComparison.OrdinalIgnoreCase)))
        {
            notes.Add(CreateWorkingSnapshot(note));
        }

        return notes;
    }
}

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
    /// Öffnet einen Vault und lädt alle Markdown-Dateien in den Arbeitsspeicher.
    /// </summary>
    public async Task<VaultInfo> OpenVaultAsync(string vaultPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultPath))
        {
            throw new ArgumentException("Der Vault-Pfad darf nicht leer sein.", nameof(vaultPath));
        }

        if (!Directory.Exists(vaultPath))
        {
            throw new DirectoryNotFoundException($"Der Vault-Ordner wurde nicht gefunden: {vaultPath}");
        }

        var vaultInfo = new VaultInfo
        {
            DisplayName = Path.GetFileName(vaultPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            RootPath = vaultPath
        };

        IReadOnlyList<NoteDocument> notes = await _vaultRepository.LoadAllNotesAsync(vaultPath, cancellationToken);

        // Der neue Zustand wird erst übernommen, wenn das Laden erfolgreich abgeschlossen wurde.
        _loadedNotes.Clear();
        _loadedNotes.AddRange(notes.OrderBy(note => note.RelativePath, StringComparer.OrdinalIgnoreCase));
        CurrentVault = vaultInfo;

        await RememberRecentFolderAsync(vaultPath, cancellationToken);
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

        foreach (NoteDocument note in _loadedNotes)
        {
            string normalizedRelativePath = note.RelativePath.Replace('\\', '/');
            string[] segments = normalizedRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Aus dem relativen Notizpfad werden zunächst alle Zwischenordner ermittelt.
            if (segments.Length > 1)
            {
                string currentFolder = string.Empty;
                for (int index = 0; index < segments.Length - 1; index++)
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
                        FullPath = Path.Combine(CurrentVault.RootPath, currentFolder.Replace('/', Path.DirectorySeparatorChar)),
                        RelativePath = currentFolder,
                        ItemType = TreeItemType.Folder
                    });
                }
            }

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
    /// Speichert eine Notiz, aktualisiert erkannte Links und den In-Memory-Zustand.
    /// </summary>
    public async Task SaveNoteAsync(NoteDocument note, string updatedContent, CancellationToken cancellationToken = default)
    {
        note.Content = updatedContent;
        note.Links = _wikiLinkParser.Parse(updatedContent);
        note.IsDirty = false;

        await _vaultRepository.SaveNoteAsync(note, cancellationToken);
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

        WikiLink? linkAtPosition = links.FirstOrDefault(link => caretPosition >= link.StartIndex && caretPosition <= link.StartIndex + link.Length);
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
    /// Merkt sich einen Vault in der Liste der zuletzt verwendeten Ordner.
    /// </summary>
    private async Task RememberRecentFolderAsync(string vaultPath, CancellationToken cancellationToken)
    {
        string normalizedPath = Path.GetFullPath(vaultPath);
        DateTime nowUtc = DateTime.UtcNow;

        _settings.RecentFolders = _settings.RecentFolders
            .Where(item => !string.Equals(item.Path, normalizedPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _settings.RecentFolders.Insert(0, new RecentFolderEntry
        {
            Path = normalizedPath,
            LastOpenedUtc = nowUtc
        });

        _settings.RecentFolders = _settings.RecentFolders.Take(10).ToList();
        _settings.LastOpenedVaultPath = normalizedPath;

        await _settingsRepository.SaveAsync(_settings, cancellationToken);
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

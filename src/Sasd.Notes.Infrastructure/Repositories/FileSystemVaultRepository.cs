using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Infrastructure.Repositories;

/// <summary>
/// Dateisystembasierte Implementierung des Vault-Repositories.
/// </summary>
public sealed class FileSystemVaultRepository : IVaultRepository
{
    private readonly IWikiLinkParser _wikiLinkParser;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="FileSystemVaultRepository"/>-Klasse.
    /// </summary>
    /// <param name="wikiLinkParser">Parser zur Extraktion von Wiki-Links.</param>
    public FileSystemVaultRepository(IWikiLinkParser wikiLinkParser)
    {
        _wikiLinkParser = wikiLinkParser;
    }

    /// <inheritdoc />
    public bool VaultExists(string vaultPath)
    {
        return !string.IsNullOrWhiteSpace(vaultPath) && Directory.Exists(Path.GetFullPath(vaultPath));
    }

    /// <inheritdoc />
    public IReadOnlyList<string> LoadFolderPaths(string vaultPath)
    {
        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        if (!Directory.Exists(normalizedVaultPath))
        {
            throw new DirectoryNotFoundException($"Der Vault-Ordner wurde nicht gefunden: {vaultPath}");
        }

        // Leere Ordner müssen ebenfalls im Navigationsbaum erscheinen und werden deshalb unabhängig von Notizen geladen.
        return Directory.GetDirectories(normalizedVaultPath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(normalizedVaultPath, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
    {
        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        string[] files = Directory.GetFiles(normalizedVaultPath, "*.md", SearchOption.AllDirectories);
        var notes = new List<NoteDocument>(files.Length);

        foreach (string file in files)
        {
            // Bei größeren Vaults soll das Laden trotzdem sauber abbrechbar bleiben.
            cancellationToken.ThrowIfCancellationRequested();

            notes.Add(await ReadNoteFileAsync(normalizedVaultPath, file, cancellationToken));
        }

        return notes;
    }

    /// <inheritdoc />
    public async Task<NoteDocument> LoadNoteAsync(
        string vaultPath,
        string fullNotePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(vaultPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullNotePath);

        if (!Path.IsPathFullyQualified(fullNotePath))
        {
            throw new ArgumentException("Der Notizpfad muss vollständig angegeben werden.", nameof(fullNotePath));
        }

        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        string normalizedNotePath = Path.GetFullPath(fullNotePath);

        // Einzelzugriffe dürfen dieselben Vault-Grenzen wie Schreibzugriffe nicht verlassen.
        EnsurePathInsideVault(normalizedVaultPath, normalizedNotePath);
        if (!string.Equals(Path.GetExtension(normalizedNotePath), ".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Es dürfen nur Markdown-Dateien geladen werden.");
        }

        if (!File.Exists(normalizedNotePath))
        {
            throw new FileNotFoundException("Die Markdown-Datei wurde nicht gefunden.", normalizedNotePath);
        }

        return await ReadNoteFileAsync(normalizedVaultPath, normalizedNotePath, cancellationToken);
    }

    /// <summary>
    /// Verwendet beim Vault-Scan und Einzelzugriff dieselben Regeln für Text, Titel, Links und Zeitstempel.
    /// </summary>
    private async Task<NoteDocument> ReadNoteFileAsync(
        string vaultPath,
        string filePath,
        CancellationToken cancellationToken)
    {
        string content = await File.ReadAllTextAsync(filePath, cancellationToken);
        string relativePath = Path.GetRelativePath(vaultPath, filePath);

        return new NoteDocument
        {
            Id = relativePath.Replace('\\', '/'),
            Title = Path.GetFileNameWithoutExtension(filePath),
            FullPath = filePath,
            RelativePath = relativePath,
            Content = content,
            Links = _wikiLinkParser.Parse(content),
            LastModifiedUtc = File.GetLastWriteTimeUtc(filePath),
            IsDirty = false
        };
    }

    /// <inheritdoc />
    public async Task SaveNoteAsync(string vaultPath, NoteDocument note, CancellationToken cancellationToken = default)
    {
        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        string normalizedNotePath = Path.GetFullPath(note.FullPath);

        // Auch bei intern erzeugten Modellen wird der Schreibpfad gegen den geöffneten Vault geprüft.
        EnsurePathInsideVault(normalizedVaultPath, normalizedNotePath);

        if (!string.Equals(Path.GetExtension(normalizedNotePath), ".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Es dürfen nur Markdown-Dateien gespeichert werden.");
        }

        if (!File.Exists(normalizedNotePath))
        {
            throw new FileNotFoundException("Die zu speichernde Markdown-Datei wurde nicht gefunden.", normalizedNotePath);
        }

        // Verhindert, dass Änderungen eines anderen Editors stillschweigend überschrieben werden.
        DateTime currentLastModifiedUtc = File.GetLastWriteTimeUtc(normalizedNotePath);
        if (note.LastModifiedUtc != default && currentLastModifiedUtc != note.LastModifiedUtc)
        {
            throw new IOException(
                "Die Markdown-Datei wurde seit dem Laden extern geändert. " +
                "Bitte laden Sie den Vault neu und prüfen Sie die Änderungen.");
        }

        string? directory = Path.GetDirectoryName(normalizedNotePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await WriteTextAtomicallyAsync(normalizedNotePath, note.Content, replaceExisting: true, cancellationToken: cancellationToken);
        note.LastModifiedUtc = File.GetLastWriteTimeUtc(normalizedNotePath);
    }

    /// <inheritdoc />
    public bool HasChangedSinceLoaded(NoteDocument note)
    {
        ArgumentNullException.ThrowIfNull(note);

        // Eine gelöschte Datei gilt ebenfalls als externe Änderung.
        if (!File.Exists(note.FullPath))
        {
            return true;
        }

        DateTime currentLastModifiedUtc = File.GetLastWriteTimeUtc(note.FullPath);
        return note.LastModifiedUtc != default && currentLastModifiedUtc != note.LastModifiedUtc;
    }

    /// <inheritdoc />
    public Task<string> CreateFolderAsync(
        string vaultPath,
        string? relativeParentPath,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new ArgumentException("Der Ordnername darf nicht leer sein.", nameof(folderName));
        }

        string safeFolderName = FileNameHelper.ToSafeFileName(folderName);
        if (safeFolderName is "." or "..")
        {
            throw new ArgumentException("Der Ordnername ist nicht zulässig.", nameof(folderName));
        }

        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        string parentPath = string.IsNullOrWhiteSpace(relativeParentPath)
            ? normalizedVaultPath
            : Path.GetFullPath(Path.Combine(
                normalizedVaultPath,
                relativeParentPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)));

        // Sowohl der übergeordnete Ordner als auch der neue Zielordner müssen im Vault liegen.
        EnsurePathInsideVault(normalizedVaultPath, parentPath);
        if (!Directory.Exists(parentPath))
        {
            throw new DirectoryNotFoundException("Der übergeordnete Ordner wurde nicht gefunden.");
        }

        string targetPath = Path.Combine(parentPath, safeFolderName);
        EnsurePathInsideVault(normalizedVaultPath, targetPath);

        if (Directory.Exists(targetPath) || File.Exists(targetPath))
        {
            throw new IOException($"Der Ordner existiert bereits: {safeFolderName}");
        }

        // Der Ordner wird erst nach allen Sicherheitsprüfungen angelegt.
        Directory.CreateDirectory(targetPath);

        string relativePath = Path.GetRelativePath(normalizedVaultPath, targetPath).Replace('\\', '/');
        return Task.FromResult(relativePath);
    }

    /// <inheritdoc />
    public async Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default)
    {
        // Ein bereits abgebrochener Auftrag darf auch keine neuen Zielordner anlegen.
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        string safeTitle = FileNameHelper.ToSafeFileName(title);
        string fileName = safeTitle.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? safeTitle
            : safeTitle + ".md";

        string fullFolderPath = string.IsNullOrWhiteSpace(relativeFolderPath)
            ? normalizedVaultPath
            : Path.GetFullPath(Path.Combine(
                normalizedVaultPath,
                relativeFolderPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)));

        // Relative Ordner dürfen den ausgewählten Vault nicht verlassen.
        EnsurePathInsideVault(normalizedVaultPath, fullFolderPath);

        Directory.CreateDirectory(fullFolderPath);

        // Ein freier Name ist nur ein Vorschlag: Ein anderer Vorgang kann ihn vor dem Commit belegen.
        // Wiederholt sich die Kollision zu oft, wird der letzte Dateifehler kontrolliert weitergereicht.
        const int maximumCollisionRetries = 32;
        for (int attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string fullPath = EnsureUniqueFilePath(fullFolderPath, fileName);
            string relativePath = Path.GetRelativePath(normalizedVaultPath, fullPath);
            string initialContent = $"# {Path.GetFileNameWithoutExtension(fullPath)}{Environment.NewLine}{Environment.NewLine}";

            try
            {
                // Neuerzeugung darf niemals den Ersetzungspfad vorhandener Dateien verwenden.
                await WriteTextAtomicallyAsync(fullPath, initialContent, replaceExisting: false, cancellationToken: cancellationToken);
            }
            catch (IOException) when (
                attempt < maximumCollisionRetries &&
                (File.Exists(fullPath) || Directory.Exists(fullPath)))
            {
                // Der Namenskonflikt wird mit einem neuen Kandidaten aufgelöst; fremde Inhalte bleiben erhalten.
                continue;
            }

            return new NoteDocument
            {
                Id = relativePath.Replace('\\', '/'),
                Title = Path.GetFileNameWithoutExtension(fullPath),
                FullPath = fullPath,
                RelativePath = relativePath,
                Content = initialContent,
                Links = _wikiLinkParser.Parse(initialContent),
                LastModifiedUtc = File.GetLastWriteTimeUtc(fullPath),
                IsDirty = false
            };
        }
    }

    /// <summary>
    /// Erzeugt einen eindeutigen Dateinamen, falls bereits eine gleichnamige Datei existiert.
    /// </summary>
    private static string EnsureUniqueFilePath(string folderPath, string fileName)
    {
        string candidatePath = Path.Combine(folderPath, fileName);
        if (!File.Exists(candidatePath) && !Directory.Exists(candidatePath))
        {
            return candidatePath;
        }

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int suffix = 2;

        while (true)
        {
            string nextCandidatePath = Path.Combine(folderPath, $"{baseName} {suffix}{extension}");
            if (!File.Exists(nextCandidatePath) && !Directory.Exists(nextCandidatePath))
            {
                return nextCandidatePath;
            }

            suffix++;
        }
    }

    /// <summary>
    /// Schreibt Text über eine temporäre Datei und veröffentlicht ihn gemäß dem gewählten Schreibmodus.
    /// </summary>
    /// <param name="targetPath">Endgültiger Pfad der Datei.</param>
    /// <param name="content">Zu speichernder Inhalt.</param>
    /// <param name="replaceExisting">Ob eine vorhandene Datei ersetzt werden soll; Neuerzeugung überschreibt niemals.</param>
    /// <param name="cancellationToken">Token zum kontrollierten Abbrechen.</param>
    private static async Task WriteTextAtomicallyAsync(
        string targetPath,
        string content,
        bool replaceExisting,
        CancellationToken cancellationToken)
    {
        string temporaryPath = targetPath + ".sasd-tmp-" + Guid.NewGuid().ToString("N");

        try
        {
            // Die temporäre Datei liegt im selben Verzeichnis, damit der spätere Austausch innerhalb desselben Dateisystems bleibt.
            await using (FileStream stream = new(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.SequentialScan | FileOptions.WriteThrough))
            await using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                await writer.WriteAsync(content.AsMemory(), cancellationToken);
                await writer.FlushAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            // Vor der Veröffentlichung wird ein während des Schreibens eingegangener Abbruch berücksichtigt.
            cancellationToken.ThrowIfCancellationRequested();

            if (!replaceExisting)
            {
                // Move ohne Overwrite entscheidet den Namenskonflikt atomar, auch wenn die Datei inzwischen existiert.
                File.Move(temporaryPath, targetPath, overwrite: false);
            }
            else
            {
                try
                {
                    // Eine zwischenzeitlich gelöschte Notiz darf beim Speichern nicht still neu erzeugt werden.
                    File.Replace(temporaryPath, targetPath, destinationBackupFileName: null, ignoreMetadataErrors: true);
                }
                catch (PlatformNotSupportedException)
                {
                    // Der Kompatibilitätspfad ist ausschließlich für ausdrücklich angefordertes Ersetzen zulässig.
                    File.Move(temporaryPath, targetPath, overwrite: true);
                }
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    /// <summary>
    /// Prüft, ob ein Pfad innerhalb eines Vault-Verzeichnisses liegt.
    /// </summary>
    private static void EnsurePathInsideVault(string vaultPath, string candidatePath)
    {
        string relativePath = Path.GetRelativePath(vaultPath, candidatePath);
        bool escapesVault = relativePath.Equals("..", StringComparison.Ordinal) ||
                            relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                            Path.IsPathRooted(relativePath);

        if (escapesVault)
        {
            throw new InvalidOperationException("Der Dateipfad liegt außerhalb des geöffneten Vaults.");
        }
    }
}

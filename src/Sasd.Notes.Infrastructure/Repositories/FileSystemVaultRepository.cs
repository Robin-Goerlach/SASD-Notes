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
    public async Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
    {
        string normalizedVaultPath = Path.GetFullPath(vaultPath);
        string[] files = Directory.GetFiles(normalizedVaultPath, "*.md", SearchOption.AllDirectories);
        var notes = new List<NoteDocument>(files.Length);

        foreach (string file in files)
        {
            // Bei größeren Vaults soll das Laden trotzdem sauber abbrechbar bleiben.
            cancellationToken.ThrowIfCancellationRequested();

            string content = await File.ReadAllTextAsync(file, cancellationToken);
            string relativePath = Path.GetRelativePath(normalizedVaultPath, file);
            string title = Path.GetFileNameWithoutExtension(file);

            notes.Add(new NoteDocument
            {
                Id = relativePath.Replace('\\', '/'),
                Title = title,
                FullPath = file,
                RelativePath = relativePath,
                Content = content,
                Links = _wikiLinkParser.Parse(content),
                LastModifiedUtc = File.GetLastWriteTimeUtc(file),
                IsDirty = false
            });
        }

        return notes;
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

        await WriteTextAtomicallyAsync(normalizedNotePath, note.Content, cancellationToken);
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
    public async Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default)
    {
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

        string fullPath = EnsureUniqueFilePath(fullFolderPath, fileName);
        string relativePath = Path.GetRelativePath(normalizedVaultPath, fullPath);
        string initialContent = $"# {Path.GetFileNameWithoutExtension(fullPath)}{Environment.NewLine}{Environment.NewLine}";

        await WriteTextAtomicallyAsync(fullPath, initialContent, cancellationToken);

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

    /// <summary>
    /// Erzeugt einen eindeutigen Dateinamen, falls bereits eine gleichnamige Datei existiert.
    /// </summary>
    private static string EnsureUniqueFilePath(string folderPath, string fileName)
    {
        string candidatePath = Path.Combine(folderPath, fileName);
        if (!File.Exists(candidatePath))
        {
            return candidatePath;
        }

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int suffix = 2;

        while (true)
        {
            string nextCandidatePath = Path.Combine(folderPath, $"{baseName} {suffix}{extension}");
            if (!File.Exists(nextCandidatePath))
            {
                return nextCandidatePath;
            }

            suffix++;
        }
    }

    /// <summary>
    /// Schreibt Text über eine temporäre Datei und ersetzt anschließend das Ziel.
    /// </summary>
    /// <param name="targetPath">Endgültiger Pfad der Datei.</param>
    /// <param name="content">Zu speichernder Inhalt.</param>
    /// <param name="cancellationToken">Token zum kontrollierten Abbrechen.</param>
    private static async Task WriteTextAtomicallyAsync(
        string targetPath,
        string content,
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

            // Der Austausch erfolgt erst, wenn der vollständige neue Inhalt sicher geschrieben wurde.
            if (File.Exists(targetPath))
            {
                File.Replace(temporaryPath, targetPath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, targetPath);
            }
        }
        catch (PlatformNotSupportedException)
        {
            // File.Replace ist auf einzelnen Dateisystemen nicht verfügbar; Move ersetzt dort ebenfalls innerhalb des Zielverzeichnisses.
            File.Move(temporaryPath, targetPath, overwrite: true);
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

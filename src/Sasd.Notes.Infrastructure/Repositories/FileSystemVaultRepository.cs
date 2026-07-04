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
    public async Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default)
    {
        string[] files = Directory.GetFiles(vaultPath, "*.md", SearchOption.AllDirectories);
        var notes = new List<NoteDocument>(files.Length);

        foreach (string file in files)
        {
            // Bei größeren Vaults soll das Laden trotzdem sauber abbrechbar bleiben.
            cancellationToken.ThrowIfCancellationRequested();

            string content = await File.ReadAllTextAsync(file, cancellationToken);
            string relativePath = Path.GetRelativePath(vaultPath, file);
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
    public async Task SaveNoteAsync(NoteDocument note, CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(note.FullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(note.FullPath, note.Content, cancellationToken);
        note.LastModifiedUtc = File.GetLastWriteTimeUtc(note.FullPath);
    }

    /// <inheritdoc />
    public async Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default)
    {
        string safeTitle = FileNameHelper.ToSafeFileName(title);
        string fileName = safeTitle.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? safeTitle
            : safeTitle + ".md";

        string fullFolderPath = string.IsNullOrWhiteSpace(relativeFolderPath)
            ? vaultPath
            : Path.Combine(vaultPath, relativeFolderPath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(fullFolderPath);

        string fullPath = EnsureUniqueFilePath(fullFolderPath, fileName);
        string relativePath = Path.GetRelativePath(vaultPath, fullPath);
        string initialContent = $"# {Path.GetFileNameWithoutExtension(fullPath)}{Environment.NewLine}{Environment.NewLine}";

        await File.WriteAllTextAsync(fullPath, initialContent, cancellationToken);

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
}

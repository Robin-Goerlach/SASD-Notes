using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Interfaces;

/// <summary>
/// Beschreibt den Zugriff auf Notizen und Ordner eines Vaults.
/// </summary>
public interface IVaultRepository
{
    /// <summary>
    /// Prüft, ob ein möglicher Vault-Ordner vorhanden ist.
    /// </summary>
    /// <param name="vaultPath">Zu prüfender Vault-Pfad.</param>
    /// <returns><see langword="true"/>, wenn der Ordner vorhanden ist.</returns>
    bool VaultExists(string vaultPath);

    /// <summary>
    /// Lädt alle Markdown-Notizen eines Vaults rekursiv.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Alle gefundenen Notizen.</returns>
    Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lädt genau eine Markdown-Notiz innerhalb des angegebenen Vaults.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <param name="fullNotePath">Vollständiger Pfad der zu lesenden Markdown-Datei.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Ein frisch geladenes Notizmodell ohne lokale Änderungen.</returns>
    /// <exception cref="InvalidOperationException">Der Pfad liegt außerhalb des Vaults oder bezeichnet keine Markdown-Datei.</exception>
    /// <exception cref="FileNotFoundException">Die angeforderte Datei wurde nicht gefunden.</exception>
    Task<NoteDocument> LoadNoteAsync(
        string vaultPath,
        string fullNotePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Speichert eine Notiz zurück in das Dateisystem.
    /// </summary>
    /// <param name="vaultPath">Pfad zum geöffneten Vault-Wurzelverzeichnis.</param>
    /// <param name="note">Zu speichernde Notiz.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    Task SaveNoteAsync(string vaultPath, NoteDocument note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prüft, ob sich eine geladene Notiz seit ihrem Einlesen im Dateisystem verändert hat.
    /// </summary>
    /// <param name="note">Zu prüfende geladene Notiz.</param>
    /// <returns><see langword="true"/>, wenn die Datei geändert oder gelöscht wurde.</returns>
    bool HasChangedSinceLoaded(NoteDocument note);

    /// <summary>
    /// Erstellt eine neue Markdown-Notiz in einem Zielordner.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <param name="relativeFolderPath">Relativer Zielordner oder <see langword="null"/>.</param>
    /// <param name="title">Titel der neuen Notiz.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Die neu erstellte Notiz.</returns>
    Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default);

    /// <summary>
    /// Erstellt einen neuen Unterordner innerhalb des Vaults.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <param name="relativeParentPath">Relativer übergeordneter Ordner oder <see langword="null"/>.</param>
    /// <param name="folderName">Name des neuen Ordners.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Der relative Pfad des neu erstellten Ordners.</returns>
    Task<string> CreateFolderAsync(
        string vaultPath,
        string? relativeParentPath,
        string folderName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lädt die vorhandenen Unterordner eines Vaults rekursiv.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <returns>Relative Unterordnerpfade in konsistenter Schrägstrich-Schreibweise.</returns>
    IReadOnlyList<string> LoadFolderPaths(string vaultPath);
}

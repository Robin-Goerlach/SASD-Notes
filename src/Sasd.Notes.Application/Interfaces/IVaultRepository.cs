using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Interfaces;

/// <summary>
/// Beschreibt den Zugriff auf Notizen und Ordner eines Vaults.
/// </summary>
public interface IVaultRepository
{
    /// <summary>
    /// Lädt alle Markdown-Notizen eines Vaults rekursiv.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Alle gefundenen Notizen.</returns>
    Task<IReadOnlyList<NoteDocument>> LoadAllNotesAsync(string vaultPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Speichert eine Notiz zurück in das Dateisystem.
    /// </summary>
    /// <param name="note">Zu speichernde Notiz.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    Task SaveNoteAsync(NoteDocument note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Erstellt eine neue Markdown-Notiz in einem Zielordner.
    /// </summary>
    /// <param name="vaultPath">Pfad zum Vault-Wurzelverzeichnis.</param>
    /// <param name="relativeFolderPath">Relativer Zielordner oder <see langword="null"/>.</param>
    /// <param name="title">Titel der neuen Notiz.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Die neu erstellte Notiz.</returns>
    Task<NoteDocument> CreateNoteAsync(string vaultPath, string? relativeFolderPath, string title, CancellationToken cancellationToken = default);
}

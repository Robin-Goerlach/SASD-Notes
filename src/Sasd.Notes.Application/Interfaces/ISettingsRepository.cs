using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Interfaces;

/// <summary>
/// Beschreibt den Zugriff auf persistente Benutzereinstellungen.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Lädt die Benutzereinstellungen.
    /// </summary>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Geladene Einstellungen oder Standardwerte.</returns>
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Speichert die Benutzereinstellungen.
    /// </summary>
    /// <param name="settings">Zu speichernde Einstellungen.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}

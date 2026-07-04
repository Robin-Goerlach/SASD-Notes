namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Beschreibt einen kürzlich geöffneten Vault-Ordner.
/// </summary>
public sealed class RecentFolderEntry
{
    /// <summary>
    /// Vollständiger Pfad des Vaults.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Zeitpunkt der letzten Öffnung in UTC.
    /// </summary>
    public DateTime LastOpenedUtc { get; init; }
}

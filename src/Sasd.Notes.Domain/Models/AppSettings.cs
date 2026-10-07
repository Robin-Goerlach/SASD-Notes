namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Persistente Benutzereinstellungen der Anwendung.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Liste der zuletzt verwendeten Vaults.
    /// </summary>
    public List<RecentFolderEntry> RecentFolders { get; set; } = new();

    /// <summary>
    /// Pfad des zuletzt geöffneten Vaults.
    /// </summary>
    public string? LastOpenedVaultPath { get; set; }
}

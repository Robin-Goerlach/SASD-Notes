namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Beschreibt den aktuell geöffneten Vault.
/// </summary>
public sealed class VaultInfo
{
    /// <summary>
    /// Anzeigename des Vaults.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Vollständiger Dateisystempfad zum Vault-Wurzelverzeichnis.
    /// </summary>
    public required string RootPath { get; init; }
}

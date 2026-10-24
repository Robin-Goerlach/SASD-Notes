namespace Sasd.Notes.Application.Models;

/// <summary>
/// Beschreibt den ausdrücklich beim Programmstart angeforderten Vault und optional eine darin zu öffnende Notiz.
/// </summary>
public sealed class StartupTarget
{
    /// <summary>
    /// Vollständiger Pfad des zu öffnenden Vault-Wurzelverzeichnisses.
    /// </summary>
    public required string VaultPath { get; init; }

    /// <summary>
    /// Vollständiger Pfad der zu öffnenden Markdown-Datei oder <see langword="null"/> bei einem reinen Ordnerstart.
    /// </summary>
    public string? NotePath { get; init; }
}

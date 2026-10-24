using Sasd.Notes.Application.Models;

namespace Sasd.Notes.Infrastructure.Services;

/// <summary>
/// Prüft einen normalisierten Startpfad am Dateisystem und bestimmt den zu öffnenden Vault.
/// </summary>
public sealed class FileSystemStartupTargetResolver
{
    /// <summary>
    /// Ermittelt aus einem vorhandenen Ordner oder einer vorhandenen Markdown-Datei das Startziel.
    /// </summary>
    /// <param name="fullPath">Vollständiger Startpfad aus der Argumentauswertung.</param>
    /// <returns>Ordnerziel oder Dateiziel mit dem unmittelbar übergeordneten Verzeichnis als Vault.</returns>
    /// <exception cref="ArgumentException">Der Pfad ist nicht vollständig oder eine vorhandene Datei besitzt keine .md-Endung.</exception>
    /// <exception cref="FileNotFoundException">Der Startpfad existiert nicht.</exception>
    public StartupTarget Resolve(string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        if (!Path.IsPathFullyQualified(fullPath))
        {
            throw new ArgumentException("Der Startpfad muss vollständig angegeben werden.", nameof(fullPath));
        }

        string normalizedPath = Path.GetFullPath(fullPath);

        // Ein echter Ordner hat Vorrang vor seiner Endung: Auch ein Verzeichnis namens Wissen.md ist ein Vault.
        if (Directory.Exists(normalizedPath))
        {
            return new StartupTarget { VaultPath = Path.TrimEndingDirectorySeparator(normalizedPath) };
        }

        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException("Der angegebene Startpfad wurde nicht gefunden.", normalizedPath);
        }

        if (!string.Equals(Path.GetExtension(normalizedPath), ".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Als Startdatei ist nur eine Markdown-Datei mit der Endung .md zulässig.", nameof(fullPath));
        }

        // Ohne explizite Vault-Kennung wird kein höheres Verzeichnis erraten.
        string parentPath = Path.GetDirectoryName(normalizedPath)
            ?? throw new ArgumentException("Der übergeordnete Ordner der Markdown-Datei konnte nicht ermittelt werden.", nameof(fullPath));
        return new StartupTarget { VaultPath = parentPath, NotePath = normalizedPath };
    }
}

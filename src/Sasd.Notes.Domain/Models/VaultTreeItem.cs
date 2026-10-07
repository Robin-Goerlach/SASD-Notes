namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Fachliches Modell für einen Knoten im Navigationsbaum.
/// </summary>
public sealed class VaultTreeItem
{
    /// <summary>
    /// Anzeigetext im Baum.
    /// </summary>
    public required string DisplayText { get; init; }

    /// <summary>
    /// Vollständiger Dateisystempfad des Knotens.
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>
    /// Relativer Pfad des Knotens bezogen auf den aktuellen Vault.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Typ des Knotens.
    /// </summary>
    public TreeItemType ItemType { get; init; }
}

namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Kennzeichnet die Art eines Knotens im Vault-Baum.
/// </summary>
public enum TreeItemType
{
    /// <summary>
    /// Der Wurzelknoten des geöffneten Vaults.
    /// </summary>
    Root,

    /// <summary>
    /// Ein Ordner innerhalb des Vaults.
    /// </summary>
    Folder,

    /// <summary>
    /// Eine Markdown-Datei innerhalb des Vaults.
    /// </summary>
    Note
}

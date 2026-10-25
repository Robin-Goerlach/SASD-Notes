namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Kennzeichnet die Bereiche einer Notiz, in denen eine Suchanfrage gefunden wurde.
/// </summary>
/// <remarks>
/// Mehrere Werte können kombiniert sein, wenn derselbe Suchbegriff beispielsweise
/// sowohl im Titel als auch im Inhalt vorkommt.
/// </remarks>
[Flags]
public enum SearchMatchKind
{
    /// <summary>
    /// In keinem durchsuchten Bereich wurde ein Treffer festgestellt.
    /// </summary>
    None = 0,

    /// <summary>
    /// Der Suchbegriff kommt im fachlichen Notiztitel vor.
    /// </summary>
    Title = 1,

    /// <summary>
    /// Der Suchbegriff kommt im relativen Vault-Pfad vor.
    /// </summary>
    RelativePath = 2,

    /// <summary>
    /// Der Suchbegriff kommt im Markdown-Inhalt vor.
    /// </summary>
    Content = 4
}

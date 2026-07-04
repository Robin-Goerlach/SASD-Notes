using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Stellt lesbare Textdarstellungen für ListBox-Einträge bereit.
/// </summary>
public static class ListItemFormatting
{
    /// <summary>
    /// Konfiguriert globale Anzeigeereignisse der wichtigsten Domänenmodelle.
    /// </summary>
    public static void Register()
    {
        // Diese Klasse ist als optionale Erweiterungsstelle vorgesehen.
        // In der aktuellen V1 werden die Darstellungen direkt über ToString-Überschreibungen bereitgestellt.
    }
}

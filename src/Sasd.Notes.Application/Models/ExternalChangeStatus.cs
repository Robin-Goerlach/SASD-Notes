namespace Sasd.Notes.Application.Models;

/// <summary>
/// Ergebnis einer nicht verändernden Prüfung des externen Dateistands.
/// </summary>
public enum ExternalChangeStatus
{
    /// <summary>Die Datei wurde erfolgreich geprüft und ist unverändert.</summary>
    Unchanged,

    /// <summary>Die Datei wurde extern geändert oder ist nicht mehr vorhanden.</summary>
    Changed,

    /// <summary>Ein Zugriffs- oder E/A-Fehler verhindert derzeit eine verlässliche Prüfung.</summary>
    Unavailable,

    /// <summary>Der Workspace wechselt, speichert die Notiz oder enthält dieses Modell nicht mehr.</summary>
    NotChecked
}

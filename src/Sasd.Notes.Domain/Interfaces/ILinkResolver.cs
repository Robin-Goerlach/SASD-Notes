using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Domain.Interfaces;

/// <summary>
/// Beschreibt die Auflösung eines Wiki-Link-Ziels auf eine bekannte Notiz.
/// </summary>
public interface ILinkResolver
{
    /// <summary>
    /// Löst ein Linkziel auf eine vorhandene Notiz auf.
    /// </summary>
    /// <param name="target">Linkziel aus dem Wiki-Link.</param>
    /// <param name="allNotes">Alle bekannten Notizen im aktuellen Vault.</param>
    /// <returns>
    /// Die eindeutig aufgelöste Notiz oder <see langword="null"/> bei fehlendem, leerem oder mehrdeutigem Ziel.
    /// </returns>
    /// <remarks>
    /// Ein exakter relativer Vault-Pfad hat Vorrang. Ordnerangaben sind verbindlich;
    /// nur Ziele ohne Ordner dürfen auf einen eindeutigen Dateinamen zurückfallen.
    /// Die Endung .md ist optional, Groß-/Kleinschreibung wird nicht unterschieden.
    /// </remarks>
    NoteDocument? Resolve(string target, IReadOnlyList<NoteDocument> allNotes);
}

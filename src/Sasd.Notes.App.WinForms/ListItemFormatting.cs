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
        // Die Methode bleibt als zentraler Einstiegspunkt für spätere globale
        // Formatierungsregeln erhalten. Die Rechercheliste registriert ihr Ereignis lokal.
    }

    /// <summary>
    /// Formatiert einen ausgehenden Wiki-Link und kennzeichnet nicht aufgelöste Ziele.
    /// </summary>
    /// <param name="link">Fachlicher Linkeintrag aus der Application-Schicht.</param>
    /// <returns>Einzeilige Darstellung für die Linkliste.</returns>
    public static string FormatOutgoingLink(OutgoingLinkInfo link)
    {
        ArgumentNullException.ThrowIfNull(link);

        return link.IsResolved
            ? $"[Ziel] {link.DisplayText} — {link.ResolvedRelativePath}"
            : $"[Nicht aufgelöst] {link.DisplayText} — {link.Target}";
    }

    /// <summary>
    /// Formatiert einen Suchtreffer mit verständlichen deutschen Fundstellen-Kategorien.
    /// </summary>
    /// <param name="hit">Fachlicher Suchtreffer aus der Application-Schicht.</param>
    /// <returns>Einzeilige Darstellung für die Recherche-Trefferliste.</returns>
    public static string FormatSearchHit(SearchHit hit)
    {
        ArgumentNullException.ThrowIfNull(hit);

        var labels = new List<string>(3);
        if (hit.MatchKinds.HasFlag(SearchMatchKind.Title))
        {
            labels.Add("Titel");
        }
        if (hit.MatchKinds.HasFlag(SearchMatchKind.RelativePath))
        {
            labels.Add("Pfad");
        }
        if (hit.MatchKinds.HasFlag(SearchMatchKind.Content))
        {
            labels.Add("Inhalt");
        }

        // Ein von älteren Daten oder Tests erzeugter unklassifizierter Treffer
        // bleibt lesbar, ohne fälschlich eine konkrete Fundstelle zu behaupten.
        string category = labels.Count == 0 ? "Treffer" : string.Join(" + ", labels);
        return $"[{category}] {hit.NoteTitle} — {hit.RelativePath} — {hit.PreviewText}";
    }
}

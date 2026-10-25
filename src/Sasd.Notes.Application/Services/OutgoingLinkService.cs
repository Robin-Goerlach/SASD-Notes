using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Erstellt eine UI-unabhängige Übersicht der ausgehenden Wiki-Links einer Notiz.
/// </summary>
public sealed class OutgoingLinkService
{
    private readonly ILinkResolver _linkResolver;

    /// <summary>
    /// Initialisiert den Dienst mit der gemeinsamen fachlichen Linkauflösung.
    /// </summary>
    /// <param name="linkResolver">Resolver für Ziele innerhalb des aktuellen Vaults.</param>
    public OutgoingLinkService(ILinkResolver linkResolver)
    {
        _linkResolver = linkResolver ?? throw new ArgumentNullException(nameof(linkResolver));
    }

    /// <summary>
    /// Ermittelt ausgehende Links in ihrer ursprünglichen Dokumentreihenfolge.
    /// </summary>
    /// <param name="sourceNote">Notiz mit bereits geparsten Wiki-Links.</param>
    /// <param name="allNotes">Geladene Notizen, gegen die Ziele aufgelöst werden.</param>
    /// <returns>Ein Eintrag je erkanntem Linkvorkommen, einschließlich nicht aufgelöster Ziele.</returns>
    public IReadOnlyList<OutgoingLinkInfo> GetOutgoingLinks(
        NoteDocument sourceNote,
        IReadOnlyList<NoteDocument> allNotes)
    {
        ArgumentNullException.ThrowIfNull(sourceNote);
        ArgumentNullException.ThrowIfNull(allNotes);

        var result = new List<OutgoingLinkInfo>(sourceNote.Links.Count);
        foreach (WikiLink link in sourceNote.Links)
        {
            NoteDocument? targetNote = _linkResolver.Resolve(link.Target, allNotes);

            // Alias und Ziel bleiben getrennt: Die UI darf einen freundlichen Text zeigen,
            // während Navigation und Fehlermeldung weiterhin das tatsächliche Ziel kennen.
            string displayText = string.IsNullOrWhiteSpace(link.Alias)
                ? link.Target
                : link.Alias!;

            result.Add(new OutgoingLinkInfo
            {
                Target = link.Target,
                DisplayText = displayText,
                RawText = link.RawText,
                ResolvedRelativePath = targetNote?.RelativePath
            });
        }

        return result;
    }
}

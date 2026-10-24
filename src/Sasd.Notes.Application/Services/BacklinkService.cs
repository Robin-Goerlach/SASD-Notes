using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Ermittelt Rückverweise auf eine bestimmte Notiz.
/// </summary>
public sealed class BacklinkService
{
    private readonly ILinkResolver _linkResolver;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="BacklinkService"/>-Klasse.
    /// </summary>
    /// <param name="linkResolver">Dienst zur Auflösung von Wiki-Link-Zielen.</param>
    public BacklinkService(ILinkResolver linkResolver)
    {
        _linkResolver = linkResolver;
    }

    /// <summary>
    /// Ermittelt alle Rückverweise auf die angegebene Zielnotiz.
    /// </summary>
    /// <param name="targetNote">Zielnotiz, für die Rückverweise gesucht werden.</param>
    /// <param name="allNotes">Alle bekannten Notizen des aktuellen Vaults.</param>
    /// <returns>Ein Rückverweis je Quelldatei, sortiert nach relativem Pfad.</returns>
    /// <remarks>Mehrere Links derselben Quelle werden zusammengefasst. Als Kontext dient ihr erster aufgelöster Ziellink.</remarks>
    public IReadOnlyList<BacklinkInfo> GetBacklinks(NoteDocument targetNote, IReadOnlyList<NoteDocument> allNotes)
    {
        var result = new List<BacklinkInfo>();
        var includedSourcePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (NoteDocument note in allNotes)
        {
            // Die Liste zeigt Quelldateien, nicht einzelne Linkvorkommen. Selbstverweise und
            // bereits aufgenommene Quellen dürfen deshalb keinen weiteren Navigationseintrag erzeugen.
            if (string.Equals(note.FullPath, targetNote.FullPath, StringComparison.OrdinalIgnoreCase) ||
                includedSourcePaths.Contains(note.FullPath))
            {
                continue;
            }

            foreach (WikiLink link in note.Links)
            {
                NoteDocument? resolvedNote = _linkResolver.Resolve(link.Target, allNotes);
                if (resolvedNote is null)
                {
                    continue;
                }

                if (!string.Equals(resolvedNote.FullPath, targetNote.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(new BacklinkInfo
                {
                    SourceTitle = note.Title,
                    SourceRelativePath = note.RelativePath,
                    ContextSnippet = link.RawText
                });

                // Der erste gültige Link liefert den Kontext; weitere Verweise derselben Datei sind bereits vertreten.
                includedSourcePaths.Add(note.FullPath);
                break;
            }
        }

        return result
            .OrderBy(item => item.SourceRelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

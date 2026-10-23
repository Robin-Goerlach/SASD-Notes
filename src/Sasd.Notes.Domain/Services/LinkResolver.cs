using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Domain.Services;

/// <summary>
/// Löst Wiki-Link-Ziele anhand bekannter Notizen im aktuellen Vault auf.
/// </summary>
public sealed class LinkResolver : ILinkResolver
{
    /// <inheritdoc />
    public NoteDocument? Resolve(string target, IReadOnlyList<NoteDocument> allNotes)
    {
        ArgumentNullException.ThrowIfNull(allNotes);

        if (string.IsNullOrWhiteSpace(target))
        {
            return null;
        }

        // Ziel und Notizpfade erhalten dieselbe Behandlung, auch bei expliziter .md-Endung.
        string normalizedTarget = RemoveMarkdownExtension(NormalizePath(target));
        if (normalizedTarget.Length == 0)
        {
            return null;
        }

        List<NoteDocument> exactMatches = allNotes.Where(note =>
            string.Equals(
                RemoveMarkdownExtension(NormalizePath(note.RelativePath)),
                normalizedTarget,
                StringComparison.OrdinalIgnoreCase)).ToList();

        if (exactMatches.Count > 0)
        {
            // Auch nach Normalisierung darf bei mehreren passenden Pfaden kein beliebiger Treffer gewinnen.
            return exactMatches.Count == 1 ? exactMatches[0] : null;
        }

        if (normalizedTarget.Contains('/'))
        {
            // Ein ausdrücklich genannter Ordner ist verbindlich. Ein fehlender Pfad darf nicht
            // unbemerkt auf eine gleichnamige Datei in einem anderen Verzeichnis umgelenkt werden.
            return null;
        }

        List<NoteDocument> nameMatches = allNotes.Where(note =>
            string.Equals(
                GetNoteName(NormalizePath(note.RelativePath)),
                normalizedTarget,
                StringComparison.OrdinalIgnoreCase)).ToList();

        // Ein Kurzlink ohne Ordner darf nur dann auf eine tiefere Notiz zeigen, wenn das Ziel eindeutig ist.
        return nameMatches.Count == 1 ? nameMatches[0] : null;
    }

    /// <summary>
    /// Ermittelt den Notiznamen aus einem bereits auf Slash-Schreibweise normalisierten relativen Pfad.
    /// </summary>
    /// <param name="normalizedPath">Relativer Wiki-Pfad mit einheitlichen Schrägstrichen.</param>
    /// <returns>Dateiname ohne Ordner und optionale Markdown-Endung.</returns>
    private static string GetNoteName(string normalizedPath)
    {
        // Wiki-Pfade verwenden unabhängig vom Betriebssystem Schrägstriche.
        int separatorIndex = normalizedPath.LastIndexOf('/');
        return RemoveMarkdownExtension(normalizedPath[(separatorIndex + 1)..]);
    }

    /// <summary>
    /// Entfernt optional die Markdown-Dateiendung.
    /// </summary>
    /// <param name="path">Zu normalisierender Pfad oder Dateiname.</param>
    /// <returns>Pfad ohne <c>.md</c>-Endung.</returns>
    private static string RemoveMarkdownExtension(string path)
    {
        return path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? path[..^3]
            : path;
    }

    /// <summary>
    /// Normalisiert einen Pfad auf Slash-Schreibweise.
    /// </summary>
    /// <param name="path">Ursprungspfad.</param>
    /// <returns>Normalisierter Pfad.</returns>
    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Trim();
    }
}

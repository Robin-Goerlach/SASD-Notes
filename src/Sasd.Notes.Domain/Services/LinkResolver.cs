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
        // Ohne gültiges Ziel ist keine Auflösung möglich.
        if (string.IsNullOrWhiteSpace(target))
        {
            return null;
        }

        string normalizedTarget = NormalizePath(target);

        // Bevorzugt wird zunächst ein exakter relativer Pfad ohne .md-Endung.
        NoteDocument? exactPathMatch = allNotes.FirstOrDefault(note =>
            string.Equals(
                NormalizePath(RemoveMarkdownExtension(note.RelativePath)),
                normalizedTarget,
                StringComparison.OrdinalIgnoreCase));

        if (exactPathMatch is not null)
        {
            return exactPathMatch;
        }

        // Falls kein Pfad passt, wird auf den Dateinamen ohne Endung zurückgefallen.
        string targetFileName = Path.GetFileName(normalizedTarget);

        return allNotes.FirstOrDefault(note =>
            string.Equals(
                RemoveMarkdownExtension(Path.GetFileName(note.RelativePath)),
                RemoveMarkdownExtension(targetFileName),
                StringComparison.OrdinalIgnoreCase));
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

using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Führt einfache Volltext- und Titel-Suchen über geladene Notizen aus.
/// </summary>
public sealed class SearchService
{
    /// <summary>
    /// Durchsucht die angegebenen Notizen nach einem Suchbegriff.
    /// </summary>
    /// <param name="notes">Zu durchsuchende Notizen.</param>
    /// <param name="query">Suchbegriff.</param>
    /// <returns>Gefundene Treffer in sinnvoller Sortierung.</returns>
    public IReadOnlyList<SearchHit> Search(IReadOnlyList<NoteDocument> notes, string query)
    {
        // Leere Suchanfragen liefern bewusst keine Trefferliste.
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<SearchHit>();
        }

        var results = new List<SearchHit>();
        string trimmedQuery = query.Trim();
        StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        foreach (NoteDocument note in notes)
        {
            int score = 0;
            int matchIndex = note.Content.IndexOf(trimmedQuery, comparison);

            if (note.Title.Contains(trimmedQuery, comparison))
            {
                score += 100;
            }

            if (note.RelativePath.Contains(trimmedQuery, comparison))
            {
                score += 40;
            }

            if (matchIndex >= 0)
            {
                score += 50;
            }

            if (score <= 0)
            {
                continue;
            }

            results.Add(new SearchHit
            {
                NoteTitle = note.Title,
                RelativePath = note.RelativePath,
                PreviewText = BuildPreview(note.Content, matchIndex, trimmedQuery.Length),
                Score = score,
                MatchIndex = matchIndex
            });
        }

        return results
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.NoteTitle, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Baut einen kurzen Vorschautext aus der Trefferposition auf.
    /// </summary>
    /// <param name="content">Volltext der Notiz.</param>
    /// <param name="matchIndex">Fundposition im Text.</param>
    /// <param name="queryLength">Länge des Suchbegriffs.</param>
    /// <returns>Kurzer Vorschautext.</returns>
    private static string BuildPreview(string content, int matchIndex, int queryLength)
    {
        // Ohne gefundene Position wird der Anfang des Dokuments als Fallback verwendet.
        if (matchIndex < 0)
        {
            return content.Length <= 120
                ? content.ReplaceLineEndings(" ")
                : content[..120].ReplaceLineEndings(" ") + "...";
        }

        int start = Math.Max(0, matchIndex - 35);
        int length = Math.Min(content.Length - start, Math.Max(queryLength + 70, 70));

        string excerpt = content.Substring(start, length).ReplaceLineEndings(" ").Trim();
        return start > 0 ? "... " + excerpt : excerpt;
    }
}

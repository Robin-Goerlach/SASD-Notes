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
    /// <returns>Treffer nach absteigendem Score, anschließend nach Titel und relativem Pfad sortiert.</returns>
    /// <remarks>Vorschauen erhalten gültige UTF-16-Surrogatpaare; die Fundposition bezieht sich weiterhin auf den Originaltext.</remarks>
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
            .ThenBy(result => result.RelativePath, StringComparer.OrdinalIgnoreCase)
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
        // Ohne Inhaltstreffer dient der Dokumentanfang als Vorschau. Die Fundposition selbst bleibt unverändert.
        int start = matchIndex < 0 ? 0 : Math.Max(0, matchIndex - 35);
        long desiredLength = matchIndex < 0 ? 120L : queryLength + 70L;
        int end = (int)Math.Min(content.Length, start + desiredLength);

        // UTF-16 speichert Zeichen wie viele Emojis als Surrogatpaar. Die Vorschau darf dieses Paar
        // weder am Anfang noch am Ende trennen. Dies ist keine vollständige Graphemsegmentierung.
        if (start > 0 && start < content.Length &&
            char.IsLowSurrogate(content[start]) && char.IsHighSurrogate(content[start - 1]))
        {
            start--;
        }
        if (end > 0 && end < content.Length &&
            char.IsHighSurrogate(content[end - 1]) && char.IsLowSurrogate(content[end]))
        {
            end++;
        }

        string excerpt = content[start..end].ReplaceLineEndings(" ").Trim();
        string prefix = start > 0 ? "... " : string.Empty;
        string suffix = end < content.Length ? " ..." : string.Empty;
        return prefix + excerpt + suffix;
    }
}

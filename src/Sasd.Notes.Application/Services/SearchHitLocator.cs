using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Lokalisiert einen früheren Inhaltstreffer sicher im aktuellen Stand einer Notiz.
/// </summary>
public static class SearchHitLocator
{
    /// <summary>
    /// Sucht den gespeicherten Treffertext im aktuellen Inhalt und bevorzugt die frühere Position.
    /// </summary>
    /// <param name="hit">Treffer aus einer zuvor ausgeführten Suche.</param>
    /// <param name="currentContent">Aktueller Markdown-Inhalt derselben Notiz.</param>
    /// <returns>Aktuelle UTF-16-Position und Länge oder <see langword="null"/>, wenn der Treffer nicht mehr existiert.</returns>
    /// <exception cref="ArgumentNullException">Treffer oder Inhalt fehlen.</exception>
    public static (int Start, int Length)? Locate(SearchHit hit, string currentContent)
    {
        ArgumentNullException.ThrowIfNull(hit);
        ArgumentNullException.ThrowIfNull(currentContent);

        // Titel- und Pfadtreffer besitzen bewusst keinen markierbaren Inhaltsbereich.
        if (hit.MatchIndex < 0
            || hit.MatchLength <= 0
            || hit.MatchedText.Length != hit.MatchLength)
        {
            return null;
        }

        if (IsMatchAt(currentContent, hit.MatchIndex, hit.MatchedText))
        {
            return (hit.MatchIndex, hit.MatchLength);
        }

        // Verschobene Treffer werden vollständig gesucht. Bei mehreren Vorkommen ist die kleinste
        // Entfernung zur ursprünglichen Position stabiler als ein pauschaler Sprung zum Dokumentanfang.
        int bestIndex = -1;
        long bestDistance = long.MaxValue;
        int searchStart = 0;
        while (searchStart <= currentContent.Length - hit.MatchedText.Length)
        {
            int candidate = currentContent.IndexOf(
                hit.MatchedText,
                searchStart,
                StringComparison.OrdinalIgnoreCase);
            if (candidate < 0)
            {
                break;
            }

            long distance = Math.Abs((long)candidate - hit.MatchIndex);
            if (distance < bestDistance)
            {
                bestIndex = candidate;
                bestDistance = distance;
            }

            // Ein Schritt statt Trefferlänge berücksichtigt auch überlappende Vorkommen.
            searchStart = candidate + 1;
        }

        return bestIndex >= 0 ? (bestIndex, hit.MatchLength) : null;
    }

    /// <summary>
    /// Prüft einen Bereich ohne Substring-Zuweisung und ohne ungültige Grenzen zuzulassen.
    /// </summary>
    private static bool IsMatchAt(string content, int start, string matchedText)
    {
        return start >= 0
            && start <= content.Length - matchedText.Length
            && content.AsSpan(start, matchedText.Length).Equals(
                matchedText.AsSpan(),
                StringComparison.OrdinalIgnoreCase);
    }
}

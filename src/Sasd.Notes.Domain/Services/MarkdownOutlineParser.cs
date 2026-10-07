using System.Text.RegularExpressions;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Domain.Services;

/// <summary>
/// Standardimplementierung zum Extrahieren von Markdown-Überschriften.
/// </summary>
public sealed class MarkdownOutlineParser : IOutlineParser
{
    /// <summary>
    /// Regulärer Ausdruck zur Erkennung von Markdown-ATX-Überschriften.
    /// </summary>
    private static readonly Regex HeadingRegex =
        new(@"^(#{1,6})\s+(.+?)\s*$", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <inheritdoc />
    public IReadOnlyList<OutlineItem> Parse(string markdownContent)
    {
        // Leerer Text erzeugt eine leere Gliederung.
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return Array.Empty<OutlineItem>();
        }

        var matches = HeadingRegex.Matches(markdownContent);
        var result = new List<OutlineItem>(matches.Count);

        foreach (Match match in matches)
        {
            result.Add(new OutlineItem
            {
                Level = match.Groups[1].Value.Length,
                HeadingText = match.Groups[2].Value.Trim(),
                StartIndex = match.Index
            });
        }

        return result;
    }
}

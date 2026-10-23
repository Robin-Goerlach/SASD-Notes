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
        new(@"^(#{1,6})[ \t]+([^\r\n]+?)[ \t]*\r?$", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <inheritdoc />
    public IReadOnlyList<OutlineItem> Parse(string markdownContent)
    {
        // Leerer Text erzeugt eine leere Gliederung.
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return Array.Empty<OutlineItem>();
        }

        IReadOnlyList<(int Start, int End)> codeBlocks = MarkdownCodeBlockScanner.FindBlocks(markdownContent);
        var matches = HeadingRegex.Matches(markdownContent);
        var result = new List<OutlineItem>(matches.Count);

        foreach (Match match in matches)
        {
            // Codebeispiele erzeugen weder navigierbare Wiki-Links noch Einträge in der Dokumentgliederung.
            if (MarkdownCodeBlockScanner.ContainsPosition(match.Index, codeBlocks))
            {
                continue;
            }

            string headingText = match.Groups[2].Value.Trim();
            if (headingText.Length == 0)
            {
                continue;
            }

            result.Add(new OutlineItem
            {
                Level = match.Groups[1].Value.Length,
                HeadingText = headingText,
                StartIndex = match.Index
            });
        }

        return result;
    }
}

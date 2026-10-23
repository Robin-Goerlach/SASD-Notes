using System.Text.RegularExpressions;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Domain.Services;

/// <summary>
/// Standardimplementierung zum Erkennen von Wiki-Links.
/// </summary>
public sealed class WikiLinkParser : IWikiLinkParser
{
    /// <summary>
    /// Regulärer Ausdruck zur Erkennung von Obsidian-artigen Wiki-Links.
    /// </summary>
    private static readonly Regex WikiLinkRegex =
        new(@"\[\[([^\r\n]+?)(\|([^\r\n]+?))?\]\]", RegexOptions.Compiled);

    /// <inheritdoc />
    public IReadOnlyList<WikiLink> Parse(string markdownContent)
    {
        // Leerer Inhalt kann keine Links enthalten.
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return Array.Empty<WikiLink>();
        }

        IReadOnlyList<(int Start, int End)> codeBlocks = MarkdownCodeBlockScanner.FindBlocks(markdownContent);
        IReadOnlyList<(int Start, int End)> inlineCodeSpans = MarkdownInlineCodeScanner.FindSpans(markdownContent, codeBlocks);
        var matches = WikiLinkRegex.Matches(markdownContent);
        var result = new List<WikiLink>(matches.Count);

        foreach (Match match in matches)
        {
            // Auch ein Link, der erst in einen Code-Span hineinragt, ist kein vollständiger Wiki-Link.
            // Die Prüfung verwendet Bereiche im Originaltext, ohne die gemeldeten Linkpositionen zu verschieben.
            if (MarkdownCodeBlockScanner.ContainsPosition(match.Index, codeBlocks) ||
                inlineCodeSpans.Any(span => match.Index < span.End && match.Index + match.Length > span.Start))
            {
                continue;
            }

            // Gruppe 1 enthält das Ziel, Gruppe 3 optional den Alias.
            string target = match.Groups[1].Value.Trim();
            if (target.Length == 0)
            {
                continue;
            }
            string? alias = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;

            result.Add(new WikiLink
            {
                RawText = match.Value,
                Target = target,
                Alias = string.IsNullOrWhiteSpace(alias) ? null : alias,
                StartIndex = match.Index,
                Length = match.Length
            });
        }

        return result;
    }
}

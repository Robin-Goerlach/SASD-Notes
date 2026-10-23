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
        var matches = WikiLinkRegex.Matches(markdownContent);
        var result = new List<WikiLink>(matches.Count);

        foreach (Match match in matches)
        {
            // Codebeispiele erzeugen weder navigierbare Wiki-Links noch Einträge in der Dokumentgliederung.
            if (MarkdownCodeBlockScanner.ContainsPosition(match.Index, codeBlocks))
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

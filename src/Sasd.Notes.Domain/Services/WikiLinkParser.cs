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
        new(@"\[\[(.+?)(\|(.+?))?\]\]", RegexOptions.Compiled);

    /// <inheritdoc />
    public IReadOnlyList<WikiLink> Parse(string markdownContent)
    {
        // Leerer Inhalt kann keine Links enthalten.
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return Array.Empty<WikiLink>();
        }

        var matches = WikiLinkRegex.Matches(markdownContent);
        var result = new List<WikiLink>(matches.Count);

        foreach (Match match in matches)
        {
            // Gruppe 1 enthält das Ziel, Gruppe 3 optional den Alias.
            string target = match.Groups[1].Value.Trim();
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

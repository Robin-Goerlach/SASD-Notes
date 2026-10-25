using System.Text;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Bereitet Wiki-Links eines Markdown-Dokuments für die schreibgeschützte Rechercheansicht vor.
/// </summary>
/// <remarks>
/// Der Dienst verwendet dieselbe Parser- und Resolverlogik wie Editor und Backlinks.
/// Er erzeugt ausschließlich internes Markdown und besitzt keine Abhängigkeit von HTML
/// oder einem konkreten UI-Control.
/// </remarks>
public sealed class ResearchMarkdownService
{
    /// <summary>
    /// URI-Präfix für aufgelöste, ausschließlich innerhalb des geladenen Vaults navigierbare Links.
    /// </summary>
    public const string InternalNoteUriPrefix = "sasd-note:";

    private readonly IWikiLinkParser _wikiLinkParser;
    private readonly ILinkResolver _linkResolver;

    /// <summary>
    /// Initialisiert den Dienst mit den gemeinsamen Wiki-Link-Regeln.
    /// </summary>
    /// <param name="wikiLinkParser">Parser für Wiki-Links und deren Originalpositionen.</param>
    /// <param name="linkResolver">Resolver für eindeutige Ziele innerhalb eines Vaults.</param>
    public ResearchMarkdownService(IWikiLinkParser wikiLinkParser, ILinkResolver linkResolver)
    {
        _wikiLinkParser = wikiLinkParser ?? throw new ArgumentNullException(nameof(wikiLinkParser));
        _linkResolver = linkResolver ?? throw new ArgumentNullException(nameof(linkResolver));
    }

    /// <summary>
    /// Wandelt eindeutig aufgelöste Wiki-Links in normale Markdown-Links mit internem URI-Schema um.
    /// </summary>
    /// <param name="markdown">Unveränderter Markdown-Arbeitsstand der angezeigten Notiz.</param>
    /// <param name="allNotes">Alle geladenen Notizen, gegen die Ziele aufgelöst werden.</param>
    /// <returns>
    /// Markdown für den Renderer. Fehlende und mehrdeutige Ziele bleiben als sichtbare
    /// Wiki-Link-Syntax erhalten.
    /// </returns>
    public string PrepareForRendering(string markdown, IReadOnlyList<NoteDocument> allNotes)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(allNotes);

        IReadOnlyList<WikiLink> wikiLinks = _wikiLinkParser.Parse(markdown);
        if (wikiLinks.Count == 0)
        {
            return markdown;
        }

        var preparedMarkdown = new StringBuilder(markdown);

        // Die Ersetzungen erfolgen von hinten nach vorne. Dadurch bleiben alle vom
        // Parser gemeldeten Positionen gültig, obwohl Ersatztexte andere Längen haben.
        foreach (WikiLink wikiLink in wikiLinks.OrderByDescending(link => link.StartIndex))
        {
            NoteDocument? targetNote = _linkResolver.Resolve(wikiLink.Target, allNotes);
            if (targetNote is null || !HasValidRange(wikiLink, markdown.Length))
            {
                continue;
            }

            string displayText = string.IsNullOrWhiteSpace(wikiLink.Alias)
                ? wikiLink.Target
                : wikiLink.Alias;
            string escapedDisplayText = EscapeMarkdownLinkText(displayText);
            string encodedPath = Uri.EscapeDataString(targetNote.RelativePath);
            string replacement =
                $"[{escapedDisplayText}]({InternalNoteUriPrefix}{encodedPath})";

            preparedMarkdown.Remove(wikiLink.StartIndex, wikiLink.Length);
            preparedMarkdown.Insert(wikiLink.StartIndex, replacement);
        }

        return preparedMarkdown.ToString();
    }

    /// <summary>
    /// Prüft defensiv, ob eine Parserposition in den übergebenen Originaltext passt.
    /// </summary>
    private static bool HasValidRange(WikiLink wikiLink, int markdownLength)
    {
        return wikiLink.StartIndex >= 0
            && wikiLink.Length > 0
            && wikiLink.StartIndex <= markdownLength - wikiLink.Length;
    }

    /// <summary>
    /// Verhindert, dass Alias- oder Zieltext unbeabsichtigt zusätzliche Markdown-Syntax erzeugt.
    /// </summary>
    private static string EscapeMarkdownLinkText(string text)
    {
        var escapedText = new StringBuilder(text.Length);

        foreach (char character in text)
        {
            if (character is '\\' or '`' or '*' or '_' or '{' or '}' or '[' or ']'
                or '<' or '>' or '(' or ')' or '#' or '+' or '-' or '.' or '!'
                or '|' or '~')
            {
                escapedText.Append('\\');
            }

            escapedText.Append(character);
        }

        return escapedText.ToString();
    }
}

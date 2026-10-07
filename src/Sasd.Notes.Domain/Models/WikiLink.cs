namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Repräsentiert einen einzelnen Wiki-Link im Stil <c>[[Notiz]]</c> oder <c>[[Notiz|Alias]]</c>.
/// </summary>
public sealed class WikiLink
{
    /// <summary>
    /// Vollständiger Rohtext des Links inklusive Klammern.
    /// </summary>
    public required string RawText { get; init; }

    /// <summary>
    /// Zielwert des Links ohne Klammern und ohne Alias.
    /// </summary>
    public required string Target { get; init; }

    /// <summary>
    /// Optionaler Alias des Links.
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// Startindex des Links im Markdown-Text.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Länge des Links im Markdown-Text.
    /// </summary>
    public int Length { get; init; }
}

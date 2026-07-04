namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Kennzeichnet eine einfache Markdown-Formatierungsaktion.
/// </summary>
public enum MarkdownFormatKind
{
    Heading1,
    Heading2,
    Heading3,
    Bold,
    Italic,
    Strikethrough,
    InlineCode,
    Quote,
    BulletList,
    NumberedList,
    CheckList,
    WikiLink
}

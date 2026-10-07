namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Kennzeichnet eine einfache Markdown-Formatierungsaktion.
/// </summary>
public enum MarkdownFormatKind
{
    /// <summary>Formatiert die betroffenen Zeilen als Überschrift erster Ebene.</summary>
    Heading1,
    /// <summary>Formatiert die betroffenen Zeilen als Überschrift zweiter Ebene.</summary>
    Heading2,
    /// <summary>Formatiert die betroffenen Zeilen als Überschrift dritter Ebene.</summary>
    Heading3,
    /// <summary>Hebt den ausgewählten Text fett hervor.</summary>
    Bold,
    /// <summary>Hebt den ausgewählten Text kursiv hervor.</summary>
    Italic,
    /// <summary>Stellt den ausgewählten Text durchgestrichen dar.</summary>
    Strikethrough,
    /// <summary>Formatiert den ausgewählten Text als Inline-Code.</summary>
    InlineCode,
    /// <summary>Formatiert die betroffenen Zeilen als Markdown-Zitat.</summary>
    Quote,
    /// <summary>Formatiert die betroffenen Zeilen als Aufzählung.</summary>
    BulletList,
    /// <summary>Formatiert die betroffenen Zeilen als nummerierte Liste.</summary>
    NumberedList,
    /// <summary>Formatiert die betroffenen Zeilen als Checkliste.</summary>
    CheckList,
    /// <summary>Umschließt den ausgewählten Text als Wiki-Link.</summary>
    WikiLink
}

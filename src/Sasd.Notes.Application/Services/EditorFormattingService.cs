using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Application.Services;

/// <summary>
/// Stellt einfache Markdown-Formatierungsoperationen für den Texteditor bereit.
/// </summary>
public sealed class EditorFormattingService
{
    /// <summary>
    /// Wendet die gewünschte Formatierung auf die aktuelle Auswahl an.
    /// </summary>
    /// <param name="text">Gesamter aktueller Editorinhalt.</param>
    /// <param name="selectionStart">Startposition der Auswahl.</param>
    /// <param name="selectionLength">Länge der Auswahl.</param>
    /// <param name="formatKind">Anzufordernde Markdown-Formatierung.</param>
    /// <returns>Transformationsergebnis mit neuem Text und neuer Auswahl.</returns>
    public TextSelectionTransformResult Apply(string text, int selectionStart, int selectionLength, MarkdownFormatKind formatKind)
    {
        string safeText = text ?? string.Empty;
        int safeSelectionStart = Math.Clamp(selectionStart, 0, safeText.Length);
        int safeSelectionLength = Math.Clamp(selectionLength, 0, safeText.Length - safeSelectionStart);

        return formatKind switch
        {
            MarkdownFormatKind.Heading1 => ApplyLinePrefix(safeText, safeSelectionStart, safeSelectionLength, "# "),
            MarkdownFormatKind.Heading2 => ApplyLinePrefix(safeText, safeSelectionStart, safeSelectionLength, "## "),
            MarkdownFormatKind.Heading3 => ApplyLinePrefix(safeText, safeSelectionStart, safeSelectionLength, "### "),
            MarkdownFormatKind.Bold => ApplyWrapper(safeText, safeSelectionStart, safeSelectionLength, "**", "**", "fett"),
            MarkdownFormatKind.Italic => ApplyWrapper(safeText, safeSelectionStart, safeSelectionLength, "*", "*", "kursiv"),
            MarkdownFormatKind.Strikethrough => ApplyWrapper(safeText, safeSelectionStart, safeSelectionLength, "~~", "~~", "durchgestrichen"),
            MarkdownFormatKind.InlineCode => ApplyWrapper(safeText, safeSelectionStart, safeSelectionLength, "`", "`", "code"),
            MarkdownFormatKind.Quote => ApplyLinePrefix(safeText, safeSelectionStart, safeSelectionLength, "> "),
            MarkdownFormatKind.BulletList => ApplyLinePrefix(safeText, safeSelectionStart, safeSelectionLength, "- "),
            MarkdownFormatKind.NumberedList => ApplyNumberedList(safeText, safeSelectionStart, safeSelectionLength),
            MarkdownFormatKind.CheckList => ApplyLinePrefix(safeText, safeSelectionStart, safeSelectionLength, "- [ ] "),
            MarkdownFormatKind.WikiLink => ApplyWrapper(safeText, safeSelectionStart, safeSelectionLength, "[[", "]]", "Notiz"),
            _ => new TextSelectionTransformResult { UpdatedText = safeText, SelectionStart = safeSelectionStart, SelectionLength = safeSelectionLength }
        };
    }

    /// <summary>
    /// Umschließt die aktuelle Auswahl mit Präfix und Suffix.
    /// </summary>
    private static TextSelectionTransformResult ApplyWrapper(string text, int selectionStart, int selectionLength, string prefix, string suffix, string placeholder)
    {
        string selectedText = selectionLength > 0 ? text.Substring(selectionStart, selectionLength) : placeholder;

        string before = text[..selectionStart];
        string after = text[(selectionStart + selectionLength)..];
        string inserted = prefix + selectedText + suffix;
        string updatedText = before + inserted + after;

        // Wenn vorher nichts ausgewählt war, soll der Platzhalter markiert bleiben.
        int newSelectionStart = selectionLength > 0
            ? selectionStart + prefix.Length
            : selectionStart + prefix.Length;

        int newSelectionLength = selectedText.Length;

        return new TextSelectionTransformResult
        {
            UpdatedText = updatedText,
            SelectionStart = newSelectionStart,
            SelectionLength = newSelectionLength
        };
    }

    /// <summary>
    /// Fügt allen betroffenen Zeilen ein Präfix hinzu.
    /// </summary>
    private static TextSelectionTransformResult ApplyLinePrefix(string text, int selectionStart, int selectionLength, string prefix)
    {
        (int lineStart, int lineEnd) = ExpandToLineRange(text, selectionStart, selectionLength);
        string targetBlock = text.Substring(lineStart, lineEnd - lineStart);

        string[] lines = targetBlock.Replace("
", "
").Split('
');
        for (int index = 0; index < lines.Length; index++)
        {
            // Leere Zeilen bleiben unangetastet, damit keine unnötigen Marker entstehen.
            if (lines[index].Length == 0)
            {
                continue;
            }

            lines[index] = prefix + lines[index];
        }

        string replacedBlock = string.Join(Environment.NewLine, lines);
        string updatedText = text[..lineStart] + replacedBlock + text[lineEnd..];

        return new TextSelectionTransformResult
        {
            UpdatedText = updatedText,
            SelectionStart = lineStart,
            SelectionLength = replacedBlock.Length
        };
    }

    /// <summary>
    /// Erzeugt eine nummerierte Liste aus den betroffenen Zeilen.
    /// </summary>
    private static TextSelectionTransformResult ApplyNumberedList(string text, int selectionStart, int selectionLength)
    {
        (int lineStart, int lineEnd) = ExpandToLineRange(text, selectionStart, selectionLength);
        string targetBlock = text.Substring(lineStart, lineEnd - lineStart);

        string[] lines = targetBlock.Replace("
", "
").Split('
');
        int visibleIndex = 1;

        for (int index = 0; index < lines.Length; index++)
        {
            if (lines[index].Length == 0)
            {
                continue;
            }

            lines[index] = $"{visibleIndex}. {lines[index]}";
            visibleIndex++;
        }

        string replacedBlock = string.Join(Environment.NewLine, lines);
        string updatedText = text[..lineStart] + replacedBlock + text[lineEnd..];

        return new TextSelectionTransformResult
        {
            UpdatedText = updatedText,
            SelectionStart = lineStart,
            SelectionLength = replacedBlock.Length
        };
    }

    /// <summary>
    /// Erweitert eine Auswahl auf vollständige Zeilen.
    /// </summary>
    private static (int Start, int End) ExpandToLineRange(string text, int selectionStart, int selectionLength)
    {
        int start = selectionStart;
        int end = selectionStart + selectionLength;

        while (start > 0 && text[start - 1] != '
')
        {
            start--;
        }

        while (end < text.Length && text[end] != '
')
        {
            end++;
        }

        return (start, end);
    }
}

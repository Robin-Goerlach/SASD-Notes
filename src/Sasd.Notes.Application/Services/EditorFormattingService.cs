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
    public TextSelectionTransformResult Apply(
        string text,
        int selectionStart,
        int selectionLength,
        MarkdownFormatKind formatKind)
    {
        string safeText = text ?? string.Empty;

        // Ungültige UI-Auswahlgrenzen werden auf den gültigen Dokumentbereich begrenzt.
        int safeSelectionStart = Math.Clamp(selectionStart, 0, safeText.Length);
        int safeSelectionLength = Math.Clamp(
            selectionLength,
            0,
            safeText.Length - safeSelectionStart);

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
            _ => new TextSelectionTransformResult
            {
                UpdatedText = safeText,
                SelectionStart = safeSelectionStart,
                SelectionLength = safeSelectionLength
            }
        };
    }

    /// <summary>
    /// Umschließt die aktuelle Auswahl mit Präfix und Suffix.
    /// </summary>
    private static TextSelectionTransformResult ApplyWrapper(
        string text,
        int selectionStart,
        int selectionLength,
        string prefix,
        string suffix,
        string placeholder)
    {
        string selectedText = selectionLength > 0
            ? text.Substring(selectionStart, selectionLength)
            : placeholder;

        string before = text[..selectionStart];
        string after = text[(selectionStart + selectionLength)..];
        string inserted = prefix + selectedText + suffix;
        string updatedText = before + inserted + after;

        // Bei einer leeren Auswahl bleibt der eingefügte Platzhalter markiert.
        return new TextSelectionTransformResult
        {
            UpdatedText = updatedText,
            SelectionStart = selectionStart + prefix.Length,
            SelectionLength = selectedText.Length
        };
    }

    /// <summary>
    /// Fügt allen betroffenen Zeilen ein Präfix hinzu.
    /// </summary>
    private static TextSelectionTransformResult ApplyLinePrefix(
        string text,
        int selectionStart,
        int selectionLength,
        string prefix)
    {
        (int lineStart, int lineEnd) = ExpandToLineRange(text, selectionStart, selectionLength);
        string targetBlock = text.Substring(lineStart, lineEnd - lineStart);
        string lineEnding = DetectLineEnding(text);

        // Für die Bearbeitung werden alle Zeilenenden vereinheitlicht und anschließend im ursprünglichen Stil geschrieben.
        string[] lines = NormalizeLineEndings(targetBlock).Split('\n', StringSplitOptions.None);
        for (int index = 0; index < lines.Length; index++)
        {
            // Leere Zeilen bleiben unangetastet, damit keine unnötigen Marker entstehen.
            if (lines[index].Length == 0)
            {
                continue;
            }

            lines[index] = prefix + lines[index];
        }

        string replacedBlock = string.Join(lineEnding, lines);
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
    private static TextSelectionTransformResult ApplyNumberedList(
        string text,
        int selectionStart,
        int selectionLength)
    {
        (int lineStart, int lineEnd) = ExpandToLineRange(text, selectionStart, selectionLength);
        string targetBlock = text.Substring(lineStart, lineEnd - lineStart);
        string lineEnding = DetectLineEnding(text);
        string[] lines = NormalizeLineEndings(targetBlock).Split('\n', StringSplitOptions.None);
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

        string replacedBlock = string.Join(lineEnding, lines);
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
    private static (int Start, int End) ExpandToLineRange(
        string text,
        int selectionStart,
        int selectionLength)
    {
        int start = selectionStart;
        int end = selectionStart + selectionLength;

        while (start > 0 && text[start - 1] != '\n')
        {
            start--;
        }

        while (end < text.Length && text[end] != '\n')
        {
            end++;
        }

        return (start, end);
    }

    /// <summary>
    /// Ermittelt den im Dokument überwiegend verwendeten Zeilenumbruch.
    /// </summary>
    private static string DetectLineEnding(string text)
    {
        return text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    }

    /// <summary>
    /// Vereinheitlicht CRLF- und CR-Zeilenumbrüche für die zeilenweise Verarbeitung.
    /// </summary>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    }
}

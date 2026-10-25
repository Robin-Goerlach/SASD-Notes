using System.Text;
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

        // Ungültige UI-Auswahlgrenzen werden begrenzt, ohne dabei ein UTF-16-Surrogatpaar aufzuteilen.
        (int safeSelectionStart, int safeSelectionLength) = NormalizeSelection(
            safeText,
            selectionStart,
            selectionLength);

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
    /// Begrenzt eine Auswahl auf den Dokumentbereich und hält UTF-16-Surrogatpaare ungeteilt.
    /// </summary>
    /// <param name="text">Aktueller Dokumenttext.</param>
    /// <param name="selectionStart">Angeforderte Startposition.</param>
    /// <param name="selectionLength">Angeforderte Auswahllänge.</param>
    /// <returns>Gültige Startposition und Länge für die nachfolgende Transformation.</returns>
    private static (int Start, int Length) NormalizeSelection(
        string text,
        int selectionStart,
        int selectionLength)
    {
        int safeStart = Math.Clamp(selectionStart, 0, text.Length);
        int safeLength = Math.Clamp(selectionLength, 0, text.Length - safeStart);

        // Ein Cursor innerhalb eines Surrogatpaars wird davor positioniert, bleibt aber eine leere Auswahl.
        if (safeLength == 0)
        {
            if (IsBetweenSurrogates(text, safeStart))
            {
                safeStart--;
            }

            return (safeStart, 0);
        }

        int safeEnd = safeStart + safeLength;
        if (IsBetweenSurrogates(text, safeStart))
        {
            safeStart--;
        }

        if (IsBetweenSurrogates(text, safeEnd))
        {
            safeEnd++;
        }

        return (safeStart, safeEnd - safeStart);
    }

    /// <summary>
    /// Prüft, ob eine UTF-16-Position zwischen High- und Low-Surrogate liegt.
    /// </summary>
    private static bool IsBetweenSurrogates(string text, int position)
    {
        return position > 0
            && position < text.Length
            && char.IsHighSurrogate(text[position - 1])
            && char.IsLowSurrogate(text[position]);
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

        // Jeder vorhandene Zeilentrenner bleibt an seiner ursprünglichen Position und in seiner ursprünglichen Form erhalten.
        string replacedBlock = TransformLinesPreservingEndings(
            targetBlock,
            line => line.Length == 0 ? line : prefix + line);
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
        int visibleIndex = 1;

        string replacedBlock = TransformLinesPreservingEndings(
            targetBlock,
            line =>
            {
                if (line.Length == 0)
                {
                    return line;
                }

                string numberedLine = $"{visibleIndex}. {line}";
                visibleIndex++;
                return numberedLine;
            });
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

        // Eine Position innerhalb von CRLF gehört noch zum Ende der vorherigen Zeile.
        if (start > 0 && start < text.Length && text[start] == '\n' && text[start - 1] == '\r')
        {
            start--;
        }

        while (start > 0 && text[start - 1] != '\r' && text[start - 1] != '\n')
        {
            start--;
        }

        int lastSelectedPosition = selectionStart + selectionLength;
        if (selectionLength > 0)
        {
            // Das Auswahlende ist exklusiv. Ein markierter Zeilenumbruch darf die nächste Zeile nicht einbeziehen.
            lastSelectedPosition--;
            while (lastSelectedPosition > start &&
                   (text[lastSelectedPosition] == '\r' || text[lastSelectedPosition] == '\n'))
            {
                lastSelectedPosition--;
            }
        }

        // Auch der Endanker darf CRLF nicht zwischen seinen beiden Zeichen aufteilen.
        if (lastSelectedPosition > 0 && lastSelectedPosition < text.Length &&
            text[lastSelectedPosition] == '\n' && text[lastSelectedPosition - 1] == '\r')
        {
            lastSelectedPosition--;
        }

        int end = Math.Max(start, lastSelectedPosition);
        while (end < text.Length && text[end] != '\r' && text[end] != '\n')
        {
            end++;
        }

        // Der letzte Zeilenumbruch bleibt außerhalb des ersetzten Blocks und damit unverändert erhalten.
        return (start, end);
    }

    /// <summary>
    /// Transformiert die sichtbaren Inhalte aller Zeilen, ohne deren individuelle Zeilentrenner zu verändern.
    /// </summary>
    /// <param name="text">Zu bearbeitender Textblock.</param>
    /// <param name="transformLine">Transformation für den Inhalt einer einzelnen Zeile ohne Zeilentrenner.</param>
    /// <returns>Transformierter Block mit unveränderten CRLF-, LF- und CR-Trennern.</returns>
    private static string TransformLinesPreservingEndings(
        string text,
        Func<string, string> transformLine)
    {
        var result = new StringBuilder(text.Length);
        int lineStart = 0;

        while (lineStart < text.Length)
        {
            int lineEnd = lineStart;
            while (lineEnd < text.Length && text[lineEnd] != '\r' && text[lineEnd] != '\n')
            {
                lineEnd++;
            }

            result.Append(transformLine(text[lineStart..lineEnd]));
            if (lineEnd >= text.Length)
            {
                break;
            }

            // CRLF bildet einen einzigen logischen Trenner; einzelne CR- oder LF-Zeichen bleiben ebenfalls unverändert.
            if (text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n')
            {
                result.Append("\r\n");
                lineStart = lineEnd + 2;
            }
            else
            {
                result.Append(text[lineEnd]);
                lineStart = lineEnd + 1;
            }
        }

        return result.ToString();
    }
}

namespace Sasd.Notes.Domain.Services;

/// <summary>
/// Erkennt einzeilige Code-Spans außerhalb bereits ermittelter eingefasster Codeblöcke.
/// </summary>
internal static class MarkdownInlineCodeScanner
{
    /// <summary>
    /// Ermittelt mit gleich langen Backtick-Gruppen begrenzte Code-Spans im Originaltext.
    /// </summary>
    /// <param name="markdownContent">Unveränderter Markdown-Text.</param>
    /// <param name="codeBlocks">Bereits bekannte eingefasste Codeblöcke.</param>
    /// <returns>Code-Spans einschließlich ihrer Marker mit exklusivem Bereichsende.</returns>
    internal static IReadOnlyList<(int Start, int End)> FindSpans(
        string markdownContent,
        IReadOnlyList<(int Start, int End)> codeBlocks)
    {
        var spans = new List<(int Start, int End)>();
        int lineStart = 0;

        while (lineStart < markdownContent.Length)
        {
            int lineEnd = lineStart;
            while (lineEnd < markdownContent.Length && markdownContent[lineEnd] != '\r' && markdownContent[lineEnd] != '\n')
            {
                lineEnd++;
            }

            // Fence-Zeilen und deren Inhalt dürfen keine zusätzlichen Inline-Codegrenzen erzeugen.
            if (!MarkdownCodeBlockScanner.ContainsPosition(lineStart, codeBlocks))
            {
                ScanLine(markdownContent, lineStart, lineEnd, spans);
            }

            // Die Originalpositionen bleiben erhalten; CRLF wird beim Fortschreiten als ein Zeilenende behandelt.
            lineStart = lineEnd;
            if (lineStart < markdownContent.Length && markdownContent[lineStart] == '\r')
            {
                lineStart++;
            }
            if (lineStart < markdownContent.Length && markdownContent[lineStart] == '\n')
            {
                lineStart++;
            }
        }

        return spans;
    }

    /// <summary>
    /// Sucht zusammengehörige Backtick-Gruppen innerhalb genau einer Zeile.
    /// </summary>
    private static void ScanLine(string text, int lineStart, int lineEnd, ICollection<(int Start, int End)> spans)
    {
        int position = lineStart;
        while (position < lineEnd)
        {
            // Außerhalb eines Code-Spans escapte Backslashes und Backticks werden als Literalpaare übersprungen.
            // Bei mehreren Backslashes ergibt sich dadurch automatisch die richtige Gerade-/Ungerade-Regel.
            if (text[position] == '\\' && position + 1 < lineEnd &&
                (text[position + 1] == '\\' || text[position + 1] == '`'))
            {
                position += 2;
                continue;
            }

            if (text[position] != '`')
            {
                position++;
                continue;
            }

            int openingEnd = SkipBackticks(text, position, lineEnd);
            int openingLength = openingEnd - position;
            int searchPosition = openingEnd;
            int closingEnd = -1;
            while (searchPosition < lineEnd)
            {
                if (text[searchPosition] != '`')
                {
                    searchPosition++;
                    continue;
                }

                int runEnd = SkipBackticks(text, searchPosition, lineEnd);
                if (runEnd - searchPosition == openingLength)
                {
                    // Innerhalb eines Code-Spans ist ein Backslash wörtlich und escapet den Abschluss nicht.
                    closingEnd = runEnd;
                    break;
                }
                searchPosition = runEnd;
            }

            if (closingEnd >= 0)
            {
                spans.Add((position, closingEnd));
                position = closingEnd;
            }
            else
            {
                // Ein offener Marker wird als Text behandelt. Spätere, anders lange Gruppen können trotzdem ein Paar bilden.
                position = openingEnd;
            }
        }
    }

    /// <summary>
    /// Liefert das exklusive Ende einer zusammenhängenden Backtick-Gruppe.
    /// </summary>
    private static int SkipBackticks(string text, int start, int lineEnd)
    {
        int end = start;
        while (end < lineEnd && text[end] == '`')
        {
            end++;
        }
        return end;
    }
}

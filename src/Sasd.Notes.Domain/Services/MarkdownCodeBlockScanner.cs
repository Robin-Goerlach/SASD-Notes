namespace Sasd.Notes.Domain.Services;

/// <summary>
/// Erkennt eingefasste Codeblöcke als unveränderte Bereiche des ursprünglichen Markdown-Texts.
/// </summary>
internal static class MarkdownCodeBlockScanner
{
    /// <summary>
    /// Ermittelt Codebereiche einschließlich öffnender und schließender Fence-Zeilen.
    /// </summary>
    /// <param name="markdownContent">Zu untersuchender Originaltext.</param>
    /// <returns>Bereiche mit inklusivem Start und exklusivem Ende in UTF-16-Zeichenpositionen.</returns>
    internal static IReadOnlyList<(int Start, int End)> FindBlocks(string markdownContent)
    {
        var blocks = new List<(int Start, int End)>();
        int blockStart = -1;
        char openingMarker = '\0';
        int openingLength = 0;
        int lineStart = 0;

        while (lineStart < markdownContent.Length)
        {
            // Zeilengrenzen werden am Originaltext ermittelt, damit CRLF nicht in LF umgewandelt werden muss.
            int lineEnd = lineStart;
            while (lineEnd < markdownContent.Length && markdownContent[lineEnd] != '\r' && markdownContent[lineEnd] != '\n')
            {
                lineEnd++;
            }

            int nextLineStart = lineEnd;
            if (nextLineStart < markdownContent.Length && markdownContent[nextLineStart] == '\r')
            {
                nextLineStart++;
            }
            if (nextLineStart < markdownContent.Length && markdownContent[nextLineStart] == '\n')
            {
                nextLineStart++;
            }

            string line = markdownContent[lineStart..lineEnd];
            int markerStart = 0;
            while (markerStart < line.Length && line[markerStart] == ' ')
            {
                markerStart++;
            }

            // Vier Leerzeichen oder ein Tab vor dem Marker gehören nicht zur hier unterstützten Fence-Syntax.
            if (markerStart <= 3 && markerStart < line.Length &&
                (line[markerStart] == '`' || line[markerStart] == '~'))
            {
                char marker = line[markerStart];
                int markerEnd = markerStart;
                while (markerEnd < line.Length && line[markerEnd] == marker)
                {
                    markerEnd++;
                }

                int markerLength = markerEnd - markerStart;
                string suffix = line[markerEnd..];
                if (blockStart < 0 && markerLength >= 3)
                {
                    // In einer Backtick-Fence darf die Sprach-/Infoangabe keine weiteren Backticks enthalten.
                    if (marker != '`' || !suffix.Contains('`'))
                    {
                        blockStart = lineStart;
                        openingMarker = marker;
                        openingLength = markerLength;
                    }
                }
                else if (blockStart >= 0 && marker == openingMarker && markerLength >= openingLength &&
                    suffix.All(character => character == ' ' || character == '\t'))
                {
                    // Kürzere, andersartige oder mit Text ergänzte Marker schließen den laufenden Block nicht.
                    blocks.Add((blockStart, nextLineStart));
                    blockStart = -1;
                }
            }

            lineStart = nextLineStart;
        }

        if (blockStart >= 0)
        {
            // Ein während des Schreibens noch offener Block reicht bis zum aktuellen Dokumentende.
            blocks.Add((blockStart, markdownContent.Length));
        }

        return blocks;
    }

    /// <summary>
    /// Prüft, ob eine Originalposition innerhalb eines zuvor ermittelten Codeblocks liegt.
    /// </summary>
    /// <param name="position">Position im ursprünglichen Text.</param>
    /// <param name="blocks">Bereiche aus <see cref="FindBlocks"/>.</param>
    /// <returns>Ob die Position zu einer Codeblock- oder Fence-Zeile gehört.</returns>
    internal static bool ContainsPosition(int position, IReadOnlyList<(int Start, int End)> blocks)
    {
        return blocks.Any(block => position >= block.Start && position < block.End);
    }
}

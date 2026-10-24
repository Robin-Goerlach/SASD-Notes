namespace Sasd.Notes.Application.Services;

/// <summary>
/// Ordnet UTF-16-Textbereiche zwischen inhaltlich gleichen Texten mit unterschiedlichen Zeilenenden zu.
/// </summary>
public static class EditorTextPositionMapper
{
    /// <summary>
    /// Überträgt einen Bereich ohne Änderung der beiden Texte.
    /// </summary>
    /// <param name="source">Text, auf den sich der ursprüngliche Bereich bezieht.</param>
    /// <param name="destination">Text, in dem der entsprechende Bereich gesucht wird.</param>
    /// <param name="start">Inklusiver Beginn im Quelltext.</param>
    /// <param name="length">Länge des Quellbereichs; 0 für eine Cursorposition.</param>
    /// <returns>Übertragener Bereich oder <see langword="null"/> bei ungültigen Grenzen oder abweichendem Inhalt.</returns>
    /// <remarks>CRLF, LF und CR werden als gleichwertige Zeilenenden behandelt. Grenzen innerhalb eines CRLF-Paars werden abgewiesen.</remarks>
    public static (int Start, int Length)? Map(string source, string destination, int start, int length)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        if (start < 0 || length < 0 || start > source.Length || length > source.Length - start)
        {
            return null;
        }

        int end = start + length;
        int sourcePosition = 0;
        int destinationPosition = 0;
        int mappedStart = -1;
        int mappedEnd = -1;
        while (true)
        {
            // Grenzen werden vor dem nächsten logischen Zeichen erfasst, einschließlich Dokumentende.
            if (sourcePosition == start)
            {
                mappedStart = destinationPosition;
            }
            if (sourcePosition == end)
            {
                mappedEnd = destinationPosition;
            }

            if (sourcePosition == source.Length || destinationPosition == destination.Length)
            {
                break;
            }

            bool sourceNewline = IsNewline(source[sourcePosition]);
            bool destinationNewline = IsNewline(destination[destinationPosition]);
            if (sourceNewline && destinationNewline)
            {
                sourcePosition = SkipNewline(source, sourcePosition);
                destinationPosition = SkipNewline(destination, destinationPosition);
            }
            else if (!sourceNewline && !destinationNewline &&
                source[sourcePosition] == destination[destinationPosition])
            {
                // Außerhalb der Zeilenenden werden die ursprünglichen UTF-16-Einheiten exakt verglichen.
                sourcePosition++;
                destinationPosition++;
            }
            else
            {
                return null;
            }
        }

        // Kein stilles Raten bei gekürztem Inhalt oder bei einer Grenze zwischen CR und LF.
        if (sourcePosition != source.Length || destinationPosition != destination.Length ||
            mappedStart < 0 || mappedEnd < 0)
        {
            return null;
        }
        return (mappedStart, mappedEnd - mappedStart);
    }

    private static bool IsNewline(char value) => value == '\r' || value == '\n';

    /// <summary>
    /// Überspringt genau ein logisches Zeilenende und erhält dabei die ursprünglichen Textindizes.
    /// </summary>
    private static int SkipNewline(string text, int position)
    {
        return text[position] == '\r' && position + 1 < text.Length && text[position + 1] == '\n'
            ? position + 2
            : position + 1;
    }
}

namespace Sasd.Notes.Domain.Services;

/// <summary>
/// Stellt Hilfsfunktionen für sichere Markdown-Dateinamen bereit.
/// </summary>
public static class FileNameHelper
{
    /// <summary>
    /// Erzeugt einen dateisystemfreundlichen Markdown-Dateinamen aus einem Titel.
    /// </summary>
    /// <param name="title">Angezeigter Titel.</param>
    /// <returns>Bereinigter Dateiname ohne Dateiendung.</returns>
    public static string ToSafeFileName(string title)
    {
        // Leere oder ungültige Titel werden abgefangen, damit keine leeren Dateinamen entstehen.
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Neue Notiz";
        }

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var buffer = new char[title.Length];
        int index = 0;

        foreach (char character in title.Trim())
        {
            // Ungültige Zeichen werden durch Leerzeichen ersetzt, damit der Titel lesbar bleibt.
            buffer[index++] = invalidCharacters.Contains(character) ? ' ' : character;
        }

        string cleaned = new string(buffer, 0, index);

        // Mehrfache Leerzeichen werden zusammengedrückt.
        string normalizedWhitespace = string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(normalizedWhitespace) ? "Neue Notiz" : normalizedWhitespace;
    }
}

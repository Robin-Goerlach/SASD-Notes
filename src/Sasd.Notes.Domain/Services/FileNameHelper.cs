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
            bool isPathSeparator = character is '/' or '\\';
            bool isControlCharacter = char.IsControl(character);
            buffer[index++] = invalidCharacters.Contains(character) || isPathSeparator || isControlCharacter
                ? ' '
                : character;
        }

        string cleaned = new string(buffer, 0, index);

        // Windows behandelt abschließende Punkte und Leerzeichen nicht zuverlässig als Dateinamenbestandteil.
        string normalizedWhitespace = string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .TrimEnd(' ', '.');

        if (string.IsNullOrWhiteSpace(normalizedWhitespace))
        {
            return "Neue Notiz";
        }

        // Reservierte Windows-Gerätenamen werden durch einen sicheren, weiterhin lesbaren Namen ersetzt.
        return IsReservedWindowsName(normalizedWhitespace)
            ? normalizedWhitespace + "_"
            : normalizedWhitespace;    }
}

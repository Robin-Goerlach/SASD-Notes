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

        // Die Windows-Regeln gelten unabhängig vom Betriebssystem des Testlaufs.
        const string invalidCharacters = "<>:\"/\\|?*";
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

        // Der Zusatz muss vor der ersten Erweiterung stehen: CON.md_ wäre weiterhin reserviert.
        if (IsReservedWindowsName(normalizedWhitespace))
        {
            int extensionIndex = normalizedWhitespace.IndexOf('.');
            return extensionIndex >= 0
                ? normalizedWhitespace.Insert(extensionIndex, "_")
                : normalizedWhitespace + "_";
        }

        return normalizedWhitespace;
    }

    /// <summary>
    /// Prüft die unter Windows reservierten Geräte- und DOS-Namen.
    /// </summary>
    private static bool IsReservedWindowsName(string value)
    {
        string baseName = value.Split('.', 2)[0];

        if (baseName.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
            baseName.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
            baseName.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
            baseName.Equals("NUL", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (baseName.Length == 4 &&
            (baseName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
             baseName.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
            baseName[3] is >= '1' and <= '9')
        {
            return true;
        }

        return false;
    }
}

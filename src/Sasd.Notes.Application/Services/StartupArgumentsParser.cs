namespace Sasd.Notes.Application.Services;

/// <summary>
/// Wertet die optionalen Startargumente ohne UI- oder Dateisystemzugriff aus.
/// </summary>
public static class StartupArgumentsParser
{
    /// <summary>
    /// Normalisiert einen einzelnen optionalen Startpfad relativ zum Arbeitsverzeichnis des aufrufenden Prozesses.
    /// </summary>
    /// <param name="arguments">Bereits vom Betriebssystem aufgeteilte Argumente ohne den Programmnamen.</param>
    /// <param name="workingDirectory">Vollständiges Arbeitsverzeichnis zum Auflösen relativer Pfade.</param>
    /// <returns>Vollständiger Startpfad oder <see langword="null"/>, wenn kein Argument angegeben wurde.</returns>
    /// <exception cref="ArgumentException">Mehrere Argumente, ein leerer Pfad oder ein ungültiger Basispfad wurden angegeben.</exception>
    public static string? Parse(IReadOnlyList<string> arguments, string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0)
        {
            return null;
        }

        if (arguments.Count != 1)
        {
            throw new ArgumentException(
                "Bitte geben Sie höchstens einen Ordner oder eine Markdown-Datei an. " +
                "Pfade mit Leerzeichen müssen in Anführungszeichen stehen.", nameof(arguments));
        }

        if (string.IsNullOrWhiteSpace(arguments[0]))
        {
            throw new ArgumentException("Der Startpfad darf nicht leer sein.", nameof(arguments));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        if (!Path.IsPathFullyQualified(workingDirectory))
        {
            throw new ArgumentException("Das Arbeitsverzeichnis muss vollständig angegeben werden.", nameof(workingDirectory));
        }

        // Anführungszeichen wurden bereits beim Programmaufruf verarbeitet. Nicht erneut splitten oder trimmen:
        // Leerzeichen und Unicode-Zeichen innerhalb eines gültigen Pfadarguments müssen erhalten bleiben.
        return Path.GetFullPath(arguments[0], workingDirectory);
    }
}

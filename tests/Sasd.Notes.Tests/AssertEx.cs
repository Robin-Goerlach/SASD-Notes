namespace Sasd.Notes.Tests;

/// <summary>
/// Sehr kleine Hilfsklasse für paketfreie Prüfungen innerhalb des Test-Harness.
/// </summary>
internal static class AssertEx
{
    /// <summary>
    /// Prüft zwei Werte auf Gleichheit.
    /// </summary>
    public static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Erwartet: {expected}; Tatsächlich: {actual}");
        }
    }

    /// <summary>
    /// Prüft eine boolesche Bedingung.
    /// </summary>
    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

using System.Text.Json;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Erstellt das kontrollierte WebView2-Skript für einen Sprung zu einer gerenderten Überschrift.
/// </summary>
/// <remarks>
/// Der Überschriftstext wird als JSON-Zeichenfolge kodiert und ausschließlich
/// mit sichtbarem DOM-Text verglichen. Er kann daher keine Anweisungen einschleusen.
/// </remarks>
internal static class ResearchOutlineScriptBuilder
{
    /// <summary>
    /// Erstellt ein Skript, das eine bestimmte gerenderte Überschrift sichtbar macht.
    /// </summary>
    /// <param name="headingLevel">Markdown-Ebene zwischen eins und sechs.</param>
    /// <param name="headingText">Sichtbarer, vom Outline-Parser gelieferter Überschriftstext.</param>
    /// <param name="occurrence">Nullbasierte Wiederholung gleichlautender Überschriften derselben Ebene.</param>
    /// <returns>Festes JavaScript mit geprüften Zahlen und sicher kodiertem Vergleichstext.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Ebene oder Wiederholungsnummer liegen außerhalb des erlaubten Bereichs.</exception>
    /// <exception cref="ArgumentException">Der Überschriftstext ist leer.</exception>
    public static string Build(int headingLevel, string headingText, int occurrence)
    {
        if (headingLevel is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(headingLevel));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(headingText);
        ArgumentOutOfRangeException.ThrowIfNegative(occurrence);

        string encodedText = JsonSerializer.Serialize(headingText);
        return $$"""
            (() => {
              const headingText = {{encodedText}};
              const occurrence = {{occurrence}};
              const headings = document.querySelectorAll(".markdown-body h" + {{headingLevel}});
              let matchingIndex = 0;

              for (const heading of headings) {
                if ((heading.textContent ?? "").trim() !== headingText) {
                  continue;
                }

                if (matchingIndex++ !== occurrence) {
                  continue;
                }

                document.querySelectorAll(".sasd-outline-target").forEach(
                  element => element.classList.remove("sasd-outline-target"));
                heading.classList.add("sasd-outline-target");
                heading.scrollIntoView({ block: "start", behavior: "auto" });
                return true;
              }

              return false;
            })();
            """;
    }
}

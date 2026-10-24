using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft ATX-Randfälle und Originalpositionen unabhängig von der Benutzeroberfläche.
/// </summary>
internal static class OutlineSyntaxTests
{
    /// <summary>
    /// Prüft Einrückung, Abschlussrauten und die Abgrenzung zu Code und normalem Text.
    /// </summary>
    public static void Run()
    {
        var parser = new MarkdownOutlineParser();
        foreach (string newline in new[] { "\n", "\r\n" })
        {
            string[] lines =
            {
                "Vorspann 😀", "# Titel ###", " ## Abschnitt Ω ##  ", "   ### C#",
                "###### Tief ####", "# Text# bleibt", "# Text ### bleibt",
                "    # Eingerückter Code", "\t# Tab-Code", "####### Zu tief",
                "#Ohne Abstand", "# ###", "##   ", "\\# Maskiert",
                "```", "  # Im Code", "```", "  ## Nach Code ##"
            };
            string text = string.Join(newline, lines);
            var outline = parser.Parse(text);
            string[] expected = { "Titel", "Abschnitt Ω", "C#", "Tief", "Text# bleibt", "Text ### bleibt", "Nach Code" };
            int[] levels = { 1, 2, 3, 6, 1, 1, 2 };
            int[] sourceLines = { 1, 2, 3, 4, 5, 6, 17 };
            AssertEx.Equal(expected.Length, outline.Count, "Nur echte, nicht leere ATX-Überschriften dürfen erscheinen.");

            for (int index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], outline[index].HeadingText, "Der sichtbare Titel muss korrekt bereinigt werden.");
                AssertEx.Equal(levels[index], outline[index].Level, "Die Einrückung darf die Überschriftenebene nicht ändern.");
                int originalStart = text.IndexOf(lines[sourceLines[index]], StringComparison.Ordinal);
                AssertEx.Equal(originalStart, outline[index].StartIndex,
                    "Navigation muss auf den unveränderten Zeilenanfang einschließlich Einrückung zeigen.");
            }
        }

        AssertEx.Equal(0, parser.Parse(string.Empty).Count, "Leere Dokumente bleiben ohne Gliederung.");
        AssertEx.Equal("Text ###x", parser.Parse("# Text ###x")[0].HeadingText,
            "Eine Rautengruppe mit nachfolgendem Text ist kein Abschluss.");
    }
}

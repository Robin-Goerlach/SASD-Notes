using Sasd.Notes.Application.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft Bereichsübertragungen bei unterschiedlichen Zeilenenden ohne WinForms.
/// </summary>
internal static class EditorTextPositionMapperTests
{
    /// <summary>
    /// Prüft Hin-/Rückrichtung, Unicode, Grenzen und abweichende Texte.
    /// </summary>
    public static void Run()
    {
        foreach (string sourceNewline in new[] { "\r\n", "\n", "\r" })
        foreach (string destinationNewline in new[] { "\r\n", "\n", "\r" })
        {
            string source = "😀" + sourceNewline + "# Titel" + sourceNewline + "[[Ziel]]";
            string destination = "😀" + destinationNewline + "# Titel" + destinationNewline + "[[Ziel]]";
            int start = source.IndexOf("[[Ziel]]", StringComparison.Ordinal);
            var range = EditorTextPositionMapper.Map(source, destination, start, 8);
            AssertEx.True(range.HasValue, "Gleicher Inhalt mit anderen Zeilenenden muss übertragbar sein.");
            AssertEx.Equal(destination.IndexOf("[[Ziel]]", StringComparison.Ordinal), range.GetValueOrDefault().Start,
                "Die Zielposition muss alle vorherigen Zeilenenden berücksichtigen.");
            AssertEx.Equal("[[Ziel]]", destination.Substring(range.GetValueOrDefault().Start, range.GetValueOrDefault().Length),
                "Der vollständige Linktext muss erhalten bleiben.");
            var reverse = EditorTextPositionMapper.Map(destination, source, range.GetValueOrDefault().Start, range.GetValueOrDefault().Length);
            AssertEx.Equal(start, reverse!.Value.Start, "Auch die Rückrichtung muss den Originalbereich liefern.");
            var heading = EditorTextPositionMapper.Map(source, destination, source.IndexOf('#'), 0);
            AssertEx.Equal(destination.IndexOf('#'), heading!.Value.Start, "Outline-Cursorpositionen müssen ebenfalls zugeordnet werden.");
            var whole = EditorTextPositionMapper.Map(source, destination, 0, source.Length);
            AssertEx.Equal(destination.Length, whole!.Value.Length, "Ein Bereich über mehrere Zeilen muss seine Länge anpassen.");
            var eof = EditorTextPositionMapper.Map(source, destination, source.Length, 0);
            AssertEx.Equal(destination.Length, eof!.Value.Start, "Die Cursorposition am Dokumentende muss gültig sein.");
        }

        AssertEx.True(EditorTextPositionMapper.Map("", "", 0, 0).HasValue, "Leere Dokumente besitzen eine gültige Anfangsposition.");
        AssertEx.True(EditorTextPositionMapper.Map("a\r\nb", "a\nb", 2, 0) is null, "Die Mitte eines CRLF-Paars ist keine übertragbare Grenze.");
        AssertEx.True(EditorTextPositionMapper.Map("a\r\nb", "a\nb", 0, 2) is null, "Auch ein Bereichsende darf CRLF nicht trennen.");
        AssertEx.True(EditorTextPositionMapper.Map("abc", "axc", 0, 1) is null, "Abweichender Inhalt außerhalb der Auswahl darf nicht ignoriert werden.");
        AssertEx.True(EditorTextPositionMapper.Map("abc", "ab", 0, 1) is null, "Gekürzter Text darf keine scheinbar gültige Zuordnung ergeben.");
        AssertEx.True(EditorTextPositionMapper.Map("ab", "abc", 0, 1) is null, "Zusätzlicher Text muss ebenfalls erkannt werden.");
        AssertEx.True(EditorTextPositionMapper.Map("abc", "abc", -1, 1) is null, "Negative Positionen müssen abgewiesen werden.");
        AssertEx.True(EditorTextPositionMapper.Map("abc", "abc", 0, -1) is null, "Negative Längen müssen abgewiesen werden.");
        AssertEx.True(EditorTextPositionMapper.Map("abc", "abc", 1, int.MaxValue) is null, "Zu große Bereiche dürfen keinen Integerüberlauf erzeugen.");
    }
}

using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft Unicode-Grenzen von Suchvorschauen und eine reproduzierbare Trefferreihenfolge.
/// </summary>
internal static class SearchPreviewTests
{
    /// <summary>
    /// Führt Vorschauprüfungen ohne Dateisystem oder Oberfläche aus.
    /// </summary>
    public static void Run()
    {
        var search = new SearchService();
        NoteDocument fallback = CreateNote(new string('x', 119) + "😀 Ende");
        SearchHit fallbackHit = search.Search(new[] { fallback }, "Treffer").Single();
        AssertEx.Equal(-1, fallbackHit.MatchIndex, "Ein reiner Titeltreffer darf keine Inhaltsposition vortäuschen.");
        AssertEx.True(fallbackHit.PreviewText.Contains("😀", StringComparison.Ordinal),
            "Die Fallback-Vorschau darf das Emoji an der Abschneidegrenze nicht trennen.");
        AssertEx.True(fallbackHit.PreviewText.EndsWith(" ...", StringComparison.Ordinal),
            "Ein abgeschnittenes Vorschauende muss gekennzeichnet sein.");
        AssertValidSurrogates(fallbackHit.PreviewText);

        string startBoundary = new string('x', 9) + "😀" + new string('y', 34) + "Q Ende";
        SearchHit startHit = search.Search(new[] { CreateNote(startBoundary) }, "Q").Single();
        AssertEx.Equal(startBoundary.IndexOf('Q'), startHit.MatchIndex, "Die originale Fundposition darf nicht verschoben werden.");
        AssertEx.True(startHit.PreviewText.StartsWith("... 😀", StringComparison.Ordinal),
            "Der Vorschauanfang muss ein angeschnittenes Surrogatpaar vollständig aufnehmen.");
        AssertValidSurrogates(startHit.PreviewText);

        string endBoundary = "Q" + new string('x', 69) + "😀 Ende";
        SearchHit endHit = search.Search(new[] { CreateNote(endBoundary) }, "Q").Single();
        AssertEx.True(endHit.PreviewText.Contains("😀", StringComparison.Ordinal), "Auch der Kontextausschnitt darf ein Emoji am Ende nicht trennen.");
        AssertValidSurrogates(endHit.PreviewText);

        foreach (string newline in new[] { "\n", "\r\n" })
        {
            string content = "Vorher 😀" + newline + "Q nachher";
            SearchHit hit = search.Search(new[] { CreateNote(content) }, " Q ").Single();
            AssertEx.Equal(content.IndexOf('Q'), hit.MatchIndex, "LF/CRLF und Suchbegriff-Trimmen dürfen die Inhaltsposition nicht verändern.");
            AssertEx.False(hit.PreviewText.Contains('\n') || hit.PreviewText.Contains('\r'), "Die Vorschau soll einzeilig bleiben.");
        }

        SearchHit empty = search.Search(new[] { CreateNote(string.Empty) }, "Treffer").Single();
        AssertEx.Equal(string.Empty, empty.PreviewText, "Eine leere Notiz muss eine leere Vorschau liefern.");
        AssertEx.Equal(0, search.Search(new[] { fallback }, "   ").Count, "Leere Suchbegriffe dürfen keine Treffer liefern.");

        NoteDocument first = CreateNote("Q", "A/Notiz.md");
        NoteDocument second = CreateNote("Q", "B/Notiz.md");
        foreach (NoteDocument[] notes in new[] { new[] { second, first }, new[] { first, second } })
        {
            IReadOnlyList<SearchHit> hits = search.Search(notes, "Q");
            AssertEx.Equal("A/Notiz.md", hits[0].RelativePath, "Bei identischen Scores und Titeln muss der Pfad die Reihenfolge bestimmen.");
            AssertEx.Equal("B/Notiz.md", hits[1].RelativePath, "Gleichnamige Notizen dürfen nicht zusammengefasst werden.");
        }
    }

    private static NoteDocument CreateNote(string content, string relativePath = "Notiz.md") => new()
    {
        Id = relativePath, Title = "Treffer", FullPath = "/Vault/" + relativePath,
        RelativePath = relativePath, Content = content
    };

    /// <summary>
    /// Erkennt einzelne Surrogathälften, statt sich auf deren Darstellung in der Testkonsole zu verlassen.
    /// </summary>
    private static void AssertValidSurrogates(string text)
    {
        for (int index = 0; index < text.Length; index++)
        {
            if (char.IsHighSurrogate(text[index]))
            {
                AssertEx.True(index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]), "Ein hohes Surrogat benötigt seine zweite Hälfte.");
                index++;
            }
            else
            {
                AssertEx.False(char.IsLowSurrogate(text[index]), "Eine niedrige Surrogathälfte darf nicht allein stehen.");
            }
        }
    }
}

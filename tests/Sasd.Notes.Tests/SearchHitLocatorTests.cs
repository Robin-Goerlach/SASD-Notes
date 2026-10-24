using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft die sichere Navigation zu Suchtreffern nach nachträglichen Inhaltsänderungen.
/// </summary>
internal static class SearchHitLocatorTests
{
    /// <summary>
    /// Prüft unveränderte, verschobene, mehrdeutige, entfernte und ungültige Treffer.
    /// </summary>
    public static void Run()
    {
        SearchHit exact = CreateHit("ZIEL", 6);
        AssertRange((6, 4), SearchHitLocator.Locate(exact, "Alpha ZIEL Omega"),
            "Ein unveränderter Treffer muss seine ursprüngliche Position behalten.");

        SearchHit moved = CreateHit("Ziel", 0);
        AssertRange((10, 4), SearchHitLocator.Locate(moved, "Vorspann: Ziel"),
            "Eingefügter Text vor dem Treffer muss durch erneute Lokalisierung berücksichtigt werden.");

        SearchHit nearest = CreateHit("Ziel", 12);
        AssertRange((11, 4), SearchHitLocator.Locate(nearest, "Ziel.......Ziel........Ziel"),
            "Bei mehreren Vorkommen muss der nächstgelegene Treffer gewählt werden.");

        SearchHit equalDistance = CreateHit("Ziel", 5);
        AssertRange((0, 4), SearchHitLocator.Locate(equalDistance, "Ziel......Ziel"),
            "Bei gleicher Entfernung muss der frühere Treffer deterministisch gewinnen.");

        SearchHit changedCase = CreateHit("Ziel", 7);
        AssertRange((7, 4), SearchHitLocator.Locate(changedCase, "Siehe: ZIEL"),
            "Die Navigation muss dieselbe ordinale Groß-/Kleinschreibung wie die Suche verwenden.");

        SearchHit unicode = CreateHit("😀Z", 0);
        AssertRange((1, 3), SearchHitLocator.Locate(unicode, "X😀Z"),
            "UTF-16-Surrogatpaare müssen mit ihrer tatsächlichen Länge lokalisiert werden.");

        SearchHit overlapping = CreateHit("aaa", 10);
        AssertRange((2, 3), SearchHitLocator.Locate(overlapping, "aaaaa"),
            "Auch überlappende Vorkommen müssen bei der Entfernungsauswahl berücksichtigt werden.");

        AssertEx.True(SearchHitLocator.Locate(CreateHit("Entfernt", 3), "Aktueller Inhalt") is null,
            "Ein entfernter Treffer darf keinen beliebigen Bereich markieren.");
        AssertEx.True(SearchHitLocator.Locate(CreateHit(string.Empty, -1), "Titelinhalt") is null,
            "Ein reiner Titel- oder Pfadtreffer besitzt keine Inhaltsauswahl.");

        var malformed = new SearchHit
        {
            NoteTitle = "Notiz",
            RelativePath = "Notiz.md",
            PreviewText = "Vorschau",
            MatchIndex = int.MaxValue,
            MatchLength = 20,
            MatchedText = "kurz"
        };
        AssertEx.True(SearchHitLocator.Locate(malformed, "kurz") is null,
            "Inkonsistente oder überlaufgefährdete Trefferdaten müssen sicher abgewiesen werden.");

        var service = new SearchService();
        NoteDocument note = new()
        {
            Id = "Notiz.md",
            Title = "Notiz",
            FullPath = "C:/Vault/Notiz.md",
            RelativePath = "Notiz.md",
            Content = "Vorher ZIEL 😀 Nachher"
        };
        SearchHit serviceHit = service.Search(new[] { note }, "  ziel  ").Single();
        AssertEx.Equal("ZIEL", serviceHit.MatchedText,
            "Die Suche muss den tatsächlichen Originaltext und nicht die Schreibweise der Anfrage speichern.");
        AssertEx.Equal(serviceHit.MatchLength, serviceHit.MatchedText.Length,
            "Gespeicherter Treffertext und UTF-16-Länge müssen übereinstimmen.");
    }

    private static SearchHit CreateHit(string matchedText, int matchIndex)
    {
        return new SearchHit
        {
            NoteTitle = "Notiz",
            RelativePath = "Notiz.md",
            PreviewText = matchedText,
            MatchIndex = matchIndex,
            MatchLength = matchedText.Length,
            MatchedText = matchedText
        };
    }

    private static void AssertRange(
        (int Start, int Length) expected,
        (int Start, int Length)? actual,
        string message)
    {
        AssertEx.True(actual.HasValue, message);
        AssertEx.Equal(expected.Start, actual.GetValueOrDefault().Start, message);
        AssertEx.Equal(expected.Length, actual.GetValueOrDefault().Length, message);
    }
}

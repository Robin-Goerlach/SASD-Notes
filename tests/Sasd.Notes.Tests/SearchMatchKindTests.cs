using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft die fachliche Klassifikation von Titel-, Pfad- und Inhaltstreffern.
/// </summary>
internal static class SearchMatchKindTests
{
    /// <summary>
    /// Prüft einzelne und kombinierte Fundstellen, Score und Markierungsdaten.
    /// </summary>
    public static void Run()
    {
        const string query = "Nadel";
        var notes = new[]
        {
            CreateNote("Titel/NurTitel.md", "Eine Nadel im Titel", "Kein Fund im Text."),
            CreateNote("Nadelpfad/NurPfad.md", "Andere Überschrift", "Kein Fund im Text."),
            CreateNote("Inhalt/NurInhalt.md", "Andere Überschrift", "Die Nadel liegt hier."),
            CreateNote("Nadel/Kombiniert.md", "Nadel überall", "Auch dieser Inhalt enthält eine Nadel.")
        };

        IReadOnlyList<SearchHit> hits = new SearchService().Search(notes, query);
        AssertEx.Equal(4, hits.Count, "Alle vier kontrollierten Suchfälle müssen gefunden werden.");

        SearchHit title = Find(hits, "Titel/NurTitel.md");
        AssertEx.Equal(SearchMatchKind.Title, title.MatchKinds,
            "Ein reiner Titeltreffer darf nicht als Pfad- oder Inhaltstreffer gelten.");
        AssertEx.Equal(100, title.Score, "Ein reiner Titeltreffer benötigt den dokumentierten Score.");
        AssertEx.Equal(-1, title.MatchIndex, "Ein Titeltreffer darf keine Inhaltsposition vortäuschen.");

        SearchHit path = Find(hits, "Nadelpfad/NurPfad.md");
        AssertEx.Equal(SearchMatchKind.RelativePath, path.MatchKinds,
            "Ein reiner Pfadtreffer muss ausdrücklich als solcher klassifiziert sein.");
        AssertEx.Equal(40, path.Score, "Ein reiner Pfadtreffer benötigt den dokumentierten Score.");

        SearchHit content = Find(hits, "Inhalt/NurInhalt.md");
        AssertEx.Equal(SearchMatchKind.Content, content.MatchKinds,
            "Ein reiner Inhaltstreffer muss ausdrücklich als solcher klassifiziert sein.");
        AssertEx.Equal(50, content.Score, "Ein reiner Inhaltstreffer benötigt den dokumentierten Score.");
        AssertEx.Equal(query, content.MatchedText,
            "Der exakte Inhaltstreffer muss seine ursprüngliche Schreibweise behalten.");

        SearchHit combined = Find(hits, "Nadel/Kombiniert.md");
        SearchMatchKind allKinds =
            SearchMatchKind.Title | SearchMatchKind.RelativePath | SearchMatchKind.Content;
        AssertEx.Equal(allKinds, combined.MatchKinds,
            "Gleichzeitige Titel-, Pfad- und Inhaltsfunde müssen gemeinsam erhalten bleiben.");
        AssertEx.Equal(190, combined.Score,
            "Die Klassifikation darf die bestehende additive Bewertung nicht verändern.");
        AssertEx.Equal("Nadel", combined.MatchedText,
            "Bei einem kombinierten Treffer muss die Inhaltsmarkierung weiterhin funktionieren.");
    }

    private static SearchHit Find(IEnumerable<SearchHit> hits, string relativePath)
    {
        return hits.Single(hit => string.Equals(
            hit.RelativePath,
            relativePath,
            StringComparison.Ordinal));
    }

    private static NoteDocument CreateNote(
        string relativePath,
        string title,
        string content)
    {
        return new NoteDocument
        {
            Id = relativePath,
            Title = title,
            RelativePath = relativePath,
            FullPath = "/Vault/" + relativePath,
            Content = content
        };
    }
}

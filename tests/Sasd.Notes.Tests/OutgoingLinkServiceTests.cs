using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft die UI-unabhängige Übersicht ausgehender Wiki-Links.
/// </summary>
internal static class OutgoingLinkServiceTests
{
    /// <summary>
    /// Prüft Alias, Dokumentreihenfolge sowie aufgelöste, fehlende und mehrdeutige Ziele.
    /// </summary>
    public static void Run()
    {
        var parser = new WikiLinkParser();
        var service = new OutgoingLinkService(new LinkResolver());
        NoteDocument source = CreateNote(
            "Quelle.md",
            "Quelle",
            "[[Wissen/Ziel|Lesbarer Alias]] [[Fehlt]] [[Plan]]");
        source.Links = parser.Parse(source.Content);

        NoteDocument target = CreateNote("Wissen/Ziel.md", "Ziel", "# Ziel");
        NoteDocument ambiguousA = CreateNote("A/Plan.md", "Plan", "# A");
        NoteDocument ambiguousB = CreateNote("B/Plan.md", "Plan", "# B");
        IReadOnlyList<OutgoingLinkInfo> links = service.GetOutgoingLinks(
            source,
            new[] { source, target, ambiguousA, ambiguousB });

        AssertEx.Equal(3, links.Count,
            "Jedes erkannte Linkvorkommen muss in Dokumentreihenfolge erhalten bleiben.");
        AssertEx.Equal("Lesbarer Alias", links[0].DisplayText,
            "Ein Alias muss als sichtbarer Text erhalten bleiben.");
        AssertEx.Equal("Wissen/Ziel.md", links[0].ResolvedRelativePath ?? string.Empty,
            "Ein eindeutiger Pfadlink muss auf den relativen Zielpfad zeigen.");
        AssertEx.True(links[0].IsResolved,
            "Ein eindeutig gefundenes Ziel muss als aufgelöst gekennzeichnet sein.");

        AssertEx.Equal("Fehlt", links[1].Target,
            "Ein fehlendes Ziel muss für Diagnose und spätere Erstellung erhalten bleiben.");
        AssertEx.False(links[1].IsResolved,
            "Ein fehlendes Ziel darf nicht als navigierbar erscheinen.");

        AssertEx.Equal("Plan", links[2].Target,
            "Auch ein mehrdeutiges Kurzziel muss unverändert sichtbar bleiben.");
        AssertEx.False(links[2].IsResolved,
            "Ein mehrdeutiges Ziel darf nicht willkürlich einer Notiz zugeordnet werden.");
    }

    private static NoteDocument CreateNote(string relativePath, string title, string content)
    {
        return new NoteDocument
        {
            Id = relativePath,
            Title = title,
            FullPath = "/Vault/" + relativePath,
            RelativePath = relativePath,
            Content = content
        };
    }
}

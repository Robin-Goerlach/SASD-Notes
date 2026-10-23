using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft Pfadtreue und eindeutige Wiki-Link-Ziele unabhängig von Reihenfolge und Betriebssystem.
/// </summary>
internal static class LinkResolutionTests
{
    /// <summary>
    /// Prüft Kurzlinks, optionale Dateiendungen, Ordnerpfade, Mehrdeutigkeiten und die gemeinsame Backlink-Regel.
    /// </summary>
    public static void Run()
    {
        var resolver = new LinkResolver();
        NoteDocument firstPlan = CreateNote("Archiv/Plan.md");
        NoteDocument secondPlan = CreateNote("Projekte/Plan.md");
        NoteDocument uniqueNote = CreateNote("Wissen/Überblick 😀.md");
        NoteDocument windowsPathNote = CreateNote("Thema\\Unterordner\\Windows.md");
        NoteDocument[] notes = { firstPlan, secondPlan, uniqueNote, windowsPathNote };

        // Beide Reihenfolgen müssen gleichwertige Ergebnisse liefern: Ein Scan darf keine fachliche Priorität erzeugen.
        foreach (NoteDocument[] orderedNotes in new[] { notes, notes.Reverse().ToArray() })
        {
            AssertTarget(resolver, orderedNotes, "Projekte/Plan", secondPlan);
            AssertTarget(resolver, orderedNotes, "Projekte/Plan.md", secondPlan);
            AssertTarget(resolver, orderedNotes, "  projekte/PLAN.MD  ", secondPlan);
            AssertTarget(resolver, orderedNotes, "Projekte\\Plan.md", secondPlan);
            AssertTarget(resolver, orderedNotes, "Archiv/Plan.md", firstPlan);
            AssertTarget(resolver, orderedNotes, "Wissen/Überblick 😀.md", uniqueNote);
            AssertTarget(resolver, orderedNotes, "Überblick 😀", uniqueNote);
            AssertTarget(resolver, orderedNotes, "Überblick 😀.md", uniqueNote);
            AssertTarget(resolver, orderedNotes, "Thema/Unterordner/Windows.md", windowsPathNote);
            AssertTarget(resolver, orderedNotes, "Windows", windowsPathNote);

            // Ein ausdrücklich angegebener, falscher Ordner darf selbst bei eindeutigem Dateinamen nicht ignoriert werden.
            AssertTarget(resolver, orderedNotes, "Fehlt/Überblick 😀", null);
            AssertTarget(resolver, orderedNotes, "Fehlt/Plan", null);
            AssertTarget(resolver, orderedNotes, "Fehlt/Plan.md", null);
            AssertTarget(resolver, orderedNotes, "../Plan", null);
            AssertTarget(resolver, orderedNotes, "/Plan", null);
            AssertTarget(resolver, orderedNotes, "./Projekte/Plan", null);
            AssertTarget(resolver, orderedNotes, "Plan", null);
            AssertTarget(resolver, orderedNotes, "Plan.md", null);
            AssertTarget(resolver, orderedNotes, string.Empty, null);
            AssertTarget(resolver, orderedNotes, " \t ", null);
            AssertTarget(resolver, orderedNotes, ".md", null);
            AssertTarget(resolver, orderedNotes, "Nicht vorhanden", null);
        }

        // Ein Kurzlink bleibt gültig, sobald sein Dateiname innerhalb des Vaults eindeutig ist.
        AssertTarget(resolver, new[] { secondPlan }, "Plan.md", secondPlan);
        AssertTarget(resolver, Array.Empty<NoteDocument>(), "Plan", null);

        // Eine Datei direkt im Vault-Wurzelverzeichnis ist ein exakter Pfad und hat Vorrang vor der Namenssuche.
        NoteDocument rootPlan = CreateNote("Plan.md");
        NoteDocument[] withRoot = notes.Append(rootPlan).ToArray();
        AssertTarget(resolver, withRoot, "Plan", rootPlan);
        AssertTarget(resolver, withRoot.Reverse().ToArray(), "Plan.md", rootPlan);
        AssertTarget(resolver, withRoot, "Fehlt/Plan.md", null);

        // Auch eine Kollision nach Case-/Separator-Normalisierung muss sichtbar unaufgelöst bleiben.
        NoteDocument conflictingPath = CreateNote("PROJEKTE\\PLAN.MD");
        AssertTarget(resolver, new[] { secondPlan, conflictingPath }, "Projekte/Plan.md", null);
        AssertTarget(resolver, new[] { conflictingPath, secondPlan }, "Projekte/Plan", null);

        RunBacklinkTests(resolver, firstPlan, secondPlan);
    }

    /// <summary>
    /// Ein falsch aufgelöster Pfad darf auch keine Rückverweise auf eine andere gleichnamige Datei erzeugen.
    /// </summary>
    private static void RunBacklinkTests(LinkResolver resolver, NoteDocument firstPlan, NoteDocument secondPlan)
    {
        const string sourceText = "[[Projekte/Plan.md|Projektplan]] [[Fehlt/Plan]] [[Plan]]";
        NoteDocument source = CreateNote("Quelle.md", sourceText);
        NoteDocument[] notes = { firstPlan, source, secondPlan };
        var backlinks = new BacklinkService(resolver);
        WikiLink explicitLink = source.Links[0];

        AssertEx.Equal("Projektplan", explicitLink.Alias, "Der Alias muss unabhängig vom Ziel erhalten bleiben.");
        AssertTarget(resolver, notes, explicitLink.Target, secondPlan);

        IReadOnlyList<BacklinkInfo> secondBacklinks = backlinks.GetBacklinks(secondPlan, notes);
        AssertEx.Equal(1, secondBacklinks.Count,
            "Nur der explizite gültige Pfad darf als Rückverweis zählen; fehlende und mehrdeutige Ziele zählen nicht.");
        AssertEx.Equal("Quelle.md", secondBacklinks[0].SourceRelativePath, "Der Rückverweis muss die richtige Quelle nennen.");
        AssertEx.Equal(explicitLink.RawText, secondBacklinks[0].ContextSnippet, "Der Rückverweis muss zum expliziten Link gehören.");
        AssertEx.Equal(0, backlinks.GetBacklinks(firstPlan, notes).Count,
            "Die gleichnamige Datei im anderen Ordner darf keine falschen Rückverweise erhalten.");

        // Auch die Backlink-Suche darf nicht davon abhängen, in welcher Reihenfolge Dateien gefunden wurden.
        AssertEx.Equal(1, backlinks.GetBacklinks(secondPlan, notes.Reverse().ToArray()).Count,
            "Die Reihenfolge darf die Zahl korrekt aufgelöster Rückverweise nicht ändern.");
    }

    private static void AssertTarget(
        LinkResolver resolver,
        IReadOnlyList<NoteDocument> notes,
        string target,
        NoteDocument? expected)
    {
        NoteDocument? actual = resolver.Resolve(target, notes);
        AssertEx.True(ReferenceEquals(expected, actual),
            $"Unerwartetes Linkziel für '{target}': erwartet '{expected?.RelativePath ?? "(unaufgelöst)"}', erhalten '{actual?.RelativePath ?? "(unaufgelöst)"}'.");
    }

    private static NoteDocument CreateNote(string relativePath, string content = "")
    {
        // Die Testpfade sind fachliche Wiki-Pfade. Es werden keine Dateien angelegt oder gelesen.
        string normalizedPath = relativePath.Replace('\\', '/');
        string fileName = normalizedPath[(normalizedPath.LastIndexOf('/') + 1)..];
        return new NoteDocument
        {
            Id = normalizedPath,
            FullPath = "/TestVault/" + normalizedPath,
            RelativePath = relativePath,
            Title = fileName[..^3],
            Content = content,
            Links = new WikiLinkParser().Parse(content)
        };
    }
}

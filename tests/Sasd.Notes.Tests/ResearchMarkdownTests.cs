using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft die gemeinsame Vorbereitung anklickbarer Wiki-Links für die Rechercheansicht.
/// </summary>
internal static class ResearchMarkdownTests
{
    /// <summary>
    /// Prüft Auflösung, Aliasdarstellung, Codeausschluss, Unicode und den Markdig-Durchlauf.
    /// </summary>
    public static void Run()
    {
        var parser = new WikiLinkParser();
        var resolver = new LinkResolver();
        var service = new ResearchMarkdownService(parser, resolver);

        NoteDocument projectPlan = CreateNote("Projekte/Plan.md");
        NoteDocument unicodeNote = CreateNote("Wissen/Überblick 😀.md");
        NoteDocument[] notes = { projectPlan, unicodeNote };

        const string markdown =
            "Siehe [[Projekte/Plan|Plan [A] *wichtig*]] und [[Überblick 😀]].\n\n" +
            "Fehlt: [[Nicht vorhanden]].\n\n" +
            "`[[Projekte/Plan]]`";
        string prepared = service.PrepareForRendering(markdown, notes);

        string projectUri = ResearchMarkdownService.InternalNoteUriPrefix
            + Uri.EscapeDataString(projectPlan.RelativePath);
        string unicodeUri = ResearchMarkdownService.InternalNoteUriPrefix
            + Uri.EscapeDataString(unicodeNote.RelativePath);

        AssertEx.True(
            prepared.Contains($"[Plan \\[A\\] \\*wichtig\\*]({projectUri})", StringComparison.Ordinal),
            "Ein Alias muss als sicher maskierter Linktext auf den aufgelösten relativen Pfad zeigen.");
        AssertEx.True(
            prepared.Contains($"[Überblick 😀]({unicodeUri})", StringComparison.Ordinal),
            "Ein eindeutiger Unicode-Kurzlink muss auf den vollständigen Zielpfad zeigen.");
        AssertEx.True(
            prepared.Contains("[[Nicht vorhanden]]", StringComparison.Ordinal),
            "Ein nicht vorhandenes Ziel muss sichtbar und unverändert bleiben.");
        AssertEx.True(
            prepared.Contains("`[[Projekte/Plan]]`", StringComparison.Ordinal),
            "Wiki-Link-Beispiele in Inline-Code dürfen nicht navigierbar gemacht werden.");

        // Der Integrationscheck stellt sicher, dass Markdig das interne Schema als Link erhält.
        string html = new MarkdigMarkdownHtmlRenderer().Render(prepared);
        AssertEx.True(
            html.Contains($"href=\"{projectUri}\"", StringComparison.Ordinal),
            "Der Markdown-Renderer muss den vorbereiteten internen Link als HTML-Anker ausgeben.");

        AssertEx.Equal(
            markdown,
            service.PrepareForRendering(markdown, Array.Empty<NoteDocument>()),
            "Ohne auflösbare Ziele darf der ursprüngliche Markdown-Arbeitsstand nicht verändert werden.");
    }

    /// <summary>
    /// Erstellt eine rein im Arbeitsspeicher verwendete Testnotiz.
    /// </summary>
    private static NoteDocument CreateNote(string relativePath)
    {
        string fileName = relativePath[(relativePath.LastIndexOf('/') + 1)..];
        return new NoteDocument
        {
            Id = relativePath,
            FullPath = "/TestVault/" + relativePath,
            RelativePath = relativePath,
            Title = fileName[..^3],
            Content = string.Empty
        };
    }
}

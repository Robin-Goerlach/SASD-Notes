using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft, dass eingefasste Markdown-Beispiele keine echten Links, Backlinks oder Outline-Einträge erzeugen.
/// </summary>
internal static class MarkdownCodeBlockTests
{
    /// <summary>
    /// Führt Parser- und Backlink-Prüfungen einschließlich unveränderter Originalpositionen aus.
    /// </summary>
    public static void Run()
    {
        foreach (string newline in new[] { "\n", "\r\n" })
        {
            RunPositionTests(newline);
            RunFenceTests(newline);
        }

        RunBacklinkTests();
    }

    /// <summary>
    /// Unicode und Codeblöcke dürfen die Positionen der verbleibenden echten Treffer nicht verschieben.
    /// </summary>
    private static void RunPositionTests(string newline)
    {
        string markdown = string.Join(newline, new[]
        {
            "# Vorher 😀",
            "[[Vorher]]",
            "```markdown",
            "# Nur Beispiel",
            "[[KeinLink|Beispiel]]",
            "```",
            "## Nachher Ω",
            "[[Nachher|Alias]]"
        });

        IReadOnlyList<WikiLink> links = new WikiLinkParser().Parse(markdown);
        IReadOnlyList<OutlineItem> outline = new MarkdownOutlineParser().Parse(markdown);
        AssertEx.Equal(2, links.Count, "Nur die Wiki-Links außerhalb des Codeblocks dürfen gefunden werden.");
        AssertEx.Equal(2, outline.Count, "Nur die Überschriften außerhalb des Codeblocks dürfen in die Gliederung.");
        AssertEx.Equal("Nachher", links[1].Target, "Das echte Linkziel nach dem Codeblock muss erhalten bleiben.");
        AssertEx.Equal("Alias", links[1].Alias, "Der echte Alias muss erhalten bleiben.");
        AssertEx.Equal(markdown.IndexOf("[[Nachher", StringComparison.Ordinal), links[1].StartIndex,
            "Die Linkposition muss sich weiterhin auf den Originaltext beziehen.");
        AssertEx.Equal(links[1].RawText, markdown.Substring(links[1].StartIndex, links[1].Length),
            "Länge und Start müssen den originalen Linktext exakt beschreiben.");
        AssertEx.Equal(markdown.IndexOf("## Nachher", StringComparison.Ordinal), outline[1].StartIndex,
            "Auch die Outline-Position muss CRLF und Unicode unverändert berücksichtigen.");
        AssertEx.Equal(2, outline[1].Level, "Die Ebene der echten Überschrift muss erhalten bleiben.");

        // Horizontale Trennzeichen dürfen keine nachfolgende Textzeile als Überschrift oder Linkinhalt verschlucken.
        IReadOnlyList<OutlineItem> boundaryOutline = new MarkdownOutlineParser().Parse(
            "#" + newline + "Kein Titel" + newline + "# " + newline + "Auch kein Titel" + newline + "## Echter Titel");
        AssertEx.Equal(1, boundaryOutline.Count, "Leere Überschriften dürfen keine Folgezeilen als Titel übernehmen.");
        IReadOnlyList<WikiLink> boundaryLinks = new WikiLinkParser().Parse("[[Über" + newline + "Zeilen]] [[   ]] [[Echt]]");
        AssertEx.Equal(1, boundaryLinks.Count, "Leere oder über Zeilengrenzen laufende Wiki-Links dürfen nicht erkannt werden.");
        AssertEx.Equal("Echt", boundaryLinks[0].Target, "Der nachfolgende gültige Link muss erhalten bleiben.");
    }

    /// <summary>
    /// Prüft Fence-Art, Länge, Einrückung, Infoangaben und noch nicht geschlossene Blöcke.
    /// </summary>
    private static void RunFenceTests(string newline)
    {
        string hidden = "# Beispiel" + newline + "[[Verborgen]]";
        string visible = "# Sichtbar" + newline + "[[Sichtbar]]";
        string fence = "```";

        AssertCounts("~~~csharp" + newline + hidden + newline + "~~~" + newline + visible, 1, 1);
        AssertCounts("   " + fence + "text" + newline + hidden + newline + "  " + fence + " \t" + newline + visible, 1, 1);
        AssertCounts(fence + "`" + newline + hidden + newline + fence + newline + hidden +
            newline + fence + "``" + newline + visible, 1, 1);
        AssertCounts(fence + newline + hidden + newline + "~~~" + newline + hidden +
            newline + fence + newline + visible, 1, 1);
        AssertCounts(fence + newline + hidden + newline + fence + "csharp" + newline + hidden +
            newline + fence + newline + visible, 1, 1);

        // Ein offener Block reicht bis EOF; auch Markerzeilen selbst erzeugen keine Links.
        AssertCounts(visible + newline + fence + " [[InfoIstKeinLink]]" + newline + hidden, 1, 1);
        AssertCounts(fence + newline + hidden, 0, 0);
        AssertCounts(fence + newline + hidden + newline + fence, 0, 0);
        AssertCounts("~~~" + newline + hidden, 0, 0);
        AssertCounts(fence, 0, 0);

        // Backticks in der Infoangabe verhindern die Interpretation als öffnende Backtick-Fence.
        AssertCounts(fence + " info`" + newline + visible, 1, 1);
        AssertCounts("    " + fence + newline + visible, 1, 1);
        AssertCounts(fence + newline + fence + newline + "~~~" + newline + hidden + newline + "~~~" + newline + visible, 1, 1);
        AssertCounts(string.Empty, 0, 0);
    }

    private static void AssertCounts(string markdown, int expectedLinks, int expectedHeadings)
    {
        AssertEx.Equal(expectedLinks, new WikiLinkParser().Parse(markdown).Count,
            "Codeblöcke müssen bei der Wiki-Link-Erkennung korrekt ausgegrenzt werden.");
        AssertEx.Equal(expectedHeadings, new MarkdownOutlineParser().Parse(markdown).Count,
            "Die Gliederung muss dieselben Codeblockgrenzen berücksichtigen.");
    }

    /// <summary>
    /// Codebeispiele dürfen auch bei real vorhandenen Zielen keine Rückverweise erzeugen.
    /// </summary>
    private static void RunBacklinkTests()
    {
        var parser = new WikiLinkParser();
        var backlinkService = new BacklinkService(new LinkResolver());
        var target = new NoteDocument
        {
            Id = "Ziel.md", Title = "Ziel", FullPath = "/Vault/Ziel.md",
            RelativePath = "Ziel.md", Content = string.Empty
        };
        var source = new NoteDocument
        {
            Id = "Quelle.md", Title = "Quelle", FullPath = "/Vault/Quelle.md",
            RelativePath = "Quelle.md", Content = "```markdown\n[[Ziel]]"
        };
        source.Links = parser.Parse(source.Content);
        AssertEx.Equal(0, backlinkService.GetBacklinks(target, new[] { source, target }).Count,
            "Ein Wiki-Link im offenen Codeblock darf keinen Backlink erzeugen.");

        // Die nächste Auswertung beginnt am aktuellen Text; ein vorher offener Block darf nicht im Parserzustand hängenbleiben.
        source.Content += "\n```\n[[Ziel]]";
        source.Links = parser.Parse(source.Content);
        IReadOnlyList<BacklinkInfo> backlinks = backlinkService.GetBacklinks(target, new[] { source, target });
        AssertEx.Equal(1, backlinks.Count, "Nach dem Schließen darf ausschließlich der echte Link einen Backlink erzeugen.");
        AssertEx.Equal("[[Ziel]]", backlinks[0].ContextSnippet, "Der Rückverweis muss zum echten Link gehören.");
    }
}

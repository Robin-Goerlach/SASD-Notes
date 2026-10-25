using Sasd.Notes.App.WinForms;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Prüft die WinForms-nahe Darstellung klassifizierter Suchtreffer.
/// </summary>
internal static class SearchHitFormattingTests
{
    /// <summary>
    /// Prüft kombinierte Kategorien, Reihenfolge und unklassifizierte Altdaten.
    /// </summary>
    public static void Run()
    {
        var combined = new SearchHit
        {
            NoteTitle = "Projektplan",
            RelativePath = "Projekte/Projektplan.md",
            PreviewText = "Nadel im Text",
            MatchKinds = SearchMatchKind.Title | SearchMatchKind.Content
        };

        string combinedText = ListItemFormatting.FormatSearchHit(combined);
        Require(combinedText.StartsWith("[Titel + Inhalt]", StringComparison.Ordinal),
            "Kombinierte Treffer müssen in einer stabilen, verständlichen Reihenfolge erscheinen.");
        Require(combinedText.Contains("Projekte/Projektplan.md", StringComparison.Ordinal),
            "Die formatierte Zeile muss den relativen Pfad behalten.");

        var legacy = new SearchHit
        {
            NoteTitle = "Altbestand",
            RelativePath = "Altbestand.md",
            PreviewText = string.Empty
        };
        Require(ListItemFormatting.FormatSearchHit(legacy).StartsWith("[Treffer]", StringComparison.Ordinal),
            "Ein unklassifizierter Treffer muss weiterhin verständlich dargestellt werden.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

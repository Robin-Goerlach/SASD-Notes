using Sasd.Notes.App.WinForms;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Prüft die sichere Erzeugung des kontrollierten Gliederungssprung-Skripts.
/// </summary>
internal static class ResearchOutlineScriptBuilderTests
{
    /// <summary>
    /// Prüft die semantische DOM-Auswahl, sichere Textkodierung und ungültige Eingaben.
    /// </summary>
    public static void Run()
    {
        const string hostileHeading = "</script>'; window.unwanted = true; // 😀";
        string script = ResearchOutlineScriptBuilder.Build(3, hostileHeading, 1);

        Require(script.Contains("document.querySelectorAll(\".markdown-body h\" + 3)", StringComparison.Ordinal),
            "Das Skript muss ausschließlich Überschriften der geprüften Ebene auswählen.");
        Require(script.Contains("const occurrence = 1", StringComparison.Ordinal),
            "Die geprüfte Wiederholungsnummer muss unverändert in das feste Skript eingehen.");
        Require(script.Contains("textContent", StringComparison.Ordinal),
            "Der Überschriftstext darf ausschließlich als sichtbarer Text verglichen werden.");
        Require(script.Contains("scrollIntoView", StringComparison.Ordinal),
            "Die ausgewählte Überschrift muss sichtbar gemacht werden.");
        Require(script.Contains("sasd-outline-target", StringComparison.Ordinal),
            "Das Sprungziel muss für den Leser kenntlich gemacht werden.");
        Require(!script.Contains("innerHTML", StringComparison.Ordinal),
            "Ein Gliederungssprung darf keinen HTML-Inhalt schreiben.");
        Require(!script.Contains(hostileHeading, StringComparison.Ordinal),
            "Notizinhalt darf nicht unkodiert im Hostskript erscheinen.");
        Require(script.Contains("\\u003C/script\\u003E", StringComparison.Ordinal),
            "HTML-Steuerzeichen müssen durch den JSON-Encoder neutralisiert werden.");

        RequireThrows<ArgumentOutOfRangeException>(
            () => ResearchOutlineScriptBuilder.Build(0, "Titel", 0),
            "Eine Überschriftsebene unter eins muss abgewiesen werden.");
        RequireThrows<ArgumentOutOfRangeException>(
            () => ResearchOutlineScriptBuilder.Build(7, "Titel", 0),
            "Eine Überschriftsebene über sechs muss abgewiesen werden.");
        RequireThrows<ArgumentException>(
            () => ResearchOutlineScriptBuilder.Build(2, " ", 0),
            "Ein leerer Überschriftstext muss abgewiesen werden.");
        RequireThrows<ArgumentOutOfRangeException>(
            () => ResearchOutlineScriptBuilder.Build(2, "Titel", -1),
            "Eine negative Wiederholungsnummer muss abgewiesen werden.");
    }

    private static void RequireThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

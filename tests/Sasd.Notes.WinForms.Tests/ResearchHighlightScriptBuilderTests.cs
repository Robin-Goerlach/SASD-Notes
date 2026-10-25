using Sasd.Notes.App.WinForms;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Prüft die sichere Erzeugung des kontrollierten Suchhervorhebungs-Skripts.
/// </summary>
internal static class ResearchHighlightScriptBuilderTests
{
    /// <summary>
    /// Prüft Struktur, Unicode und die JSON-Kodierung potenziell gefährlicher Suchtexte.
    /// </summary>
    public static void Run()
    {
        const string hostileText = "</script>'; window.unwanted = true; // 😀";
        string script = ResearchHighlightScriptBuilder.Build(hostileText);

        Require(script.Contains("document.createElement(\"mark\")", StringComparison.Ordinal),
            "Das Skript muss einen semantischen Markierungsbereich erzeugen.");
        Require(script.Contains("textContent", StringComparison.Ordinal),
            "Der Treffer darf ausschließlich als Textinhalt eingesetzt werden.");
        Require(!script.Contains(hostileText, StringComparison.Ordinal),
            "Unvertrauenswürdiger Suchtext darf nicht unkodiert im Skript erscheinen.");
        Require(!script.Contains("</script>", StringComparison.OrdinalIgnoreCase),
            "Der Suchtext darf keinen Script-Kontext beenden können.");
        Require(script.Contains("\\u003C/script\\u003E", StringComparison.Ordinal),
            "HTML-Steuerzeichen müssen durch den JSON-Encoder neutralisiert werden.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

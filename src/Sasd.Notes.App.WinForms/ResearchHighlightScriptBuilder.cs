using System.Text.Json;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Erstellt das fest vorgegebene WebView2-Skript zur Hervorhebung eines sichtbaren Suchtreffers.
/// </summary>
/// <remarks>
/// Der Suchtext wird als JSON-Zeichenfolge kodiert. Dadurch kann Notizinhalt keine
/// zusätzlichen JavaScript-Anweisungen in das kontrollierte Hostskript einfügen.
/// </remarks>
internal static class ResearchHighlightScriptBuilder
{
    /// <summary>
    /// Erstellt ein Skript, das den ersten exakt passenden sichtbaren Textknoten markiert und zentriert.
    /// </summary>
    /// <param name="matchedText">Exakter Inhaltstreffer aus dem aktuellen Markdown-Arbeitsstand.</param>
    /// <returns>Festes JavaScript mit sicher kodiertem Suchtext.</returns>
    public static string Build(string matchedText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matchedText);

        string encodedText = JsonSerializer.Serialize(matchedText);
        return $$"""
            (() => {
              const needle = {{encodedText}};
              const root = document.querySelector(".markdown-body");
              if (!root || needle.length === 0) {
                return false;
              }

              const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
              let node;
              while ((node = walker.nextNode()) !== null) {
                const value = node.nodeValue ?? "";
                const matchIndex = value.indexOf(needle);
                if (matchIndex < 0 || node.parentNode === null) {
                  continue;
                }

                // Der Textknoten wird ohne innerHTML zerlegt. Der Treffer bleibt damit
                // reiner Text und kann keine HTML-Struktur oder Ereignisse einschleusen.
                const fragment = document.createDocumentFragment();
                fragment.append(document.createTextNode(value.substring(0, matchIndex)));

                const marker = document.createElement("mark");
                marker.className = "sasd-search-hit";
                marker.textContent = value.substring(matchIndex, matchIndex + needle.length);
                fragment.append(marker);

                fragment.append(document.createTextNode(value.substring(matchIndex + needle.length)));
                node.parentNode.replaceChild(fragment, node);
                marker.scrollIntoView({ block: "center", behavior: "auto" });
                return true;
              }

              return false;
            })();
            """;
    }
}

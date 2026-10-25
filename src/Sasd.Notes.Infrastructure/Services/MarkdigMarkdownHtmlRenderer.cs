using Markdig;
using Sasd.Notes.Application.Interfaces;

namespace Sasd.Notes.Infrastructure.Services;

/// <summary>
/// Rendert Markdown mit Markdig als eigenständiges, sicher eingeschränktes HTML-Dokument.
/// </summary>
public sealed class MarkdigMarkdownHtmlRenderer : IMarkdownHtmlRenderer
{
    private const string DocumentPrefix = """
        <!doctype html>
        <html lang="de">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <meta http-equiv="Content-Security-Policy"
                content="default-src 'none'; img-src data:; style-src 'unsafe-inline'; font-src 'none'; frame-src 'none'; object-src 'none';">
          <style>
            :root {
              color-scheme: light dark;
              font-family: "Segoe UI", Arial, sans-serif;
              line-height: 1.6;
            }
            body {
              margin: 0;
              color: #202124;
              background: #ffffff;
            }
            .markdown-body {
              max-width: 920px;
              margin: 0 auto;
              padding: 28px 38px 64px;
            }
            h1, h2, h3, h4, h5, h6 {
              line-height: 1.25;
              margin-top: 1.45em;
              margin-bottom: 0.55em;
            }
            h1, h2 {
              border-bottom: 1px solid #d8dee4;
              padding-bottom: 0.25em;
            }
            p, ul, ol, blockquote, pre, table {
              margin-top: 0;
              margin-bottom: 1em;
            }
            blockquote {
              margin-left: 0;
              padding: 0.2em 1em;
              color: #57606a;
              border-left: 4px solid #8c959f;
            }
            code {
              padding: 0.15em 0.35em;
              border-radius: 4px;
              background: #eff1f3;
              font-family: Consolas, "Cascadia Mono", monospace;
            }
            pre {
              overflow: auto;
              padding: 1em;
              border-radius: 6px;
              background: #f6f8fa;
            }
            pre code {
              padding: 0;
              background: transparent;
            }
            table {
              border-collapse: collapse;
            }
            th, td {
              padding: 0.4em 0.7em;
              border: 1px solid #d0d7de;
            }
            a {
              color: #0969da;
            }
            mark.sasd-search-hit {
              padding: 0.05em 0.15em;
              color: #24292f;
              background: #fff08a;
              border: 1px solid #d4b106;
              border-radius: 3px;
            }
            img {
              max-width: 100%;
            }
            @media (prefers-color-scheme: dark) {
              body {
                color: #e6edf3;
                background: #0d1117;
              }
              h1, h2, th, td {
                border-color: #30363d;
              }
              blockquote {
                color: #9da7b3;
                border-color: #6e7681;
              }
              code {
                background: #252b33;
              }
              pre {
                background: #161b22;
              }
              a {
                color: #58a6ff;
              }
              mark.sasd-search-hit {
                color: #ffffff;
                background: #7a5d00;
                border-color: #c9a227;
              }
            }
          </style>
        </head>
        <body>
          <main class="markdown-body">
        """;

    private const string DocumentSuffix = """
          </main>
        </body>
        </html>
        """;

    private readonly MarkdownPipeline _pipeline;

    /// <summary>
    /// Initialisiert den Renderer mit den gebräuchlichen, rein darstellenden Markdig-Erweiterungen.
    /// </summary>
    public MarkdigMarkdownHtmlRenderer()
    {
        // Eingebettetes HTML wird nicht übernommen. Die erweiterte Markdown-Pipeline
        // liefert unter anderem Tabellen, Aufgabenlisten und Durchstreichung.
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml()
            .Build();
    }

    /// <inheritdoc />
    public string Render(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        // Markdig erzeugt ausschließlich den Body. Der feste Rahmen ergänzt Darstellung
        // und eine restriktive CSP, ohne den übergebenen Markdown-Text zu verändern.
        string body = Markdown.ToHtml(markdown, _pipeline);
        return string.Concat(DocumentPrefix, body, DocumentSuffix);
    }
}

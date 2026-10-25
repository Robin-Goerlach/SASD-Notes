using Sasd.Notes.Application.Models;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;
using Sasd.Notes.Infrastructure.Services;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Einstiegspunkt der Windows-Forms-Anwendung.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Startet die Anwendung und baut die benötigten Dienste auf.
    /// </summary>
    /// <param name="args">Optional ein Ordner oder eine Markdown-Datei als Startziel.</param>
    /// <returns>Null nach regulärem Beenden; eins bei ungültigen Startargumenten.</returns>
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        StartupTarget? startupTarget;
        try
        {
            string? startupPath = StartupArgumentsParser.Parse(args, Environment.CurrentDirectory);
            startupTarget = startupPath is null ? null : new FileSystemStartupTargetResolver().Resolve(startupPath);
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // Ungültige explizite Ziele dürfen nicht stillschweigend den zuletzt verwendeten Vault öffnen.
            MessageBox.Show(
                "Das Startziel konnte nicht geöffnet werden." + Environment.NewLine + Environment.NewLine +
                exception.Message + Environment.NewLine + Environment.NewLine +
                "Aufruf: Sasd.Notes.App.WinForms.exe [Ordner oder Markdown-Datei]",
                "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }

        IWikiLinkParser wikiLinkParser = new WikiLinkParser();
        IOutlineParser outlineParser = new MarkdownOutlineParser();
        ILinkResolver linkResolver = new LinkResolver();

        var vaultRepository = new FileSystemVaultRepository(wikiLinkParser);
        var settingsRepository = new JsonSettingsRepository();
        var searchService = new SearchService();
        var backlinkService = new BacklinkService(linkResolver);
        var outgoingLinkService = new OutgoingLinkService(linkResolver);
        var editorFormattingService = new EditorFormattingService();
        var markdownRenderer = new MarkdigMarkdownHtmlRenderer();
        var researchMarkdownService = new ResearchMarkdownService(wikiLinkParser, linkResolver);

        var workspaceService = new NotesWorkspaceService(
            vaultRepository,
            settingsRepository,
            wikiLinkParser,
            outlineParser,
            linkResolver,
            searchService,
            backlinkService,
            outgoingLinkService);

        // Der vollqualifizierte Name vermeidet die Kollision mit dem Namespace Sasd.Notes.Application.
        System.Windows.Forms.Application.Run(new MainForm(
            workspaceService,
            editorFormattingService,
            startupTarget,
            markdownRenderer,
            researchMarkdownService));
        return 0;
    }
}

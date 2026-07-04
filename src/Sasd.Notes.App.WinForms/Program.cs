using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Interfaces;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Einstiegspunkt der Windows-Forms-Anwendung.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Startet die Anwendung und baut die benötigten Dienste auf.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        IWikiLinkParser wikiLinkParser = new WikiLinkParser();
        IOutlineParser outlineParser = new MarkdownOutlineParser();
        ILinkResolver linkResolver = new LinkResolver();

        var vaultRepository = new FileSystemVaultRepository(wikiLinkParser);
        var settingsRepository = new JsonSettingsRepository();
        var searchService = new SearchService();
        var backlinkService = new BacklinkService(linkResolver);
        var editorFormattingService = new EditorFormattingService();

        var workspaceService = new NotesWorkspaceService(
            vaultRepository,
            settingsRepository,
            wikiLinkParser,
            outlineParser,
            linkResolver,
            searchService,
            backlinkService);

        Application.Run(new MainForm(workspaceService, editorFormattingService));
    }
}

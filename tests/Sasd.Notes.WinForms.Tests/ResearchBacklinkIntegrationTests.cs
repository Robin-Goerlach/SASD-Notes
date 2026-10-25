using Sasd.Notes.App.WinForms;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Prüft die Übergabe fachlich berechneter Backlinks an die Rechercheansicht.
/// </summary>
internal static class ResearchBacklinkIntegrationTests
{
    /// <summary>
    /// Erstellt einen echten temporären Vault und prüft die sichtbare Rückverweisliste.
    /// </summary>
    public static void Run()
    {
        string vaultPath = Path.Combine(
            Path.GetTempPath(),
            "sasd-research-backlinks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(vaultPath);

        try
        {
            File.WriteAllText(
                Path.Combine(vaultPath, "Quelle.md"),
                "# Quelle\n\nSiehe [[Ziel]].");
            File.WriteAllText(
                Path.Combine(vaultPath, "Ziel.md"),
                "# Ziel");

            var parser = new WikiLinkParser();
            var resolver = new LinkResolver();
            var workspace = new NotesWorkspaceService(
                new FileSystemVaultRepository(parser),
                new InMemorySettingsRepository(),
                parser,
                new MarkdownOutlineParser(),
                resolver,
                new SearchService(),
                new BacklinkService(resolver));

            workspace.InitializeAsync().GetAwaiter().GetResult();
            workspace.OpenVaultAsync(vaultPath).GetAwaiter().GetResult();
            NoteDocument target = workspace.LoadedNotes.Single(note => note.Title == "Ziel");

            using var form = new ResearchForm(
                workspace,
                markdownRenderer: null,
                researchMarkdownService: null,
                _ => Task.FromResult(true));
            form.RefreshWorkspace(target);

            ListBox backlinks = form.Controls
                .Find("listBoxResearchBacklinks", searchAllChildren: true)
                .OfType<ListBox>()
                .Single();
            Require(backlinks.Items.Count == 1,
                "Die Rechercheansicht muss den fachlich berechneten Rückverweis anzeigen.");
            Require(backlinks.Items[0] is BacklinkInfo backlink
                    && backlink.SourceRelativePath == "Quelle.md",
                "Der sichtbare Rückverweis muss auf die richtige Quellnotiz zeigen.");
            Require(backlinks.Enabled,
                "Eine nicht leere Backlinkliste muss per Maus und Tastatur bedienbar sein.");
        }
        finally
        {
            Directory.Delete(vaultPath, recursive: true);
        }
    }

    /// <summary>
    /// Hält Einstellungen vollständig im Arbeitsspeicher, damit der Test keine Benutzerdateien berührt.
    /// </summary>
    private sealed class InMemorySettingsRepository : ISettingsRepository
    {
        /// <inheritdoc />
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppSettings());
        }

        /// <inheritdoc />
        public Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

using Sasd.Notes.App.WinForms;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Prüft die Übergabe fachlich berechneter ein- und ausgehender Links an die Rechercheansicht.
/// </summary>
internal static class ResearchBacklinkIntegrationTests
{
    /// <summary>
    /// Erstellt einen echten temporären Vault und prüft Rückverweise sowie offene und aufgelöste Ziele.
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
                "# Quelle\n\nSiehe [[Ziel|Zielalias]] und [[Fehlt]].");
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
            NoteDocument source = workspace.LoadedNotes.Single(note => note.Title == "Quelle");

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

            form.RefreshWorkspace(source);
            ListBox outgoingLinks = form.Controls
                .Find("listBoxResearchOutgoingLinks", searchAllChildren: true)
                .OfType<ListBox>()
                .Single();
            Require(outgoingLinks.Items.Count == 2,
                "Aufgelöste und offene ausgehende Links müssen gemeinsam sichtbar sein.");
            OutgoingLinkInfo resolvedLink = outgoingLinks.Items
                .OfType<OutgoingLinkInfo>()
                .Single(link => link.IsResolved);
            OutgoingLinkInfo unresolvedLink = outgoingLinks.Items
                .OfType<OutgoingLinkInfo>()
                .Single(link => !link.IsResolved);
            Require(resolvedLink.ResolvedRelativePath == "Ziel.md",
                "Der eindeutige Link muss sein navigierbares Ziel behalten.");
            Require(unresolvedLink.Target == "Fehlt",
                "Ein fehlendes Ziel muss ausdrücklich in der Linkliste verbleiben.");
            Require(ListItemFormatting.FormatOutgoingLink(resolvedLink)
                    .StartsWith("[Ziel]", StringComparison.Ordinal),
                "Ein aufgelöstes Ziel muss in der Oberfläche eindeutig gekennzeichnet sein.");
            Require(ListItemFormatting.FormatOutgoingLink(unresolvedLink)
                    .StartsWith("[Nicht aufgelöst]", StringComparison.Ordinal),
                "Ein offenes Ziel muss in der Oberfläche eindeutig gekennzeichnet sein.");
            Require(outgoingLinks.Enabled,
                "Auch eine gemischte Linkliste muss per Maus und Tastatur bedienbar sein.");
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

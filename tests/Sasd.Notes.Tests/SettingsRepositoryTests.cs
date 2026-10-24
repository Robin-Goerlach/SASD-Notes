using Sasd.Notes.Domain.Models;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft Einstellungen ausschließlich in einem eigenen temporären Verzeichnis.
/// </summary>
internal static class SettingsRepositoryTests
{
    /// <summary>
    /// Prüft beschädigte Daten, strukturelle null-Werte, Unicode und abgebrochene Schreibaufträge.
    /// </summary>
    public static async Task RunAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), "sasd-settings-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(root, "Unterordner", "settings.json");
        var repository = new JsonSettingsRepository(path);
        try
        {
            AssertEx.Equal(0, (await repository.LoadAsync()).RecentFolders.Count, "Fehlende Einstellungen liefern Standardwerte.");
            AssertEx.False(Directory.Exists(root), "Konstruktion und Lesen dürfen keine Verzeichnisse anlegen.");

            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await ExpectCancellationAsync(() => repository.SaveAsync(new AppSettings(), cancelled.Token));
            AssertEx.False(Directory.Exists(root), "Ein bereits abgebrochener Schreibauftrag darf keine Verzeichnisse anlegen.");
            await ExpectCancellationAsync(() => repository.LoadAsync(cancelled.Token));

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            foreach (string json in new[] { "{ungültig", "null", "{\"recentFolders\":null}" })
            {
                await File.WriteAllTextAsync(path, json);
                AssertEx.Equal(0, (await repository.LoadAsync()).RecentFolders.Count, "Beschädigte oder leere Daten benötigen sichere Standards.");
                AssertEx.Equal(json, await File.ReadAllTextAsync(path), "Lesen darf beschädigte Einstellungen nicht überschreiben.");
            }

            const string partial = """
                {"recentFolders":[null,{"path":null},{"path":" "},{"path":"Nicht mehr vorhanden Ω"}],
                 "lastOpenedVaultPath":"Gemerkter Vault 😀"}
                """;
            await File.WriteAllTextAsync(path, partial);
            AppSettings loaded = await repository.LoadAsync();
            AssertEx.Equal(1, loaded.RecentFolders.Count, "Nur strukturell unbrauchbare Einträge dürfen entfallen.");
            AssertEx.Equal("Nicht mehr vorhanden Ω", loaded.RecentFolders[0].Path, "Historische Vaults dürfen ohne Existenzprüfung erhalten bleiben.");
            AssertEx.Equal("Gemerkter Vault 😀", loaded.LastOpenedVaultPath, "Andere gültige Einstellungen müssen erhalten bleiben.");

            await repository.SaveAsync(loaded);
            string saved = await File.ReadAllTextAsync(path);
            AppSettings roundtrip = await repository.LoadAsync();
            AssertEx.Equal(loaded.LastOpenedVaultPath, roundtrip.LastOpenedVaultPath, "Unicode muss den JSON-Roundtrip überstehen.");
            await ExpectCancellationAsync(() => repository.SaveAsync(new AppSettings(), cancelled.Token));
            AssertEx.Equal(saved, await File.ReadAllTextAsync(path), "Ein abgebrochener Auftrag muss die vorhandene Datei erhalten.");
            AssertEx.Equal(0, Directory.GetFiles(Path.GetDirectoryName(path)!, "*.tmp-*").Length, "Temporäre Dateien dürfen nicht zurückbleiben.");
        }
        finally
        {
            // Es werden ausschließlich die eigens für diesen Test erzeugten Dateien entfernt.
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static async Task ExpectCancellationAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        throw new InvalidOperationException("Der abgebrochene Auftrag muss als Abbruch gemeldet werden.");
    }
}

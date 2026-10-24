using Sasd.Notes.Application.Models;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Infrastructure.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft Startargumente und die Dateisystemauflösung ohne WinForms oder Benutzereinstellungen.
/// </summary>
internal static class StartupTargetTests
{
    /// <summary>
    /// Prüft Argumentanzahl, relative Pfade, Unicode und vorhandene bzw. ungültige Startziele.
    /// </summary>
    public static async Task RunAsync()
    {
        string temporaryRoot = Path.Combine(Path.GetTempPath(), "sasd-startup-" + Guid.NewGuid().ToString("N"));
        string vaultPath = Path.Combine(temporaryRoot, "Notizen mit Leerzeichen Ω");
        string notePath = Path.Combine(vaultPath, "Übersicht 😀.MD");
        string markdownNamedDirectory = Path.Combine(temporaryRoot, "Ordner.md");
        Directory.CreateDirectory(vaultPath);
        Directory.CreateDirectory(markdownNamedDirectory);
        string originalWorkingDirectory = Environment.CurrentDirectory;
        try
        {
            const string noteContent = "# Unverändert 😀\r\n[[Andere Notiz]]";
            await File.WriteAllTextAsync(notePath, noteContent);
            string textFile = Path.Combine(vaultPath, "Kein Markdown.txt");
            await File.WriteAllTextAsync(textFile, "Unveränderte Textdatei");
            var resolver = new FileSystemStartupTargetResolver();

            AssertEx.True(StartupArgumentsParser.Parse(Array.Empty<string>(), temporaryRoot) is null,
                "Ohne Startargument muss das bisherige Startverhalten gewählt werden.");
            ExpectFailure<ArgumentException>(() => StartupArgumentsParser.Parse(new[] { vaultPath, notePath }, temporaryRoot));
            ExpectFailure<ArgumentException>(() => StartupArgumentsParser.Parse(new[] { string.Empty }, temporaryRoot));
            ExpectFailure<ArgumentException>(() => StartupArgumentsParser.Parse(new[] { " \t " }, temporaryRoot));
            ExpectFailure<ArgumentException>(() => StartupArgumentsParser.Parse(new[] { "." }, "relativer-Basispfad"));
            ExpectFailure<ArgumentException>(() => StartupArgumentsParser.Parse(new[] { "\0" }, temporaryRoot));

            string parsedFolder = StartupArgumentsParser.Parse(new[] { vaultPath }, temporaryRoot)!;
            StartupTarget folderTarget = resolver.Resolve(parsedFolder);
            AssertEx.Equal(vaultPath, folderTarget.VaultPath, "Ein vorhandener Ordner muss direkt als Vault geöffnet werden.");
            AssertEx.True(folderTarget.NotePath is null, "Ein Ordnerstart darf keine beliebige Notiz auswählen.");
            AssertEx.Equal(vaultPath, resolver.Resolve(vaultPath + Path.DirectorySeparatorChar).VaultPath,
                "Ein abschließender Verzeichnistrenner darf keinen abweichenden Startup-Vault erzeugen.");

            string relativeNote = Path.GetRelativePath(temporaryRoot, notePath);
            string parsedNote = StartupArgumentsParser.Parse(new[] { relativeNote }, temporaryRoot)!;
            StartupTarget fileTarget = resolver.Resolve(parsedNote);
            AssertEx.Equal(notePath, fileTarget.NotePath, "Die Markdown-Datei mit Unicode und Leerzeichen muss vollständig erhalten bleiben.");
            AssertEx.Equal(vaultPath, fileTarget.VaultPath, "Beim Dateistart ist der direkte Elternordner der Vault.");
            AssertEx.Equal(notePath, StartupArgumentsParser.Parse(new[] { notePath }, temporaryRoot),
                "Ein vollständiger Pfad darf durch das Arbeitsverzeichnis nicht verändert werden.");
            AssertEx.Equal(vaultPath, Path.TrimEndingDirectorySeparator(StartupArgumentsParser.Parse(new[] { Path.Combine(Path.GetFileName(vaultPath), ".") }, temporaryRoot)!),
                "Relative Punktsegmente müssen ohne Änderung des Prozess-Arbeitsverzeichnisses normalisiert werden.");

            StartupTarget directoryWithExtension = resolver.Resolve(markdownNamedDirectory);
            AssertEx.Equal(markdownNamedDirectory, directoryWithExtension.VaultPath,
                "Ein echtes Verzeichnis mit .md-Endung muss als Ordner behandelt werden.");
            AssertEx.True(directoryWithExtension.NotePath is null, "Die Endung allein darf einen Ordner nicht als Datei klassifizieren.");

            ExpectFailure<FileNotFoundException>(() => resolver.Resolve(Path.Combine(vaultPath, "Fehlt.md")));
            ExpectFailure<FileNotFoundException>(() => resolver.Resolve(Path.Combine(temporaryRoot, "Fehlender Ordner")));
            ExpectFailure<ArgumentException>(() => resolver.Resolve(textFile));
            ExpectFailure<ArgumentException>(() => resolver.Resolve("Relativ.md"));

            AssertEx.Equal(originalWorkingDirectory, Environment.CurrentDirectory, "Die Startauswertung darf das Prozess-Arbeitsverzeichnis nicht umstellen.");
            AssertEx.Equal(noteContent, await File.ReadAllTextAsync(notePath), "Die Startauswertung darf die Markdown-Datei nicht verändern.");
            AssertEx.Equal("Unveränderte Textdatei", await File.ReadAllTextAsync(textFile),
                "Auch abgelehnte Dateien dürfen nicht verändert werden.");
            AssertEx.False(File.Exists(Path.Combine(vaultPath, "Fehlt.md")), "Ein ungültiges Startziel darf nicht automatisch angelegt werden.");
        }
        finally
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    private static void ExpectFailure<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        throw new InvalidOperationException($"Erwartete Ausnahme fehlt: {typeof(TException).Name}");
    }
}

using System.Diagnostics;
using Sasd.Notes.App.WinForms;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;
using Sasd.Notes.Domain.Services;
using Sasd.Notes.Infrastructure.Repositories;

namespace Sasd.Notes.WinForms.Tests;

/// <summary>
/// Paketfreie Windows-Regressionstests für das asynchrone Schließen des Hauptfensters.
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            using var form = new CloseTestForm();
            form.Show();

            // Eine noch nicht abgeschlossene Speicherung darf das echte Fenster nicht schließen.
            form.Close();
            Require(!form.IsDisposed && form.Visible, "Das Fenster wurde vor Abschluss der Bestätigung geschlossen.");
            Require(!form.Enabled, "Während der Bestätigung muss die Bearbeitung gesperrt sein.");
            form.Close();
            Require(form.ConfirmationCount == 1, "Ein weiterer Schließversuch darf keine zweite Bestätigung starten.");

            // Abbrechen oder fehlgeschlagenes Speichern liefert false und hält das Fenster offen.
            form.PendingConfirmation.SetResult(false);
            PumpUntil(() => form.Enabled);
            Require(!form.IsDisposed && form.Visible, "Nach abgelehnter Bestätigung muss das Fenster offen bleiben.");

            form.PendingConfirmation = new TaskCompletionSource<bool>();
            form.Close();
            Require(form.ConfirmationCount == 2, "Ein erneuter Versuch muss nach einem Abbruch möglich sein.");
            Require(!form.IsDisposed, "Auch beim zweiten Versuch muss das Fenster auf den Abschluss warten.");

            form.PendingConfirmation.SetResult(true);
            PumpUntil(() => form.IsDisposed);
            Require(form.ConfirmationCount == 2, "Der genehmigte zweite Schließvorgang darf nicht erneut nachfragen.");

            Console.WriteLine("Alle WinForms-Schließprüfungen wurden erfolgreich ausgeführt.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    /// <summary>
    /// Verarbeitet Windows-Nachrichten bis zur erwarteten Zustandsänderung, mit begrenzter Laufzeit.
    /// </summary>
    private static void PumpUntil(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(10))
            {
                throw new TimeoutException("Die erwartete Fensterzustandsänderung ist ausgeblieben.");
            }

            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Ersetzt nur die Benutzerentscheidung; der reale FormClosing-Ablauf bleibt unverändert.
    /// </summary>
    private sealed class CloseTestForm : MainForm
    {
        public CloseTestForm()
            : base(CreateWorkspace(), new EditorFormattingService())
        {
            ShowInTaskbar = false;
        }

        public TaskCompletionSource<bool> PendingConfirmation { get; set; } = new();

        public int ConfirmationCount { get; private set; }

        protected override Task<bool> ConfirmSaveChangesIfRequiredAsync()
        {
            ConfirmationCount++;
            return PendingConfirmation.Task;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Kein automatischer Vault-Start: Der Test darf keine lokalen Benutzereinstellungen lesen.
        }

        private static NotesWorkspaceService CreateWorkspace()
        {
            var parser = new WikiLinkParser();
            var resolver = new LinkResolver();
            return new NotesWorkspaceService(
                new FileSystemVaultRepository(parser),
                new InMemorySettingsRepository(),
                parser,
                new MarkdownOutlineParser(),
                resolver,
                new SearchService(),
                new BacklinkService(resolver));
        }
    }

    private sealed class InMemorySettingsRepository : ISettingsRepository
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppSettings());
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

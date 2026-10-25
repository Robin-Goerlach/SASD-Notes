using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Eigenständiges Fenster zum Suchen, Lesen und Navigieren im aktuell geöffneten Vault.
/// </summary>
public sealed class ResearchForm : Form
{
    private readonly NotesWorkspaceService _workspaceService;
    private readonly Func<NoteDocument, Task<bool>> _openInEditorAsync;
    private readonly ResearchNavigationService _navigationService = new();
    private readonly ToolStripButton _backButton = new("Zurück");
    private readonly ToolStripButton _forwardButton = new("Vor");
    private readonly ToolStripButton _openInEditorButton = new("Im Editor öffnen");
    private readonly ToolStripTextBox _searchTextBox = new() { AutoSize = false, Width = 280 };
    private readonly ListBox _searchResults = new();
    private readonly Label _documentHeader = new();
    private readonly RichTextBox _documentView = new();
    private readonly ToolStripStatusLabel _statusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };

    /// <summary>
    /// Initialisiert eine Rechercheansicht für den gemeinsam genutzten Workspace.
    /// </summary>
    /// <param name="workspaceService">Workspace mit dem aktuell geladenen Vault und dessen Arbeitsständen.</param>
    /// <param name="openInEditorAsync">Vom Hauptfenster kontrollierter Wechsel einer Notiz in den Editor.</param>
    public ResearchForm(
        NotesWorkspaceService workspaceService,
        Func<NoteDocument, Task<bool>> openInEditorAsync)
    {
        _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
        _openInEditorAsync = openInEditorAsync ?? throw new ArgumentNullException(nameof(openInEditorAsync));

        InitializeLayout();
        UpdateNavigationButtons();
    }

    /// <summary>
    /// Setzt die Ansicht nach einem Vault-Wechsel zurück und zeigt optional eine Startnotiz.
    /// </summary>
    /// <param name="preferredNote">Notiz, die im neuen Rechercheverlauf zuerst angezeigt werden soll.</param>
    public void RefreshWorkspace(NoteDocument? preferredNote)
    {
        _navigationService.Reset();
        _searchResults.Items.Clear();
        _searchTextBox.Clear();

        NoteDocument? liveNote = preferredNote is null
            ? null
            : FindLoadedNote(preferredNote.RelativePath);

        if (liveNote is null)
        {
            ClearDocument(_workspaceService.CurrentVault is null
                ? "Öffnen Sie zunächst einen Vault."
                : "Geben Sie einen Suchbegriff ein oder öffnen Sie eine Notiz aus dem Editor.");
        }
        else
        {
            NavigateTo(liveNote);
        }

        UpdateNavigationButtons();
    }

    /// <inheritdoc />
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        // Beim Zurückkehren ins Fenster wird derselbe Workspace-Gegenstand erneut gelesen.
        // Damit werden auch noch nicht gespeicherte Editoränderungen sichtbar.
        if (_navigationService.CurrentRelativePath is not { } relativePath)
        {
            return;
        }

        NoteDocument? currentNote = FindLoadedNote(relativePath);
        if (currentNote is null)
        {
            _navigationService.Reset();
            ClearDocument("Die zuvor angezeigte Notiz gehört nicht mehr zum aktuellen Vault.");
            UpdateNavigationButtons();
            return;
        }

        DisplayDocument(currentNote);
    }

    /// <summary>
    /// Erstellt das eigenständige, bewusst vom Editor getrennte Recherchelayout.
    /// </summary>
    private void InitializeLayout()
    {
        Text = "SASD Notes – Recherche";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 600);
        Size = new Size(1200, 760);

        var toolStrip = new ToolStrip();
        var searchButton = new ToolStripButton("Suchen");
        toolStrip.Items.AddRange(new ToolStripItem[]
        {
            _backButton,
            _forwardButton,
            new ToolStripSeparator(),
            new ToolStripLabel("Suche"),
            _searchTextBox,
            searchButton,
            new ToolStripSeparator(),
            _openInEditorButton
        });

        _backButton.Click += (_, _) => NavigateHistory(_navigationService.GoBack());
        _forwardButton.Click += (_, _) => NavigateHistory(_navigationService.GoForward());
        searchButton.Click += (_, _) => ExecuteSearch();
        _openInEditorButton.Click += openInEditorButton_Click;
        _searchTextBox.KeyDown += searchTextBox_KeyDown;

        var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        var resultLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Suchtreffer – Doppelklick öffnet die Notiz",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _searchResults.Dock = DockStyle.Fill;
        _searchResults.IntegralHeight = false;
        _searchResults.DoubleClick += (_, _) => NavigateToSelectedResult();
        _searchResults.KeyDown += searchResults_KeyDown;
        leftPanel.Controls.Add(_searchResults);
        leftPanel.Controls.Add(resultLabel);

        var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        _documentHeader.Dock = DockStyle.Top;
        _documentHeader.Height = 34;
        _documentHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _documentHeader.TextAlign = ContentAlignment.MiddleLeft;
        _documentHeader.AutoEllipsis = true;

        // Der erste Schnitt ist absichtlich schreibgeschützt. Ein HTML-Renderer wird später
        // hinter einer eigenen Schnittstelle ergänzt und verändert den Markdown-Text nicht.
        _documentView.Dock = DockStyle.Fill;
        _documentView.ReadOnly = true;
        _documentView.BorderStyle = BorderStyle.None;
        _documentView.Font = new Font("Consolas", 11F);
        _documentView.WordWrap = true;
        _documentView.BackColor = SystemColors.Window;
        rightPanel.Controls.Add(_documentView);
        rightPanel.Controls.Add(_documentHeader);

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 390,
            FixedPanel = FixedPanel.Panel1
        };
        splitContainer.Panel1.Controls.Add(leftPanel);
        splitContainer.Panel2.Controls.Add(rightPanel);

        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_statusLabel);

        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
    }

    /// <summary>
    /// Sucht ausschließlich im bereits geladenen Arbeitsstand des aktuellen Vaults.
    /// </summary>
    private void ExecuteSearch()
    {
        IReadOnlyList<SearchHit> hits = _workspaceService.Search(_searchTextBox.Text);

        _searchResults.BeginUpdate();
        _searchResults.Items.Clear();
        foreach (SearchHit hit in hits)
        {
            _searchResults.Items.Add(hit);
        }
        _searchResults.EndUpdate();

        _statusLabel.Text = hits.Count == 0
            ? "Keine Treffer."
            : $"{hits.Count} Treffer. Doppelklick oder Eingabetaste öffnet eine Notiz.";
    }

    /// <summary>
    /// Öffnet den ausgewählten Suchtreffer in der schreibgeschützten Rechercheansicht.
    /// </summary>
    private void NavigateToSelectedResult()
    {
        if (_searchResults.SelectedItem is not SearchHit hit)
        {
            return;
        }

        NoteDocument? note = FindLoadedNote(hit.RelativePath);
        if (note is null)
        {
            _statusLabel.Text = "Die ausgewählte Notiz gehört nicht mehr zum aktuellen Vault.";
            return;
        }

        NavigateTo(note);
    }

    /// <summary>
    /// Nimmt eine Notiz in den Rechercheverlauf auf und zeigt ihren aktuellen Arbeitsstand.
    /// </summary>
    private void NavigateTo(NoteDocument note)
    {
        _navigationService.NavigateTo(note.RelativePath);
        DisplayDocument(note);
        UpdateNavigationButtons();
    }

    /// <summary>
    /// Zeigt einen bereits vom Navigationsdienst gewählten Verlaufseintrag an.
    /// </summary>
    private void NavigateHistory(string? relativePath)
    {
        if (relativePath is null)
        {
            return;
        }

        NoteDocument? note = FindLoadedNote(relativePath);
        if (note is null)
        {
            _navigationService.Reset();
            ClearDocument("Die Verlaufsnotiz gehört nicht mehr zum aktuellen Vault.");
        }
        else
        {
            DisplayDocument(note);
        }

        UpdateNavigationButtons();
    }

    /// <summary>
    /// Übernimmt den aktuellen In-Memory-Text einer Notiz in die Leseansicht.
    /// </summary>
    private void DisplayDocument(NoteDocument note)
    {
        _documentHeader.Text = note.RelativePath;
        _documentView.Text = note.Content;
        _openInEditorButton.Enabled = true;
        _statusLabel.Text = note.IsDirty
            ? "Arbeitsstand mit ungespeicherten Änderungen."
            : "Schreibgeschützte Markdown-Ansicht.";
    }

    /// <summary>
    /// Leert die Dokumentansicht und zeigt einen erklärenden Zustandstext.
    /// </summary>
    private void ClearDocument(string message)
    {
        _documentHeader.Text = "Keine Notiz ausgewählt";
        _documentView.Clear();
        _openInEditorButton.Enabled = false;
        _statusLabel.Text = message;
    }

    /// <summary>
    /// Sucht ohne Dateisystemzugriff nach einer aktuell geladenen Notiz.
    /// </summary>
    private NoteDocument? FindLoadedNote(string relativePath)
    {
        return _workspaceService.LoadedNotes.FirstOrDefault(note =>
            string.Equals(note.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Aktualisiert die Schaltflächen aus dem UI-unabhängigen Verlauf.
    /// </summary>
    private void UpdateNavigationButtons()
    {
        _backButton.Enabled = _navigationService.CanGoBack;
        _forwardButton.Enabled = _navigationService.CanGoForward;
        _openInEditorButton.Enabled = _navigationService.CurrentRelativePath is not null;
    }

    /// <summary>
    /// Übergibt die aktuell gelesene Notiz kontrolliert an das Hauptfenster.
    /// </summary>
    private async void openInEditorButton_Click(object? sender, EventArgs e)
    {
        if (_navigationService.CurrentRelativePath is not { } relativePath)
        {
            return;
        }

        NoteDocument? note = FindLoadedNote(relativePath);
        if (note is null)
        {
            _statusLabel.Text = "Die Notiz gehört nicht mehr zum aktuellen Vault.";
            return;
        }

        bool opened = await _openInEditorAsync(note);
        _statusLabel.Text = opened
            ? $"Im Editor geöffnet: {note.RelativePath}"
            : "Der Editorwechsel wurde abgebrochen.";
    }

    /// <summary>
    /// Startet die Suche mit der Eingabetaste.
    /// </summary>
    private void searchTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        ExecuteSearch();
        e.SuppressKeyPress = true;
    }

    /// <summary>
    /// Öffnet einen Treffer auch tastaturbedient.
    /// </summary>
    private void searchResults_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        NavigateToSelectedResult();
        e.SuppressKeyPress = true;
    }
}

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Eigenständiges Fenster zum Suchen, Lesen und Navigieren im aktuell geöffneten Vault.
/// </summary>
public sealed class ResearchForm : Form
{
    // NavigateToString akzeptiert höchstens zwei Millionen Zeichen. Ein kleiner
    // Sicherheitsabstand verhindert Grenzfehler durch zukünftige Rahmenänderungen.
    private const int MaximumBrowserDocumentLength = 1_900_000;

    private readonly NotesWorkspaceService _workspaceService;
    private readonly IMarkdownHtmlRenderer? _markdownRenderer;
    private readonly ResearchMarkdownService? _researchMarkdownService;
    private readonly Func<NoteDocument, Task<bool>> _openInEditorAsync;
    private readonly ResearchNavigationService _navigationService = new();
    private readonly ToolStripButton _backButton = new("Zurück");
    private readonly ToolStripButton _forwardButton = new("Vor");
    private readonly ToolStripButton _openInEditorButton = new("Im Editor öffnen");
    private readonly ToolStripTextBox _searchTextBox = new() { AutoSize = false, Width = 280 };
    private readonly ListBox _searchResults = new();
    private readonly ListBox _outlineItems = new();
    private readonly Label _documentHeader = new();
    private readonly WebView2 _browserView = new();
    private readonly RichTextBox _fallbackView = new();
    private readonly ToolStripStatusLabel _statusLabel = new()
    {
        Spring = true,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private string? _pendingHtml;
    private SearchHit? _activeSearchHit;
    private (int Start, int Length)? _pendingSearchHighlight;
    private bool _browserReady;
    private bool _browserDocumentReady;
    private bool _browserUnavailable;

    /// <summary>
    /// Initialisiert eine Rechercheansicht für den gemeinsam genutzten Workspace.
    /// </summary>
    /// <param name="workspaceService">Workspace mit dem aktuell geladenen Vault und dessen Arbeitsständen.</param>
    /// <param name="markdownRenderer">Renderer für das sichere, vollständige HTML-Dokument oder <see langword="null"/> für die Quelltextansicht.</param>
    /// <param name="researchMarkdownService">Dienst zum Vorbereiten eindeutig aufgelöster Wiki-Links.</param>
    /// <param name="openInEditorAsync">Vom Hauptfenster kontrollierter Wechsel einer Notiz in den Editor.</param>
    public ResearchForm(
        NotesWorkspaceService workspaceService,
        IMarkdownHtmlRenderer? markdownRenderer,
        ResearchMarkdownService? researchMarkdownService,
        Func<NoteDocument, Task<bool>> openInEditorAsync)
    {
        _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
        _markdownRenderer = markdownRenderer;
        _researchMarkdownService = researchMarkdownService;
        _openInEditorAsync = openInEditorAsync ?? throw new ArgumentNullException(nameof(openInEditorAsync));

        InitializeLayout();
        UpdateNavigationButtons();
        Shown += researchForm_Shown;
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

        var resultPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
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
        resultPanel.Controls.Add(_searchResults);
        resultPanel.Controls.Add(resultLabel);

        var outlinePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        var outlineLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Gliederung – Doppelklick springt zur Überschrift",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _outlineItems.Dock = DockStyle.Fill;
        _outlineItems.IntegralHeight = false;
        _outlineItems.Enabled = false;
        _outlineItems.DoubleClick += async (_, _) => await NavigateToSelectedOutlineAsync();
        _outlineItems.KeyDown += outlineItems_KeyDown;
        outlinePanel.Controls.Add(_outlineItems);
        outlinePanel.Controls.Add(outlineLabel);

        // Suchtreffer und Dokumentgliederung bleiben in einer gemeinsamen Navigationsspalte.
        // Das bestehende Editorfenster wird dadurch nicht verändert.
        var navigationSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 360,
            FixedPanel = FixedPanel.Panel2,
            Panel1MinSize = 180,
            Panel2MinSize = 130
        };
        navigationSplitContainer.Panel1.Controls.Add(resultPanel);
        navigationSplitContainer.Panel2.Controls.Add(outlinePanel);

        var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        _documentHeader.Dock = DockStyle.Top;
        _documentHeader.Height = 34;
        _documentHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _documentHeader.TextAlign = ContentAlignment.MiddleLeft;
        _documentHeader.AutoEllipsis = true;

        _browserView.Dock = DockStyle.Fill;
        _browserView.Visible = false;

        // Die Fallback-Ansicht bleibt hinter WebView2 erhalten. Sie zeigt weiterhin
        // lesbaren Inhalt, wenn Runtime, Renderer oder Dokumentgröße die HTML-Ansicht verhindern.
        _fallbackView.Dock = DockStyle.Fill;
        _fallbackView.ReadOnly = true;
        _fallbackView.BorderStyle = BorderStyle.None;
        _fallbackView.Font = new Font("Consolas", 11F);
        _fallbackView.WordWrap = true;
        _fallbackView.BackColor = SystemColors.Window;

        var documentPanel = new Panel { Dock = DockStyle.Fill };
        documentPanel.Controls.Add(_browserView);
        documentPanel.Controls.Add(_fallbackView);
        rightPanel.Controls.Add(documentPanel);
        rightPanel.Controls.Add(_documentHeader);

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 390,
            FixedPanel = FixedPanel.Panel1
        };
        splitContainer.Panel1.Controls.Add(navigationSplitContainer);
        splitContainer.Panel2.Controls.Add(rightPanel);

        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_statusLabel);

        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
    }

    /// <summary>
    /// Initialisiert WebView2 erst nach dem Anzeigen des Fensters und richtet sichere Standardwerte ein.
    /// </summary>
    private async void researchForm_Shown(object? sender, EventArgs e)
    {
        if (_markdownRenderer is null)
        {
            _browserUnavailable = true;
            ShowFallback();
            return;
        }

        try
        {
            await _browserView.EnsureCoreWebView2Async();

            // Die generierte Vorschau benötigt weder JavaScript noch .NET-Hostobjekte.
            // Das Abschalten reduziert die Angriffsfläche zusätzlich zur CSP des Dokuments.
            _browserView.CoreWebView2.Settings.IsScriptEnabled = false;
            _browserView.CoreWebView2.Settings.IsWebMessageEnabled = false;
            _browserView.CoreWebView2.Settings.AreHostObjectsAllowed = false;
            _browserView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _browserView.CoreWebView2.NavigationStarting += browserView_NavigationStarting;
            _browserView.CoreWebView2.NavigationCompleted += browserView_NavigationCompleted;
            _browserReady = true;

            if (_pendingHtml is not null)
            {
                ShowBrowserDocument(_pendingHtml);
            }
        }
        catch (WebView2RuntimeNotFoundException)
        {
            UseBrowserFallback("Die WebView2-Runtime ist nicht installiert. Markdown wird als Quelltext angezeigt.");
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or COMException or IOException)
        {
            UseBrowserFallback("Die formatierte Ansicht konnte nicht initialisiert werden. Markdown wird als Quelltext angezeigt.");
        }
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

        NavigateTo(note, hit);
    }

    /// <summary>
    /// Nimmt eine Notiz in den Rechercheverlauf auf und zeigt ihren aktuellen Arbeitsstand.
    /// </summary>
    /// <param name="note">Anzuzeigende geladene Notiz.</param>
    /// <param name="searchHit">Optionaler Suchtreffer, der im Dokument hervorgehoben werden soll.</param>
    private void NavigateTo(NoteDocument note, SearchHit? searchHit = null)
    {
        _activeSearchHit = searchHit;
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

        _activeSearchHit = null;
        _pendingSearchHighlight = null;

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
    /// Rendert den aktuellen In-Memory-Text, ohne ihn oder die Notizdatei zu verändern.
    /// </summary>
    private void DisplayDocument(NoteDocument note)
    {
        _documentHeader.Text = note.RelativePath;
        _fallbackView.Text = note.Content;
        PopulateOutline(note);
        _openInEditorButton.Enabled = true;
        _pendingSearchHighlight = LocateActiveSearchHit(note);

        if (_markdownRenderer is null || _browserUnavailable)
        {
            ShowFallback();
            if (_activeSearchHit is not null && _pendingSearchHighlight is null)
            {
                _statusLabel.Text = "Die Notiz wurde über Titel oder Pfad gefunden; im Inhalt gibt es keine markierbare Fundstelle.";
            }
            else if (_pendingSearchHighlight is not null)
            {
                _statusLabel.Text = "Suchtreffer in der schreibgeschützten Markdown-Quelltextansicht markiert.";
            }
            else
            {
                _statusLabel.Text = note.IsDirty
                    ? "Quelltextansicht mit ungespeicherten Änderungen."
                    : "Schreibgeschützte Markdown-Quelltextansicht.";
            }

            return;
        }

        try
        {
            // Die Application-Schicht wandelt nur eindeutig aufgelöste Wiki-Links um.
            // Der HTML-Renderer bleibt dadurch frei von Vault- und Navigationswissen.
            string renderableMarkdown = _researchMarkdownService?.PrepareForRendering(
                note.Content,
                _workspaceService.LoadedNotes) ?? note.Content;
            string html = _markdownRenderer.Render(renderableMarkdown);
            if (html.Length > MaximumBrowserDocumentLength)
            {
                _pendingHtml = null;
                ShowFallback();
                _statusLabel.Text = "Das Dokument ist für die eingebettete HTML-Ansicht zu groß und wird als Quelltext angezeigt.";
                return;
            }

            _pendingHtml = html;
            if (_browserReady)
            {
                ShowBrowserDocument(html);
            }
            else
            {
                ShowFallback();
            }

            if (_activeSearchHit is not null && _pendingSearchHighlight is null)
            {
                _statusLabel.Text = "Die Notiz wurde über Titel oder Pfad gefunden; im Inhalt gibt es keine markierbare Fundstelle.";
            }
            else
            {
                string baseStatus = note.IsDirty
                    ? "Formatierte Ansicht des ungespeicherten Arbeitsstands."
                    : "Formatierte, schreibgeschützte Markdown-Ansicht.";
                _statusLabel.Text = _pendingSearchHighlight is null
                    ? baseStatus
                    : baseStatus + " Der Suchtreffer wird hervorgehoben.";
            }
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            _pendingHtml = null;
            ShowFallback();
            _statusLabel.Text = "Markdown konnte nicht formatiert werden und wird als Quelltext angezeigt.";
        }
    }

    /// <summary>
    /// Zeigt ein bereits erzeugtes HTML-Dokument im initialisierten Browser-Control.
    /// </summary>
    private void ShowBrowserDocument(string html)
    {
        // Während NavigateToString läuft, darf eine Auswahl nicht versehentlich
        // noch die zuvor dargestellte HTML-Seite ansteuern.
        _browserDocumentReady = false;
        _outlineItems.Enabled = false;
        _browserView.NavigateToString(html);
        _browserView.Visible = true;
        _fallbackView.Visible = false;
    }

    /// <summary>
    /// Aktiviert die stets verfügbare schreibgeschützte Quelltextdarstellung.
    /// </summary>
    private void ShowFallback()
    {
        _browserDocumentReady = false;
        _browserView.Visible = false;
        _fallbackView.Visible = true;
        _outlineItems.Enabled = _outlineItems.Items.Count > 0;

        // Auch ohne WebView2 bleibt ein Inhaltstreffer direkt auffindbar.
        if (_pendingSearchHighlight is { } highlight
            && highlight.Start >= 0
            && highlight.Start <= _fallbackView.TextLength - highlight.Length)
        {
            _fallbackView.Select(highlight.Start, highlight.Length);
            _fallbackView.ScrollToCaret();
        }
        else
        {
            _fallbackView.Select(0, 0);
        }
    }

    /// <summary>
    /// Schaltet nach einem Initialisierungsfehler dauerhaft auf die sichere Fallback-Ansicht um.
    /// </summary>
    private void UseBrowserFallback(string message)
    {
        _browserUnavailable = true;
        _browserReady = false;
        _pendingHtml = null;
        ShowFallback();
        _statusLabel.Text = message;
    }

    /// <summary>
    /// Markiert nach abgeschlossener interner Dokumentnavigation den ausgewählten Inhaltstreffer.
    /// </summary>
    private async void browserView_NavigationCompleted(
        object? sender,
        CoreWebView2NavigationCompletedEventArgs e)
    {
        _browserDocumentReady = e.IsSuccess;
        _outlineItems.Enabled = e.IsSuccess && _outlineItems.Items.Count > 0;
        if (!e.IsSuccess || _pendingSearchHighlight is not { } highlight)
        {
            return;
        }

        string matchedText = _fallbackView.Text.Substring(highlight.Start, highlight.Length);
        try
        {
            // ExecuteScriptAsync führt ausschließlich das feste Hostskript aus. Skripte aus
            // Markdown bleiben durch WebView2-Einstellungen und CSP vollständig deaktiviert.
            string result = await _browserView.CoreWebView2.ExecuteScriptAsync(
                ResearchHighlightScriptBuilder.Build(matchedText));
            _statusLabel.Text = string.Equals(result, "true", StringComparison.Ordinal)
                ? "Suchtreffer im formatierten Dokument hervorgehoben."
                : "Die Fundstelle liegt in Markdown-Steuerzeichen oder über mehreren Darstellungsbereichen und konnte nicht markiert werden.";
        }
        catch (Exception exception) when (exception is InvalidOperationException or COMException)
        {
            _statusLabel.Text = "Die Suchtreffer-Hervorhebung ist derzeit nicht verfügbar.";
        }
    }

    /// <summary>
    /// Lokalisiert den ausgewählten Treffer erneut im aktuellen Arbeitsstand der angezeigten Notiz.
    /// </summary>
    private (int Start, int Length)? LocateActiveSearchHit(NoteDocument note)
    {
        if (_activeSearchHit is null
            || !string.Equals(
                _activeSearchHit.RelativePath,
                note.RelativePath,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Zwischen Suche und Öffnen kann der Editorinhalt geändert worden sein.
        // Der gemeinsame Locator bevorzugt die alte Position und sucht andernfalls
        // das nächste weiterhin vorhandene Vorkommen.
        return SearchHitLocator.Locate(_activeSearchHit, note.Content);
    }

    /// <summary>
    /// Verhindert freie Browsernavigation und übergibt externe Links nur nach Bestätigung an Windows.
    /// </summary>
    private void browserView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (TryNavigateInternalNote(e))
        {
            return;
        }

        if (string.Equals(e.Uri, "about:blank", StringComparison.OrdinalIgnoreCase)
            || e.Uri.StartsWith("about:blank#", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        e.Cancel = true;
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp
                && uri.Scheme != Uri.UriSchemeHttps
                && uri.Scheme != Uri.UriSchemeMailto))
        {
            _statusLabel.Text = "Die angeforderte Navigation wurde aus Sicherheitsgründen blockiert.";
            return;
        }

        DialogResult result = MessageBox.Show(
            this,
            $"Externen Link mit der Windows-Standardanwendung öffnen?{Environment.NewLine}{Environment.NewLine}{uri}",
            "SASD Notes – Externer Link",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
        {
            _statusLabel.Text = "Externer Link wurde nicht geöffnet.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            _statusLabel.Text = "Externer Link wurde an Windows übergeben.";
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            _statusLabel.Text = "Der externe Link konnte nicht geöffnet werden.";
        }
    }

    /// <summary>
    /// Erkennt das interne Recherche-Schema und öffnet ausschließlich eine bereits geladene Notiz.
    /// </summary>
    private bool TryNavigateInternalNote(CoreWebView2NavigationStartingEventArgs e)
    {
        if (!e.Uri.StartsWith(
                ResearchMarkdownService.InternalNoteUriPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Interne Links dürfen niemals vom eingebetteten Browser selbst verarbeitet werden.
        e.Cancel = true;
        string encodedRelativePath = e.Uri[ResearchMarkdownService.InternalNoteUriPrefix.Length..];

        string relativePath;
        try
        {
            relativePath = Uri.UnescapeDataString(encodedRelativePath);
        }
        catch (UriFormatException)
        {
            _statusLabel.Text = "Der interne Wiki-Link ist ungültig.";
            return true;
        }

        NoteDocument? targetNote = FindLoadedNote(relativePath);
        if (targetNote is null)
        {
            _statusLabel.Text = "Das Ziel des Wiki-Links ist nicht mehr im aktuellen Vault vorhanden.";
            return true;
        }

        NavigateTo(targetNote);
        return true;
    }

    /// <summary>
    /// Leert die Dokumentansicht und zeigt einen erklärenden Zustandstext.
    /// </summary>
    private void ClearDocument(string message)
    {
        _documentHeader.Text = "Keine Notiz ausgewählt";
        _fallbackView.Clear();
        _outlineItems.Items.Clear();
        _outlineItems.Enabled = false;
        _pendingHtml = null;
        _activeSearchHit = null;
        _pendingSearchHighlight = null;
        ShowFallback();
        _openInEditorButton.Enabled = false;
        _statusLabel.Text = message;
    }

    /// <summary>
    /// Füllt die Gliederung mit dem gemeinsamen Markdown-Outline-Parser.
    /// </summary>
    private void PopulateOutline(NoteDocument note)
    {
        IReadOnlyList<OutlineItem> outline = _workspaceService.GetOutline(note);

        _outlineItems.BeginUpdate();
        _outlineItems.Items.Clear();
        foreach (OutlineItem item in outline)
        {
            _outlineItems.Items.Add(item);
        }
        _outlineItems.EndUpdate();

        // Im Quelltext-Fallback kann sofort gesprungen werden. Für die HTML-Ansicht
        // wird die Liste erst nach erfolgreichem NavigationCompleted freigegeben.
        _outlineItems.Enabled = outline.Count > 0
            && (!_browserView.Visible || _browserDocumentReady);
    }

    /// <summary>
    /// Springt zur ausgewählten Überschrift in der aktuell sichtbaren Darstellung.
    /// </summary>
    private async Task NavigateToSelectedOutlineAsync()
    {
        if (_outlineItems.SelectedItem is not OutlineItem item)
        {
            return;
        }

        if (_fallbackView.Visible)
        {
            int caretPosition = Math.Clamp(item.StartIndex, 0, _fallbackView.TextLength);
            _fallbackView.Select(caretPosition, 0);
            _fallbackView.ScrollToCaret();
            _statusLabel.Text = $"Zur Überschrift gesprungen: {item.HeadingText}";
            return;
        }

        if (!_browserReady || !_browserDocumentReady || _browserView.CoreWebView2 is null)
        {
            _statusLabel.Text = "Die formatierte Ansicht wird noch geladen.";
            return;
        }

        try
        {
            // Ebene, Titel und Wiederholungsnummer vermeiden eine Verschiebung durch
            // Überschriftsformen, die Markdig zusätzlich zum V1-Outline-Parser kennt.
            int occurrence = CountPreviousEquivalentHeadings(
                _outlineItems.SelectedIndex,
                item);
            string result = await _browserView.CoreWebView2.ExecuteScriptAsync(
                ResearchOutlineScriptBuilder.Build(item.Level, item.HeadingText, occurrence));
            _statusLabel.Text = string.Equals(result, "true", StringComparison.Ordinal)
                ? $"Zur Überschrift gesprungen: {item.HeadingText}"
                : "Die ausgewählte Überschrift konnte in der formatierten Ansicht nicht gefunden werden.";
        }
        catch (Exception exception) when (exception is InvalidOperationException or COMException)
        {
            _statusLabel.Text = "Der Sprung zur Überschrift ist derzeit nicht verfügbar.";
        }
    }

    /// <summary>
    /// Zählt gleichlautende Überschriften derselben Ebene vor dem ausgewählten Eintrag.
    /// </summary>
    private int CountPreviousEquivalentHeadings(int selectedIndex, OutlineItem selectedItem)
    {
        int occurrence = 0;
        for (int index = 0; index < selectedIndex; index++)
        {
            if (_outlineItems.Items[index] is OutlineItem previous
                && previous.Level == selectedItem.Level
                && string.Equals(
                    previous.HeadingText,
                    selectedItem.HeadingText,
                    StringComparison.Ordinal))
            {
                occurrence++;
            }
        }

        return occurrence;
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
    /// Führt einen Gliederungssprung auch tastaturbedient aus.
    /// </summary>
    private async void outlineItems_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        await NavigateToSelectedOutlineAsync();
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

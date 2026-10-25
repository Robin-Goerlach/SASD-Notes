using Sasd.Notes.App.WinForms.Dialogs;
using Sasd.Notes.Application.Models;
using Sasd.Notes.Application.Services;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.App.WinForms;

/// <summary>
/// Hauptfenster der Anwendung mit Navigation, Editor und Kontextbereichen.
/// </summary>
public partial class MainForm : Form
{
    private readonly NotesWorkspaceService _workspaceService;
    private readonly EditorFormattingService _editorFormattingService;
    private readonly StartupTarget? _startupTarget;
    private NoteDocument? _currentNote;
    private bool _suppressEditorEvents;
    private readonly System.Windows.Forms.Timer _externalChangeTimer;
    private readonly ToolStripMenuItem _newFolderMenuItem;
    private readonly ToolStripMenuItem _reloadNoteMenuItem;
    private readonly ToolStripMenuItem _researchMenuItem;
    private ResearchForm? _researchForm;
    private string? _externalChangeWarningPath;
    private ExternalChangeStatus? _externalChangeWarningStatus;
    private bool _closeConfirmationInProgress;
    private bool _closeApproved;
    private bool _vaultOpenInProgress;
    private bool _startupInProgress;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="MainForm"/>-Klasse.
    /// </summary>
    /// <param name="workspaceService">Dienst für den aktiven Vault und seine Notizen.</param>
    /// <param name="editorFormattingService">UI-unabhängige Markdown-Formatierung.</param>
    /// <param name="startupTarget">Explizites Startziel oder <see langword="null"/> für das bisherige Startverhalten.</param>
    public MainForm(
        NotesWorkspaceService workspaceService,
        EditorFormattingService editorFormattingService,
        StartupTarget? startupTarget = null)
    {
        _workspaceService = workspaceService;
        _editorFormattingService = editorFormattingService;
        _startupTarget = startupTarget;
        InitializeComponent();
        AddAdditionalFormattingButtons();

        // Der Menüpunkt wird programmgesteuert ergänzt, weil der Dialog keine Designer-Abhängigkeit benötigt.
        _newFolderMenuItem = new ToolStripMenuItem("Neuer Ordner...");
        _newFolderMenuItem.Click += newFolderToolStripMenuItem_Click;
        int newNoteIndex = fileToolStripMenuItem.DropDownItems.IndexOf(newNoteToolStripMenuItem);
        fileToolStripMenuItem.DropDownItems.Insert(newNoteIndex, _newFolderMenuItem);

        _reloadNoteMenuItem = new ToolStripMenuItem("Aktuelle Notiz neu laden")
        {
            ShortcutKeys = Keys.F6
        };
        _reloadNoteMenuItem.Click += reloadNoteToolStripMenuItem_Click;
        int refreshIndex = fileToolStripMenuItem.DropDownItems.IndexOf(refreshVaultToolStripMenuItem);
        fileToolStripMenuItem.DropDownItems.Insert(refreshIndex + 1, _reloadNoteMenuItem);

        // Die Recherche bleibt ein eigenes Fenster, damit die bewährte Editoroberfläche unverändert bleibt.
        var viewMenuItem = new ToolStripMenuItem("Ansicht");
        _researchMenuItem = new ToolStripMenuItem("Recherche...")
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.R
        };
        _researchMenuItem.Click += researchToolStripMenuItem_Click;
        viewMenuItem.DropDownItems.Add(_researchMenuItem);
        int helpIndex = menuStripMain.Items.IndexOf(helpToolStripMenuItem);
        menuStripMain.Items.Insert(helpIndex, viewMenuItem);

        // Die Prüfung läuft periodisch über den Workspace, damit die UI selbst keinen Dateisystemzugriff benötigt.
        _externalChangeTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _externalChangeTimer.Tick += externalChangeTimer_Tick;
        _externalChangeTimer.Start();
    }

    /// <summary>
    /// Ergänzt die Formatierungsbefehle, die im Designer nicht benötigt werden.
    /// </summary>
    private void AddAdditionalFormattingButtons()
    {
        var strikethroughButton = CreateFormattingButton("Durch", MarkdownFormatKind.Strikethrough);
        var quoteButton = CreateFormattingButton("Zitat", MarkdownFormatKind.Quote);
        var numberedListButton = CreateFormattingButton("1. Liste", MarkdownFormatKind.NumberedList);

        // Die zusätzlichen Befehle werden neben den bestehenden Listen- und Inline-Befehlen eingeordnet.
        int insertIndex = toolStripMain.Items.IndexOf(toolStripButtonCode) + 1;
        toolStripMain.Items.Insert(insertIndex, strikethroughButton);
        toolStripMain.Items.Insert(insertIndex + 1, quoteButton);
        toolStripMain.Items.Insert(insertIndex + 2, numberedListButton);
    }

    /// <summary>
    /// Erzeugt einen einheitlich konfigurierten Markdown-Toolbarbutton.
    /// </summary>
    /// <param name="caption">Kurze Beschriftung des Buttons.</param>
    /// <param name="formatKind">Anzuwendende Markdown-Formatierung.</param>
    /// <returns>Konfigurierter Toolbarbutton.</returns>
    private ToolStripButton CreateFormattingButton(string caption, MarkdownFormatKind formatKind)
    {
        var button = new ToolStripButton(caption)
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Tag = formatKind
        };

        button.Click += toolStripFormatButton_Click;
        return button;
    }

    /// <summary>
    /// Führt Startarbeiten nach dem Laden des Fensters aus.
    /// </summary>
    private async void MainForm_Load(object? sender, EventArgs e)
    {
        // Auch das asynchrone Laden der Einstellungen gehört zum geschützten Programmstart.
        // Dadurch kann kein verspäteter Startabschluss auf ein bereits geschlossenes Fenster zugreifen.
        _startupInProgress = true;
        bool wasEnabled = Enabled;
        bool previousWaitCursor = UseWaitCursor;
        Enabled = false;
        UseWaitCursor = true;
        try
        {
            await _workspaceService.InitializeAsync();
            RefreshRecentFoldersMenu();

            if (_startupTarget is not null)
            {
                // Das explizite Argument hat Vorrang vor dem zuletzt verwendeten Vault.
                // Der normale Öffnungsweg übernimmt Fehlerbehandlung, Recent Folders und den Dateibaum.
                bool opened = await OpenVaultAsync(_startupTarget.VaultPath);
                if (opened && _startupTarget.NotePath is not null)
                {
                    NoteDocument startupNote = _workspaceService.FindNoteByFullPath(_startupTarget.NotePath)
                        ?? throw new FileNotFoundException(
                            "Die angeforderte Markdown-Datei ist im geöffneten Vault nicht mehr verfügbar.", _startupTarget.NotePath);
                    OpenNoteInEditor(startupNote);
                }
                return;
            }

            string? lastOpenedVaultPath = _workspaceService.GetLastOpenedVaultPath();
            if (!string.IsNullOrWhiteSpace(lastOpenedVaultPath) && _workspaceService.VaultExists(lastOpenedVaultPath))
            {
                await OpenVaultAsync(lastOpenedVaultPath);
            }
            else
            {
                SetStatus("Kein Vault geöffnet.");
            }
        }
        catch (Exception ex)
        {
            ShowError("Die Anwendung konnte nicht initialisiert werden.", ex);
        }
        finally
        {
            _startupInProgress = false;
            if (!IsDisposed && !Disposing)
            {
                Enabled = wasEnabled;
                UseWaitCursor = previousWaitCursor;
            }
        }
    }

    /// <summary>
    /// Öffnet einen Vault über den Ordnerdialog.
    /// </summary>
    private async void openVaultToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Vault-Ordner für SASD Notes auswählen",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await OpenVaultAsync(dialog.SelectedPath);
    }

    /// <summary>
    /// Öffnet eine neue Notiz im derzeit ausgewählten Ordner.
    /// </summary>
    private async void newNoteToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_workspaceService.CurrentVault is null)
        {
            MessageBox.Show(this, "Bitte öffnen Sie zuerst einen Vault-Ordner.", "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Das Öffnen der neuen Notiz ersetzt den Editorinhalt und darf daher keine ungesicherten Änderungen verwerfen.
        if (!await ConfirmSaveChangesIfRequiredAsync())
        {
            return;
        }

        VaultTreeItem? selectedTreeItem = treeViewVault.SelectedNode?.Tag as VaultTreeItem;
        string? relativeFolderPath = _workspaceService.GetRelativeFolderPathForTreeItem(selectedTreeItem);

        using var dialog = new NewNoteDialog(relativeFolderPath);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            NoteDocument createdNote = await _workspaceService.CreateNoteAsync(dialog.NoteTitle, relativeFolderPath);
            RebuildTree();
            OpenNoteInEditor(createdNote);
            SetStatus($"Neue Notiz erstellt: {createdNote.RelativePath}");
        }
        catch (Exception ex)
        {
            ShowError("Die neue Notiz konnte nicht erstellt werden.", ex);
        }
    }

    /// <summary>
    /// Legt einen neuen Unterordner im aktuell ausgewählten Vault-Ordner an.
    /// </summary>
    private async void newFolderToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_workspaceService.CurrentVault is null)
        {
            MessageBox.Show(this, "Bitte öffnen Sie zuerst einen Vault-Ordner.", "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        VaultTreeItem? selectedTreeItem = treeViewVault.SelectedNode?.Tag as VaultTreeItem;
        string? relativeParentPath = _workspaceService.GetRelativeFolderPathForTreeItem(selectedTreeItem);

        using var dialog = new NewFolderDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FolderName))
        {
            return;
        }

        try
        {
            string relativeFolderPath = await _workspaceService.CreateFolderAsync(dialog.FolderName, relativeParentPath);
            RebuildTree();

            string fullFolderPath = Path.Combine(
                _workspaceService.CurrentVault.RootPath,
                relativeFolderPath.Replace('/', Path.DirectorySeparatorChar));
            SelectTreeNodeForPath(fullFolderPath);
            SetStatus($"Ordner erstellt: {relativeFolderPath}");
        }
        catch (Exception ex)
        {
            ShowError("Der neue Ordner konnte nicht erstellt werden.", ex);
        }
    }

    /// <summary>
    /// Lädt die aktuelle Notiz nach einer bestätigten Verwerfung lokaler Änderungen neu.
    /// </summary>
    private async void reloadNoteToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_currentNote is null)
        {
            SetStatus("Keine Notiz zum Neuladen geöffnet.");
            return;
        }

        if (_currentNote.IsDirty)
        {
            DialogResult result = MessageBox.Show(
                this,
                "Die aktuelle Notiz enthält ungespeicherte Änderungen. " +
                "Sollen diese Änderungen verworfen und die Datei neu geladen werden?",
                "SASD Notes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }
        }

        NoteDocument noteToReload = _currentNote;
        try
        {
            NoteDocument reloadedNote = await _workspaceService.ReloadNoteAsync(noteToReload);

            // Der Benutzer kann inzwischen eine andere Notiz geöffnet haben; der Leseabschluss darf ihn nicht zurückholen.
            if (!ReferenceEquals(_currentNote, noteToReload))
            {
                return;
            }

            OpenNoteInEditor(reloadedNote);
            SetStatus($"Notiz neu geladen: {reloadedNote.RelativePath}");
        }
        catch (Exception ex)
        {
            ShowError("Die aktuelle Notiz konnte nicht neu geladen werden.", ex);
        }
    }

    /// <summary>
    /// Speichert die aktuell geöffnete Notiz.
    /// </summary>
    private async void saveToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        await SaveCurrentNoteAsync();
    }

    /// <summary>
    /// Aktualisiert den geöffneten Vault aus dem Dateisystem.
    /// </summary>
    private async void refreshVaultToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_workspaceService.CurrentVault is null)
        {
            return;
        }

        string currentVaultPath = _workspaceService.CurrentVault.RootPath;
        await OpenVaultAsync(currentVaultPath);
    }

    /// <summary>
    /// Reagiert auf die Auswahl eines Knotens im Navigationsbaum.
    /// </summary>
    private async void treeViewVault_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is not VaultTreeItem item || item.ItemType != TreeItemType.Note)
        {
            return;
        }

        NoteDocument? note = _workspaceService.FindNoteByFullPath(item.FullPath);
        if (note is null)
        {
            return;
        }

        if (!await ConfirmSaveChangesIfRequiredAsync())
        {
            // Wenn der Benutzer den Wechsel abbricht, wird der alte Zustand beibehalten.
            return;
        }

        OpenNoteInEditor(note);
    }

    /// <summary>
    /// Markiert die geöffnete Notiz bei Textänderungen als geändert.
    /// </summary>
    private void richTextBoxEditor_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressEditorEvents || _currentNote is null)
        {
            return;
        }

        _workspaceService.UpdateWorkingContent(_currentNote, richTextBoxEditor.Text);
        UpdateWindowTitle();
        UpdateStatusMetrics();
        RefreshContextPanels();
    }

    /// <summary>
    /// Führt eine Suchanfrage über den aktuellen Vault aus.
    /// </summary>
    private void buttonSearch_Click(object? sender, EventArgs e)
    {
        ExecuteSearch();
    }

    /// <summary>
    /// Führt eine Suche über die Eingabetaste aus.
    /// </summary>
    private void toolStripTextBoxSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        ExecuteSearch();
        e.SuppressKeyPress = true;
    }

    /// <summary>
    /// Öffnet den in der Trefferliste ausgewählten Treffer.
    /// </summary>
    private async void listBoxSearchResults_DoubleClick(object? sender, EventArgs e)
    {
        if (listBoxSearchResults.SelectedItem is not SearchHit selectedHit)
        {
            return;
        }

        NoteDocument? targetNote = _workspaceService.LoadedNotes.FirstOrDefault(note =>
            string.Equals(note.RelativePath, selectedHit.RelativePath, StringComparison.OrdinalIgnoreCase));

        if (targetNote is null)
        {
            return;
        }

        if (!await ConfirmSaveChangesIfRequiredAsync())
        {
            return;
        }

        // Eine seit der Suche bearbeitete Notiz kann den Treffer verschoben oder entfernt haben.
        // Die Application-Logik bestätigt deshalb den Treffertext, bevor die UI eine Auswahl setzt.
        var contentSelection = selectedHit.MatchLength > 0
            ? SearchHitLocator.Locate(selectedHit, targetNote.Content)
            : null;

        OpenNoteInEditor(targetNote);
        if (selectedHit.MatchLength <= 0)
        {
            return;
        }

        if (contentSelection is not { } currentRange)
        {
            SetStatus("Der Suchtreffer ist im aktuellen Inhalt nicht mehr vorhanden. Bitte suchen Sie erneut.");
            return;
        }

        // Suche und Locator verwenden den Originaltext; RichTextBox kann dessen Zeilenenden normalisieren.
        var selection = EditorTextPositionMapper.Map(
            targetNote.Content, richTextBoxEditor.Text, currentRange.Start, currentRange.Length);
        if (selection is not { } mapped)
        {
            SetStatus("Der Suchtreffer lässt sich nicht sicher auf den Editorinhalt abbilden.");
            return;
        }

        richTextBoxEditor.Focus();
        richTextBoxEditor.SelectionStart = mapped.Start;
        richTextBoxEditor.SelectionLength = mapped.Length;
        richTextBoxEditor.ScrollToCaret();
    }

    /// <summary>
    /// Öffnet die Quellnotiz eines Backlinks.
    /// </summary>
    private async void listBoxBacklinks_DoubleClick(object? sender, EventArgs e)
    {
        if (listBoxBacklinks.SelectedItem is not BacklinkInfo backlink)
        {
            return;
        }

        NoteDocument? targetNote = _workspaceService.LoadedNotes.FirstOrDefault(note =>
            string.Equals(note.RelativePath, backlink.SourceRelativePath, StringComparison.OrdinalIgnoreCase));

        if (targetNote is null)
        {
            return;
        }

        if (!await ConfirmSaveChangesIfRequiredAsync())
        {
            return;
        }

        OpenNoteInEditor(targetNote);
    }

    /// <summary>
    /// Springt zur ausgewählten Überschrift im Editor.
    /// </summary>
    private void listBoxOutline_DoubleClick(object? sender, EventArgs e)
    {
        if (listBoxOutline.SelectedItem is not OutlineItem outline || _currentNote is null)
        {
            return;
        }

        var position = EditorTextPositionMapper.Map(_currentNote.Content, richTextBoxEditor.Text, outline.StartIndex, 0);
        if (position is not { } mapped)
        {
            SetStatus("Die Gliederungsposition passt nicht mehr zum Editorinhalt.");
            return;
        }

        richTextBoxEditor.Focus();
        richTextBoxEditor.SelectionStart = mapped.Start;
        richTextBoxEditor.SelectionLength = 0;
        richTextBoxEditor.ScrollToCaret();
    }

    /// <summary>
    /// Wendet eine einfache Markdown-Formatierung an.
    /// </summary>
    private void toolStripFormatButton_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripButton button || button.Tag is not MarkdownFormatKind formatKind)
        {
            return;
        }

        ApplyFormatting(formatKind);
    }

    /// <summary>
    /// Öffnet den an Cursorposition oder Auswahl liegenden Wiki-Link.
    /// </summary>
    private async void openSelectedLinkToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        await OpenSelectedLinkAsync();
    }

    /// <summary>
    /// Reagiert auf Tastaturkürzel innerhalb des Editors.
    /// </summary>
    private async void richTextBoxEditor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.S)
        {
            await SaveCurrentNoteAsync();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.B)
        {
            ApplyFormatting(MarkdownFormatKind.Bold);
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.I)
        {
            ApplyFormatting(MarkdownFormatKind.Italic);
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.Enter)
        {
            await OpenSelectedLinkAsync();
            e.SuppressKeyPress = true;
        }
    }

    /// <summary>
    /// Behandelt das Schließen des Fensters inklusive Dirty-State-Abfrage.
    /// </summary>
    private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_startupInProgress || _vaultOpenInProgress)
        {
            // Ein noch laufender Ladeabschluss darf nicht auf bereits freigegebene Controls zugreifen.
            e.Cancel = true;
            SetStatus("Bitte warten Sie, bis der Ladevorgang abgeschlossen ist.");
            return;
        }

        if (_closeApproved || e.Cancel)
        {
            return;
        }

        // WinForms wartet nicht auf async-void-Handler: Der erste Versuch muss vor dem await abgebrochen werden.
        e.Cancel = true;
        if (_closeConfirmationInProgress)
        {
            return;
        }

        _closeConfirmationInProgress = true;
        bool wasEnabled = Enabled;
        Enabled = false;

        try
        {
            if (!await ConfirmSaveChangesIfRequiredAsync())
            {
                return;
            }

            if (IsDisposed || Disposing)
            {
                return;
            }

            // Ein zweiter Schließversuch läuft erst nach vollständig abgeschlossener Bestätigung bzw. Speicherung.
            _closeApproved = true;
            BeginInvoke(new Action(Close));
        }
        catch (Exception ex)
        {
            ShowError("Das Fenster konnte nicht sicher geschlossen werden.", ex);
        }
        finally
        {
            _closeConfirmationInProgress = false;
            if (!IsDisposed && !Disposing)
            {
                Enabled = wasEnabled;
            }
        }
    }

    /// <summary>
    /// Gibt die periodische Änderungsprüfung nach dem tatsächlichen Schließen frei.
    /// </summary>
    /// <param name="e">Informationen über den abgeschlossenen Schließvorgang.</param>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Bei abgebrochenem Schließen bleibt der Timer aktiv; beim endgültigen Schließen wird er freigegeben.
        _externalChangeTimer.Dispose();
        base.OnFormClosed(e);
    }

    /// <summary>
    /// Öffnet einen Vault, lädt alle Notizen und aktualisiert die Oberfläche.
    /// </summary>
    /// <param name="vaultPath">Pfad des gewünschten Vaults.</param>
    /// <returns>Ob der Vault erfolgreich geöffnet wurde.</returns>
    private async Task<bool> OpenVaultAsync(string vaultPath)
    {
        if (_vaultOpenInProgress || _closeConfirmationInProgress || _closeApproved)
        {
            return false;
        }

        // Die Sperre beginnt vor einer eventuell asynchronen Speicherbestätigung, damit kein zweiter Wechsel startet.
        _vaultOpenInProgress = true;
        bool wasEnabled = Enabled;
        bool previousWaitCursor = UseWaitCursor;
        try
        {
            if (!await ConfirmSaveChangesIfRequiredAsync())
            {
                return false;
            }

            Enabled = false;
            UseWaitCursor = true;
            SetStatus("Vault wird geöffnet...");
            VaultInfo vaultInfo = await _workspaceService.OpenVaultAsync(vaultPath);
            RebuildTree();
            RefreshRecentFoldersMenu();
            _currentNote = null;
            ClearEditor();
            labelCurrentDocument.Text = "Keine Notiz geöffnet";

            // Ein bereits geöffnetes Recherchefenster darf keine Verlaufsreferenzen
            // auf Notizen des vorherigen Vaults behalten.
            _researchForm?.RefreshWorkspace(preferredNote: null);
            SetStatus($"Vault geöffnet: {vaultInfo.DisplayName}");
            return true;
        }
        catch (Exception ex)
        {
            ShowError("Der Vault konnte nicht geöffnet werden.", ex);
            return false;
        }
        finally
        {
            _vaultOpenInProgress = false;
            if (!IsDisposed && !Disposing)
            {
                Enabled = wasEnabled;
                UseWaitCursor = previousWaitCursor;
            }
        }
    }

    /// <summary>
    /// Baut die TreeView aus dem fachlichen Baumzustand neu auf.
    /// </summary>
    private void RebuildTree()
    {
        treeViewVault.BeginUpdate();
        treeViewVault.Nodes.Clear();

        VaultInfo? currentVault = _workspaceService.CurrentVault;
        if (currentVault is null)
        {
            treeViewVault.EndUpdate();
            return;
        }

        TreeNode rootNode = new(currentVault.DisplayName)
        {
            Tag = new VaultTreeItem
            {
                DisplayText = currentVault.DisplayName,
                FullPath = currentVault.RootPath,
                RelativePath = string.Empty,
                ItemType = TreeItemType.Root
            }
        };

        treeViewVault.Nodes.Add(rootNode);

        // Zuerst werden Ordner eingetragen, damit auch leere Ordner sichtbar bleiben.
        foreach (VaultTreeItem folder in _workspaceService.BuildTreeItems()
                     .Where(item => item.ItemType == TreeItemType.Folder)
                     .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            AddFolderNode(rootNode, folder);
        }

        // Anschließend werden die Markdown-Dateien in die bereits vorhandenen Ordner einsortiert.
        foreach (NoteDocument note in _workspaceService.LoadedNotes.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            AddNoteNode(rootNode, note);
        }

        rootNode.Expand();
        treeViewVault.EndUpdate();
        toolStripStatusVault.Text = currentVault.RootPath;
    }

    /// <summary>
    /// Fügt einen vorhandenen Ordnerknoten inklusive Zwischenordnern in die TreeView ein.
    /// </summary>
    private void AddFolderNode(TreeNode rootNode, VaultTreeItem folder)
    {
        string normalizedPath = folder.RelativePath.Replace(Path.DirectorySeparatorChar, '/');
        string[] parts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        TreeNode currentNode = rootNode;
        string currentRelativePath = string.Empty;

        foreach (string part in parts)
        {
            currentRelativePath = string.IsNullOrEmpty(currentRelativePath)
                ? part
                : currentRelativePath + "/" + part;

            TreeNode? existingNode = currentNode.Nodes
                .Cast<TreeNode>()
                .FirstOrDefault(node => string.Equals(node.Text, part, StringComparison.OrdinalIgnoreCase) &&
                                        node.Tag is VaultTreeItem item &&
                                        item.ItemType == TreeItemType.Folder);

            if (existingNode is null)
            {
                existingNode = new TreeNode(part)
                {
                    Tag = new VaultTreeItem
                    {
                        DisplayText = part,
                        FullPath = Path.Combine(_workspaceService.CurrentVault!.RootPath, currentRelativePath.Replace('/', Path.DirectorySeparatorChar)),
                        RelativePath = currentRelativePath,
                        ItemType = TreeItemType.Folder
                    }
                };

                currentNode.Nodes.Add(existingNode);
            }

            currentNode = existingNode;
        }
    }
    /// <summary>
    /// Fügt eine Notiz inklusive benötigter Zwischenordner in die TreeView ein.
    /// </summary>
    private void AddNoteNode(TreeNode rootNode, NoteDocument note)
    {
        string[] parts = note.RelativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        TreeNode currentNode = rootNode;
        string currentRelativePath = string.Empty;

        for (int index = 0; index < parts.Length - 1; index++)
        {
            currentRelativePath = string.IsNullOrEmpty(currentRelativePath)
                ? parts[index]
                : currentRelativePath + "/" + parts[index];

            TreeNode? existingFolderNode = currentNode.Nodes
                .Cast<TreeNode>()
                .FirstOrDefault(node => string.Equals(node.Text, parts[index], StringComparison.OrdinalIgnoreCase) && node.Tag is VaultTreeItem item && item.ItemType == TreeItemType.Folder);

            if (existingFolderNode is null)
            {
                existingFolderNode = new TreeNode(parts[index])
                {
                    Tag = new VaultTreeItem
                    {
                        DisplayText = parts[index],
                        FullPath = Path.Combine(_workspaceService.CurrentVault!.RootPath, currentRelativePath.Replace('/', Path.DirectorySeparatorChar)),
                        RelativePath = currentRelativePath,
                        ItemType = TreeItemType.Folder
                    }
                };

                currentNode.Nodes.Add(existingFolderNode);
            }

            currentNode = existingFolderNode;
        }

        currentNode.Nodes.Add(new TreeNode(parts[^1])
        {
            Tag = new VaultTreeItem
            {
                DisplayText = parts[^1],
                FullPath = note.FullPath,
                RelativePath = note.RelativePath,
                ItemType = TreeItemType.Note
            }
        });
    }

    /// <summary>
    /// Öffnet eine Notiz im Editor und aktualisiert die Kontextinformationen.
    /// </summary>
    private void OpenNoteInEditor(NoteDocument note)
    {
        _currentNote = note;
        _externalChangeWarningPath = null;
        _externalChangeWarningStatus = null;
        _suppressEditorEvents = true;
        richTextBoxEditor.Text = note.Content;
        _suppressEditorEvents = false;

        labelCurrentDocument.Text = note.RelativePath;
        UpdateWindowTitle();
        UpdateStatusMetrics();
        RefreshContextPanels();
        SelectTreeNodeForPath(note.FullPath);
        SetStatus($"Notiz geöffnet: {note.RelativePath}");
    }

    /// <summary>
    /// Wählt den Baumknoten passend zum Dateipfad aus.
    /// </summary>
    private void SelectTreeNodeForPath(string fullPath)
    {
        foreach (TreeNode rootNode in treeViewVault.Nodes)
        {
            TreeNode? foundNode = FindNodeByFullPath(rootNode, fullPath);
            if (foundNode is null)
            {
                continue;
            }

            treeViewVault.SelectedNode = foundNode;
            foundNode.EnsureVisible();
            return;
        }
    }

    /// <summary>
    /// Sucht rekursiv nach einem Baumknoten mit einem bestimmten Vollpfad.
    /// </summary>
    private static TreeNode? FindNodeByFullPath(TreeNode node, string fullPath)
    {
        if (node.Tag is VaultTreeItem item && string.Equals(item.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
        {
            return node;
        }

        foreach (TreeNode childNode in node.Nodes)
        {
            TreeNode? foundNode = FindNodeByFullPath(childNode, fullPath);
            if (foundNode is not null)
            {
                return foundNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Speichert die aktuelle Notiz, sofern vorhanden.
    /// </summary>
    private async Task<bool> SaveCurrentNoteAsync()
    {
        if (_currentNote is null)
        {
            return true;
        }

        // Die Referenz bleibt über das await stabil, selbst wenn inzwischen eine andere Notiz geöffnet wird.
        NoteDocument noteToSave = _currentNote;
        string contentToSave = richTextBoxEditor.Text;
        try
        {
            await _workspaceService.SaveNoteAsync(noteToSave, contentToSave);
            UpdateWindowTitle();
            UpdateStatusMetrics();
            RefreshContextPanels();
            SetStatus(noteToSave.IsDirty
                ? $"Textstand gespeichert; neuere Änderungen sind ungespeichert: {noteToSave.RelativePath}"
                : $"Notiz gespeichert: {noteToSave.RelativePath}");
            // Ein auf Speichern wartender Schließ- oder Wechselbefehl darf neuere Änderungen nicht verwerfen.
            return !noteToSave.IsDirty;
        }
        catch (Exception ex)
        {
            ShowError("Die aktuelle Notiz konnte nicht gespeichert werden.", ex);
            return false;
        }
    }

    /// <summary>
    /// Führt eine Suchanfrage über den geladenen Vault aus.
    /// </summary>
    private void ExecuteSearch()
    {
        listBoxSearchResults.BeginUpdate();
        listBoxSearchResults.Items.Clear();

        foreach (SearchHit hit in _workspaceService.Search(toolStripTextBoxSearch.Text))
        {
            listBoxSearchResults.Items.Add(hit);
        }

        listBoxSearchResults.EndUpdate();
        tabControlContext.SelectedTab = tabPageSearch;
        SetStatus($"Suche abgeschlossen: {listBoxSearchResults.Items.Count} Treffer.");
    }

    /// <summary>
    /// Aktualisiert Backlinks, Outline und Info-Panel für die aktuelle Notiz.
    /// </summary>
    private void RefreshContextPanels()
    {
        listBoxBacklinks.BeginUpdate();
        listBoxBacklinks.Items.Clear();
        listBoxOutline.BeginUpdate();
        listBoxOutline.Items.Clear();

        if (_currentNote is not null)
        {
            IReadOnlyList<WikiLink> unresolvedLinks = _workspaceService.GetUnresolvedLinks(_currentNote);

            foreach (BacklinkInfo backlink in _workspaceService.GetBacklinks(_currentNote))
            {
                listBoxBacklinks.Items.Add(backlink);
            }

            foreach (OutlineItem outline in _workspaceService.GetOutline(_currentNote))
            {
                listBoxOutline.Items.Add(outline);
            }

            textBoxInfo.Text = $"Titel: {_currentNote.Title}{Environment.NewLine}" +
                               $"Relativer Pfad: {_currentNote.RelativePath}{Environment.NewLine}" +
                               $"Letzte Änderung (UTC): {_currentNote.LastModifiedUtc:u}{Environment.NewLine}" +
                               $"Wiki-Links: {_currentNote.Links.Count}{Environment.NewLine}" +
                               $"Nicht auflösbare Links: {FormatUnresolvedLinks(unresolvedLinks)}{Environment.NewLine}" +
                               $"Status: {(_currentNote.IsDirty ? "ungespeichert" : "gespeichert")}";
        }
        else
        {
            textBoxInfo.Clear();
        }

        listBoxBacklinks.EndUpdate();
        listBoxOutline.EndUpdate();
    }

    /// <summary>
    /// Formatiert die nicht auflösbaren Linkziele für das Info-Panel.
    /// </summary>
    /// <param name="unresolvedLinks">Nicht auflösbare Links der aktuellen Notiz.</param>
    /// <returns>Anzahl und Ziele in kompakter Darstellung.</returns>
    private static string FormatUnresolvedLinks(IReadOnlyList<WikiLink> unresolvedLinks)
    {
        if (unresolvedLinks.Count == 0)
        {
            return "0";
        }

        string targets = string.Join(", ", unresolvedLinks.Select(link => link.Target));
        return $"{unresolvedLinks.Count} ({targets})";
    }

    /// <summary>
    /// Aktualisiert die Liste der zuletzt verwendeten Ordner im Menü.
    /// </summary>
    private void RefreshRecentFoldersMenu()
    {
        recentFoldersToolStripMenuItem.DropDownItems.Clear();
        IReadOnlyList<RecentFolderEntry> recentFolders = _workspaceService.GetRecentFolders();

        if (recentFolders.Count == 0)
        {
            recentFoldersToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem("(keine)") { Enabled = false });
            return;
        }

        foreach (RecentFolderEntry recentFolder in recentFolders)
        {
            var item = new ToolStripMenuItem(recentFolder.Path)
            {
                Tag = recentFolder.Path
            };
            item.Click += recentFolderMenuItem_Click;
            recentFoldersToolStripMenuItem.DropDownItems.Add(item);
        }
    }

    /// <summary>
    /// Öffnet einen Vault aus der Recent-Folders-Liste.
    /// </summary>
    private async void recentFolderMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem menuItem || menuItem.Tag is not string folderPath)
        {
            return;
        }

        if (!_workspaceService.VaultExists(folderPath))
        {
            MessageBox.Show(this, "Der zuletzt verwendete Ordner ist nicht mehr vorhanden.", "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await OpenVaultAsync(folderPath);
    }

    /// <summary>
    /// Aktualisiert Fenstertitel und Dirty-State-Anzeige.
    /// </summary>
    private void UpdateWindowTitle()
    {
        string documentName = _currentNote?.Title ?? "SASD Notes";
        string dirtyMarker = _currentNote?.IsDirty == true ? "* " : string.Empty;
        Text = dirtyMarker + documentName + " - SASD Notes";
    }

    /// <summary>
    /// Aktualisiert Wort- und Zeichenzähler in der Statusleiste.
    /// </summary>
    private void UpdateStatusMetrics()
    {
        string currentText = richTextBoxEditor.Text;
        int wordCount = currentText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        int characterCount = currentText.Length;

        toolStripStatusMetrics.Text = $"{wordCount} Wörter | {characterCount} Zeichen";
    }

    /// <summary>
    /// Leert den Editor bei geschlossenem oder gewechseltem Vault.
    /// </summary>
    private void ClearEditor()
    {
        _suppressEditorEvents = true;
        richTextBoxEditor.Clear();
        _suppressEditorEvents = false;
        textBoxInfo.Clear();
        listBoxBacklinks.Items.Clear();
        listBoxOutline.Items.Clear();
        UpdateWindowTitle();
        UpdateStatusMetrics();
    }

    /// <summary>
    /// Zeigt eine kurze Statusmeldung in der Statusleiste an.
    /// </summary>
    private void SetStatus(string message)
    {
        toolStripStatusMessage.Text = message;
    }

    /// <summary>
    /// Prüft regelmäßig den externen Dateistand der geöffneten Notiz.
    /// </summary>
    private void externalChangeTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentNote is null || _startupInProgress || _vaultOpenInProgress || _closeConfirmationInProgress)
        {
            return;
        }

        // Nur die geöffnete Notiz benötigt eine Warnung. Fehler anderer Dateien beeinflussen diese Prüfung nicht.
        ExternalChangeStatus status = _workspaceService.CheckExternalChange(_currentNote);
        if (status == ExternalChangeStatus.NotChecked)
        {
            return;
        }

        if (status == ExternalChangeStatus.Unchanged)
        {
            if (_externalChangeWarningStatus == ExternalChangeStatus.Unavailable)
            {
                SetStatus($"Dateiprüfung wieder möglich: {_currentNote.RelativePath}");
            }
            _externalChangeWarningPath = null;
            _externalChangeWarningStatus = null;
            return;
        }

        // Pro Datei und Zustand nur einmal melden; der nächste Tick versucht die Prüfung weiterhin erneut.
        if (string.Equals(_externalChangeWarningPath, _currentNote.FullPath, StringComparison.OrdinalIgnoreCase)
            && _externalChangeWarningStatus == status)
        {
            return;
        }

        _externalChangeWarningPath = _currentNote.FullPath;
        _externalChangeWarningStatus = status;
        SetStatus(status == ExternalChangeStatus.Unavailable
            ? $"Warnung: Externe Änderungen derzeit nicht prüfbar: {_currentNote.RelativePath}"
            : $"Warnung: Datei extern geändert oder gelöscht: {_currentNote.RelativePath}");
    }

    /// <summary>
    /// Zeigt einen Fehler verständlich an.
    /// </summary>
    private void ShowError(string userMessage, Exception exception)
    {
        MessageBox.Show(this, userMessage + Environment.NewLine + Environment.NewLine + exception.Message, "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        SetStatus(userMessage);
    }

    /// <summary>
    /// Fragt bei ungespeicherten Änderungen ab, ob gespeichert werden soll.
    /// </summary>
    /// <returns>Ob der angeforderte Wechsel oder Schließvorgang fortgesetzt werden darf.</returns>
    protected virtual async Task<bool> ConfirmSaveChangesIfRequiredAsync()
    {
        if (_currentNote?.IsDirty != true)
        {
            return true;
        }

        DialogResult dialogResult = MessageBox.Show(
            this,
            $"Die Notiz '{_currentNote.Title}' enthält ungespeicherte Änderungen. Möchten Sie jetzt speichern?",
            "SASD Notes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (dialogResult == DialogResult.Cancel)
        {
            return false;
        }

        if (dialogResult == DialogResult.Yes)
        {
            return await SaveCurrentNoteAsync();
        }

        // „Nicht speichern“ verwirft auch den zwischengespeicherten Arbeitsstand, nicht nur den sichtbaren Editor.
        if (!_workspaceService.TryDiscardWorkingChanges(_currentNote))
        {
            SetStatus("Änderungen konnten noch nicht verworfen werden. Bitte warten Sie auf laufende Speicherung.");
            return false;
        }

        // Ein nachfolgender Dialog oder Vault-Wechsel kann weiterhin abgebrochen werden bzw. fehlschlagen.
        // Deshalb wird bereits jetzt auch der verbleibende Editor auf den wiederhergestellten Stand gebracht.
        _suppressEditorEvents = true;
        richTextBoxEditor.Text = _currentNote.Content;
        _suppressEditorEvents = false;
        UpdateWindowTitle();
        UpdateStatusMetrics();
        RefreshContextPanels();
        return true;
    }

    /// <summary>
    /// Wendet eine Markdown-Formatierung auf die aktuelle Auswahl an.
    /// </summary>
    private void ApplyFormatting(MarkdownFormatKind formatKind)
    {
        if (_currentNote is null)
        {
            return;
        }

        TextSelectionTransformResult result = _editorFormattingService.Apply(
            richTextBoxEditor.Text,
            richTextBoxEditor.SelectionStart,
            richTextBoxEditor.SelectionLength,
            formatKind);

        _suppressEditorEvents = true;
        richTextBoxEditor.Text = result.UpdatedText;
        richTextBoxEditor.SelectionStart = result.SelectionStart;
        richTextBoxEditor.SelectionLength = result.SelectionLength;
        _suppressEditorEvents = false;

        _workspaceService.UpdateWorkingContent(_currentNote, richTextBoxEditor.Text);
        UpdateWindowTitle();
        UpdateStatusMetrics();
        RefreshContextPanels();
        richTextBoxEditor.Focus();
    }

    /// <summary>
    /// Versucht, den ausgewählten oder umschlossenen Wiki-Link zu öffnen.
    /// </summary>
    private async Task OpenSelectedLinkAsync()
    {
        if (_currentNote is null)
        {
            return;
        }

        // Die Rückrichtung ist genauso wichtig: Cursor und Auswahl stammen aus dem Editor,
        // während die gültigen Wiki-Link-Bereiche im Originaltext ermittelt wurden.
        var selection = EditorTextPositionMapper.Map(
            richTextBoxEditor.Text, _currentNote.Content,
            richTextBoxEditor.SelectionStart, richTextBoxEditor.SelectionLength);
        if (selection is not { } mapped)
        {
            SetStatus("Die Editorposition passt nicht mehr zum Notizinhalt.");
            return;
        }

        string? selectedText = mapped.Length == 0 ? null : _currentNote.Content.Substring(mapped.Start, mapped.Length);
        NoteDocument? targetNote = _workspaceService.TryResolveLink(_currentNote, mapped.Start, selectedText);

        if (targetNote is null)
        {
            MessageBox.Show(this, "An der aktuellen Auswahl konnte kein gültiger Wiki-Link aufgelöst werden.", "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!await ConfirmSaveChangesIfRequiredAsync())
        {
            return;
        }

        OpenNoteInEditor(targetNote);
    }

    /// <summary>
    /// Öffnet oder aktiviert die eigenständige Rechercheansicht.
    /// </summary>
    private void researchToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_researchForm is not null && !_researchForm.IsDisposed)
        {
            _researchForm.Activate();
            return;
        }

        _researchForm = new ResearchForm(_workspaceService, OpenNoteFromResearchAsync);
        _researchForm.FormClosed += (_, _) => _researchForm = null;
        _researchForm.RefreshWorkspace(_currentNote);
        _researchForm.Show(this);
    }

    /// <summary>
    /// Öffnet eine Recherche-Notiz erst nach der üblichen Dirty-State-Prüfung im Editor.
    /// </summary>
    /// <param name="requestedNote">Von der Rechercheansicht ausgewählte Notiz.</param>
    /// <returns><see langword="true"/>, wenn die Notiz im Editor geöffnet wurde.</returns>
    private async Task<bool> OpenNoteFromResearchAsync(NoteDocument requestedNote)
    {
        NoteDocument? liveNote = _workspaceService.LoadedNotes.FirstOrDefault(note =>
            string.Equals(note.RelativePath, requestedNote.RelativePath, StringComparison.OrdinalIgnoreCase));
        if (liveNote is null)
        {
            SetStatus("Die Recherche-Notiz gehört nicht mehr zum aktuellen Vault.");
            return false;
        }

        if (!ReferenceEquals(_currentNote, liveNote) && !await ConfirmSaveChangesIfRequiredAsync())
        {
            return false;
        }

        OpenNoteInEditor(liveNote);
        Activate();
        return true;
    }

    /// <summary>
    /// Öffnet eine kurze Info-Box zur Anwendung.
    /// </summary>
    private void aboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            this,
            "SASD Notes\n\n" +
            "Lokale Markdown-Notizen mit Wiki-Links, Suche, Backlinks und thematischen Ordnern.\n\n" +
            "V1 WinForms / .NET 8",
            "Über SASD Notes",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}

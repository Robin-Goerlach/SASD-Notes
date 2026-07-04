using Sasd.Notes.App.WinForms.Dialogs;
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
    private NoteDocument? _currentNote;
    private bool _suppressEditorEvents;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="MainForm"/>-Klasse.
    /// </summary>
    public MainForm(NotesWorkspaceService workspaceService, EditorFormattingService editorFormattingService)
    {
        _workspaceService = workspaceService;
        _editorFormattingService = editorFormattingService;
        InitializeComponent();
    }

    /// <summary>
    /// Führt Startarbeiten nach dem Laden des Fensters aus.
    /// </summary>
    private async void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            await _workspaceService.InitializeAsync();
            RefreshRecentFoldersMenu();

            string? lastOpenedVaultPath = _workspaceService.GetLastOpenedVaultPath();
            if (!string.IsNullOrWhiteSpace(lastOpenedVaultPath) && Directory.Exists(lastOpenedVaultPath))
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

        OpenNoteInEditor(targetNote);

        if (selectedHit.MatchIndex >= 0 && selectedHit.MatchIndex < richTextBoxEditor.TextLength)
        {
            richTextBoxEditor.Focus();
            richTextBoxEditor.SelectionStart = selectedHit.MatchIndex;
            richTextBoxEditor.SelectionLength = Math.Min(toolStripTextBoxSearch.Text.Length, richTextBoxEditor.TextLength - selectedHit.MatchIndex);
            richTextBoxEditor.ScrollToCaret();
        }
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

        richTextBoxEditor.Focus();
        richTextBoxEditor.SelectionStart = Math.Clamp(outline.StartIndex, 0, richTextBoxEditor.TextLength);
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
        bool mayClose = await ConfirmSaveChangesIfRequiredAsync();
        if (!mayClose)
        {
            e.Cancel = true;
        }
    }

    /// <summary>
    /// Öffnet einen Vault, lädt alle Notizen und aktualisiert die Oberfläche.
    /// </summary>
    private async Task OpenVaultAsync(string vaultPath)
    {
        if (!await ConfirmSaveChangesIfRequiredAsync())
        {
            return;
        }

        try
        {
            VaultInfo vaultInfo = await _workspaceService.OpenVaultAsync(vaultPath);
            RebuildTree();
            RefreshRecentFoldersMenu();
            _currentNote = null;
            ClearEditor();
            labelCurrentDocument.Text = "Keine Notiz geöffnet";
            SetStatus($"Vault geöffnet: {vaultInfo.DisplayName}");
        }
        catch (Exception ex)
        {
            ShowError("Der Vault konnte nicht geöffnet werden.", ex);
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

        // Der Baum wird aus relativen Pfaden rekonstruiert und alphabetisch erweitert.
        foreach (NoteDocument note in _workspaceService.LoadedNotes.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            AddNoteNode(rootNode, note);
        }

        rootNode.Expand();
        treeViewVault.EndUpdate();
        toolStripStatusVault.Text = currentVault.RootPath;
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

        try
        {
            await _workspaceService.SaveNoteAsync(_currentNote, richTextBoxEditor.Text);
            UpdateWindowTitle();
            UpdateStatusMetrics();
            RefreshContextPanels();
            SetStatus($"Notiz gespeichert: {_currentNote.RelativePath}");
            return true;
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

        if (!Directory.Exists(folderPath))
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
    private async Task<bool> ConfirmSaveChangesIfRequiredAsync()
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

        NoteDocument? targetNote = _workspaceService.TryResolveLink(
            _currentNote,
            richTextBoxEditor.SelectionStart,
            richTextBoxEditor.SelectedText);

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
    /// Öffnet eine kurze Info-Box zur Anwendung.
    /// </summary>
    private void aboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            this,
            "SASD Notes

Lokale Markdown-Notizen mit Wiki-Links, Suche, Backlinks und thematischen Ordnern.

V1 WinForms / .NET 8",
            "Über SASD Notes",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}

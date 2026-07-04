using System.Drawing;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.App.WinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private MenuStrip menuStripMain;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem openVaultToolStripMenuItem;
    private ToolStripMenuItem recentFoldersToolStripMenuItem;
    private ToolStripMenuItem newNoteToolStripMenuItem;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem refreshVaultToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem editToolStripMenuItem;
    private ToolStripMenuItem openSelectedLinkToolStripMenuItem;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStrip toolStripMain;
    private ToolStripButton toolStripButtonOpenVault;
    private ToolStripButton toolStripButtonNewNote;
    private ToolStripButton toolStripButtonSave;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton toolStripButtonHeading1;
    private ToolStripButton toolStripButtonHeading2;
    private ToolStripButton toolStripButtonHeading3;
    private ToolStripButton toolStripButtonBold;
    private ToolStripButton toolStripButtonItalic;
    private ToolStripButton toolStripButtonCode;
    private ToolStripButton toolStripButtonBulletList;
    private ToolStripButton toolStripButtonCheckList;
    private ToolStripButton toolStripButtonWikiLink;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripLabel toolStripLabelSearch;
    private ToolStripTextBox toolStripTextBoxSearch;
    private ToolStripButton toolStripButtonSearch;
    private StatusStrip statusStripMain;
    private ToolStripStatusLabel toolStripStatusMessage;
    private ToolStripStatusLabel toolStripStatusVault;
    private ToolStripStatusLabel toolStripStatusMetrics;
    private SplitContainer splitContainerLeftRight;
    private SplitContainer splitContainerEditorContext;
    private TreeView treeViewVault;
    private Label labelCurrentDocument;
    private RichTextBox richTextBoxEditor;
    private TabControl tabControlContext;
    private TabPage tabPageSearch;
    private TabPage tabPageBacklinks;
    private TabPage tabPageOutline;
    private TabPage tabPageInfo;
    private ListBox listBoxSearchResults;
    private ListBox listBoxBacklinks;
    private ListBox listBoxOutline;
    private TextBox textBoxInfo;
    private Panel panelEditorHeader;

    /// <summary>
    /// Bereinigt verwendete Ressourcen.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Initialisiert das Hauptfenster und alle enthaltenen Steuerelemente.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuStripMain = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        openVaultToolStripMenuItem = new ToolStripMenuItem();
        recentFoldersToolStripMenuItem = new ToolStripMenuItem();
        newNoteToolStripMenuItem = new ToolStripMenuItem();
        saveToolStripMenuItem = new ToolStripMenuItem();
        refreshVaultToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        editToolStripMenuItem = new ToolStripMenuItem();
        openSelectedLinkToolStripMenuItem = new ToolStripMenuItem();
        helpToolStripMenuItem = new ToolStripMenuItem();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        toolStripMain = new ToolStrip();
        toolStripButtonOpenVault = new ToolStripButton();
        toolStripButtonNewNote = new ToolStripButton();
        toolStripButtonSave = new ToolStripButton();
        toolStripSeparator1 = new ToolStripSeparator();
        toolStripButtonHeading1 = new ToolStripButton();
        toolStripButtonHeading2 = new ToolStripButton();
        toolStripButtonHeading3 = new ToolStripButton();
        toolStripButtonBold = new ToolStripButton();
        toolStripButtonItalic = new ToolStripButton();
        toolStripButtonCode = new ToolStripButton();
        toolStripButtonBulletList = new ToolStripButton();
        toolStripButtonCheckList = new ToolStripButton();
        toolStripButtonWikiLink = new ToolStripButton();
        toolStripSeparator2 = new ToolStripSeparator();
        toolStripLabelSearch = new ToolStripLabel();
        toolStripTextBoxSearch = new ToolStripTextBox();
        toolStripButtonSearch = new ToolStripButton();
        statusStripMain = new StatusStrip();
        toolStripStatusMessage = new ToolStripStatusLabel();
        toolStripStatusVault = new ToolStripStatusLabel();
        toolStripStatusMetrics = new ToolStripStatusLabel();
        splitContainerLeftRight = new SplitContainer();
        treeViewVault = new TreeView();
        splitContainerEditorContext = new SplitContainer();
        richTextBoxEditor = new RichTextBox();
        panelEditorHeader = new Panel();
        labelCurrentDocument = new Label();
        tabControlContext = new TabControl();
        tabPageSearch = new TabPage();
        listBoxSearchResults = new ListBox();
        tabPageBacklinks = new TabPage();
        listBoxBacklinks = new ListBox();
        tabPageOutline = new TabPage();
        listBoxOutline = new ListBox();
        tabPageInfo = new TabPage();
        textBoxInfo = new TextBox();
        menuStripMain.SuspendLayout();
        toolStripMain.SuspendLayout();
        statusStripMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainerLeftRight).BeginInit();
        splitContainerLeftRight.Panel1.SuspendLayout();
        splitContainerLeftRight.Panel2.SuspendLayout();
        splitContainerLeftRight.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainerEditorContext).BeginInit();
        splitContainerEditorContext.Panel1.SuspendLayout();
        splitContainerEditorContext.Panel2.SuspendLayout();
        splitContainerEditorContext.SuspendLayout();
        panelEditorHeader.SuspendLayout();
        tabControlContext.SuspendLayout();
        tabPageSearch.SuspendLayout();
        tabPageBacklinks.SuspendLayout();
        tabPageOutline.SuspendLayout();
        tabPageInfo.SuspendLayout();
        SuspendLayout();
        // 
        // menuStripMain
        // 
        menuStripMain.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, helpToolStripMenuItem });
        menuStripMain.Location = new Point(0, 0);
        menuStripMain.Name = "menuStripMain";
        menuStripMain.Size = new Size(1400, 24);
        menuStripMain.TabIndex = 0;
        menuStripMain.Text = "menuStripMain";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openVaultToolStripMenuItem, recentFoldersToolStripMenuItem, newNoteToolStripMenuItem, saveToolStripMenuItem, refreshVaultToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(46, 20);
        fileToolStripMenuItem.Text = "Datei";
        // 
        // openVaultToolStripMenuItem
        // 
        openVaultToolStripMenuItem.Name = "openVaultToolStripMenuItem";
        openVaultToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
        openVaultToolStripMenuItem.Size = new Size(224, 22);
        openVaultToolStripMenuItem.Text = "Vault öffnen...";
        openVaultToolStripMenuItem.Click += openVaultToolStripMenuItem_Click;
        // 
        // recentFoldersToolStripMenuItem
        // 
        recentFoldersToolStripMenuItem.Name = "recentFoldersToolStripMenuItem";
        recentFoldersToolStripMenuItem.Size = new Size(224, 22);
        recentFoldersToolStripMenuItem.Text = "Recent Folders";
        // 
        // newNoteToolStripMenuItem
        // 
        newNoteToolStripMenuItem.Name = "newNoteToolStripMenuItem";
        newNoteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
        newNoteToolStripMenuItem.Size = new Size(224, 22);
        newNoteToolStripMenuItem.Text = "Neue Notiz";
        newNoteToolStripMenuItem.Click += newNoteToolStripMenuItem_Click;
        // 
        // saveToolStripMenuItem
        // 
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        saveToolStripMenuItem.Size = new Size(224, 22);
        saveToolStripMenuItem.Text = "Speichern";
        saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
        // 
        // refreshVaultToolStripMenuItem
        // 
        refreshVaultToolStripMenuItem.Name = "refreshVaultToolStripMenuItem";
        refreshVaultToolStripMenuItem.ShortcutKeys = Keys.F5;
        refreshVaultToolStripMenuItem.Size = new Size(224, 22);
        refreshVaultToolStripMenuItem.Text = "Vault aktualisieren";
        refreshVaultToolStripMenuItem.Click += refreshVaultToolStripMenuItem_Click;
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(224, 22);
        exitToolStripMenuItem.Text = "Beenden";
        exitToolStripMenuItem.Click += (sender, args) => Close();
        // 
        // editToolStripMenuItem
        // 
        editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openSelectedLinkToolStripMenuItem });
        editToolStripMenuItem.Name = "editToolStripMenuItem";
        editToolStripMenuItem.Size = new Size(75, 20);
        editToolStripMenuItem.Text = "Bearbeiten";
        // 
        // openSelectedLinkToolStripMenuItem
        // 
        openSelectedLinkToolStripMenuItem.Name = "openSelectedLinkToolStripMenuItem";
        openSelectedLinkToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Enter;
        openSelectedLinkToolStripMenuItem.Size = new Size(265, 22);
        openSelectedLinkToolStripMenuItem.Text = "Ausgewählten Link öffnen";
        openSelectedLinkToolStripMenuItem.Click += openSelectedLinkToolStripMenuItem_Click;
        // 
        // helpToolStripMenuItem
        // 
        helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
        helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        helpToolStripMenuItem.Size = new Size(44, 20);
        helpToolStripMenuItem.Text = "Hilfe";
        // 
        // aboutToolStripMenuItem
        // 
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new Size(139, 22);
        aboutToolStripMenuItem.Text = "Über ...";
        aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
        // 
        // toolStripMain
        // 
        toolStripMain.Items.AddRange(new ToolStripItem[]
        {
            toolStripButtonOpenVault,
            toolStripButtonNewNote,
            toolStripButtonSave,
            toolStripSeparator1,
            toolStripButtonHeading1,
            toolStripButtonHeading2,
            toolStripButtonHeading3,
            toolStripButtonBold,
            toolStripButtonItalic,
            toolStripButtonCode,
            toolStripButtonBulletList,
            toolStripButtonCheckList,
            toolStripButtonWikiLink,
            toolStripSeparator2,
            toolStripLabelSearch,
            toolStripTextBoxSearch,
            toolStripButtonSearch
        });
        toolStripMain.Location = new Point(0, 24);
        toolStripMain.Name = "toolStripMain";
        toolStripMain.Size = new Size(1400, 25);
        toolStripMain.TabIndex = 1;
        // 
        // toolStrip buttons
        // 
        toolStripButtonOpenVault.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonOpenVault.Text = "Ordner";
        toolStripButtonOpenVault.Click += openVaultToolStripMenuItem_Click;
        toolStripButtonNewNote.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonNewNote.Text = "Neu";
        toolStripButtonNewNote.Click += newNoteToolStripMenuItem_Click;
        toolStripButtonSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonSave.Text = "Speichern";
        toolStripButtonSave.Click += saveToolStripMenuItem_Click;
        toolStripButtonHeading1.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonHeading1.Text = "H1";
        toolStripButtonHeading1.Tag = MarkdownFormatKind.Heading1;
        toolStripButtonHeading1.Click += toolStripFormatButton_Click;
        toolStripButtonHeading2.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonHeading2.Text = "H2";
        toolStripButtonHeading2.Tag = MarkdownFormatKind.Heading2;
        toolStripButtonHeading2.Click += toolStripFormatButton_Click;
        toolStripButtonHeading3.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonHeading3.Text = "H3";
        toolStripButtonHeading3.Tag = MarkdownFormatKind.Heading3;
        toolStripButtonHeading3.Click += toolStripFormatButton_Click;
        toolStripButtonBold.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonBold.Text = "B";
        toolStripButtonBold.Tag = MarkdownFormatKind.Bold;
        toolStripButtonBold.Click += toolStripFormatButton_Click;
        toolStripButtonItalic.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonItalic.Text = "I";
        toolStripButtonItalic.Tag = MarkdownFormatKind.Italic;
        toolStripButtonItalic.Click += toolStripFormatButton_Click;
        toolStripButtonCode.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonCode.Text = "Code";
        toolStripButtonCode.Tag = MarkdownFormatKind.InlineCode;
        toolStripButtonCode.Click += toolStripFormatButton_Click;
        toolStripButtonBulletList.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonBulletList.Text = "Liste";
        toolStripButtonBulletList.Tag = MarkdownFormatKind.BulletList;
        toolStripButtonBulletList.Click += toolStripFormatButton_Click;
        toolStripButtonCheckList.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonCheckList.Text = "Check";
        toolStripButtonCheckList.Tag = MarkdownFormatKind.CheckList;
        toolStripButtonCheckList.Click += toolStripFormatButton_Click;
        toolStripButtonWikiLink.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonWikiLink.Text = "[[Link]]";
        toolStripButtonWikiLink.Tag = MarkdownFormatKind.WikiLink;
        toolStripButtonWikiLink.Click += toolStripFormatButton_Click;
        toolStripLabelSearch.Text = "Suche";
        toolStripTextBoxSearch.AutoSize = false;
        toolStripTextBoxSearch.Size = new Size(240, 25);
        toolStripTextBoxSearch.KeyDown += toolStripTextBoxSearch_KeyDown;
        toolStripButtonSearch.DisplayStyle = ToolStripItemDisplayStyle.Text;
        toolStripButtonSearch.Text = "Suchen";
        toolStripButtonSearch.Click += buttonSearch_Click;
        // 
        // statusStripMain
        // 
        statusStripMain.Items.AddRange(new ToolStripItem[] { toolStripStatusMessage, toolStripStatusVault, toolStripStatusMetrics });
        statusStripMain.Location = new Point(0, 739);
        statusStripMain.Name = "statusStripMain";
        statusStripMain.Size = new Size(1400, 22);
        statusStripMain.TabIndex = 2;
        toolStripStatusMessage.Spring = true;
        toolStripStatusMessage.TextAlign = ContentAlignment.MiddleLeft;
        toolStripStatusVault.Spring = true;
        toolStripStatusVault.TextAlign = ContentAlignment.MiddleLeft;
        toolStripStatusMetrics.Text = "0 Wörter | 0 Zeichen";
        // 
        // splitContainerLeftRight
        // 
        splitContainerLeftRight.Dock = DockStyle.Fill;
        splitContainerLeftRight.Location = new Point(0, 49);
        splitContainerLeftRight.Name = "splitContainerLeftRight";
        splitContainerLeftRight.Panel1.Controls.Add(treeViewVault);
        splitContainerLeftRight.Panel2.Controls.Add(splitContainerEditorContext);
        splitContainerLeftRight.Size = new Size(1400, 690);
        splitContainerLeftRight.SplitterDistance = 320;
        splitContainerLeftRight.TabIndex = 3;
        // 
        // treeViewVault
        // 
        treeViewVault.Dock = DockStyle.Fill;
        treeViewVault.HideSelection = false;
        treeViewVault.Location = new Point(0, 0);
        treeViewVault.Name = "treeViewVault";
        treeViewVault.Size = new Size(320, 690);
        treeViewVault.TabIndex = 0;
        treeViewVault.AfterSelect += treeViewVault_AfterSelect;
        // 
        // splitContainerEditorContext
        // 
        splitContainerEditorContext.Dock = DockStyle.Fill;
        splitContainerEditorContext.Location = new Point(0, 0);
        splitContainerEditorContext.Name = "splitContainerEditorContext";
        splitContainerEditorContext.Panel1.Controls.Add(richTextBoxEditor);
        splitContainerEditorContext.Panel1.Controls.Add(panelEditorHeader);
        splitContainerEditorContext.Panel2.Controls.Add(tabControlContext);
        splitContainerEditorContext.Size = new Size(1076, 690);
        splitContainerEditorContext.SplitterDistance = 720;
        splitContainerEditorContext.TabIndex = 0;
        // 
        // panelEditorHeader
        // 
        panelEditorHeader.Controls.Add(labelCurrentDocument);
        panelEditorHeader.Dock = DockStyle.Top;
        panelEditorHeader.Location = new Point(0, 0);
        panelEditorHeader.Name = "panelEditorHeader";
        panelEditorHeader.Size = new Size(720, 34);
        panelEditorHeader.TabIndex = 0;
        // 
        // labelCurrentDocument
        // 
        labelCurrentDocument.AutoEllipsis = true;
        labelCurrentDocument.Dock = DockStyle.Fill;
        labelCurrentDocument.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        labelCurrentDocument.Location = new Point(0, 0);
        labelCurrentDocument.Name = "labelCurrentDocument";
        labelCurrentDocument.Padding = new Padding(8, 8, 8, 0);
        labelCurrentDocument.Size = new Size(720, 34);
        labelCurrentDocument.TabIndex = 0;
        labelCurrentDocument.Text = "Keine Notiz geöffnet";
        // 
        // richTextBoxEditor
        // 
        richTextBoxEditor.AcceptsTab = true;
        richTextBoxEditor.BorderStyle = BorderStyle.None;
        richTextBoxEditor.Dock = DockStyle.Fill;
        richTextBoxEditor.Font = new Font("Consolas", 11F, FontStyle.Regular, GraphicsUnit.Point);
        richTextBoxEditor.Location = new Point(0, 34);
        richTextBoxEditor.Name = "richTextBoxEditor";
        richTextBoxEditor.Size = new Size(720, 656);
        richTextBoxEditor.TabIndex = 1;
        richTextBoxEditor.Text = "";
        richTextBoxEditor.WordWrap = false;
        richTextBoxEditor.TextChanged += richTextBoxEditor_TextChanged;
        richTextBoxEditor.KeyDown += richTextBoxEditor_KeyDown;
        // 
        // tabControlContext
        // 
        tabControlContext.Controls.Add(tabPageSearch);
        tabControlContext.Controls.Add(tabPageBacklinks);
        tabControlContext.Controls.Add(tabPageOutline);
        tabControlContext.Controls.Add(tabPageInfo);
        tabControlContext.Dock = DockStyle.Fill;
        tabControlContext.Location = new Point(0, 0);
        tabControlContext.Name = "tabControlContext";
        tabControlContext.SelectedIndex = 0;
        tabControlContext.Size = new Size(352, 690);
        tabControlContext.TabIndex = 0;
        // 
        // tabPageSearch
        // 
        tabPageSearch.Controls.Add(listBoxSearchResults);
        tabPageSearch.Location = new Point(4, 24);
        tabPageSearch.Name = "tabPageSearch";
        tabPageSearch.Padding = new Padding(3);
        tabPageSearch.Size = new Size(344, 662);
        tabPageSearch.TabIndex = 0;
        tabPageSearch.Text = "Suche";
        tabPageSearch.UseVisualStyleBackColor = true;
        // 
        // listBoxSearchResults
        // 
        listBoxSearchResults.Dock = DockStyle.Fill;
        listBoxSearchResults.FormattingEnabled = true;
        listBoxSearchResults.ItemHeight = 15;
        listBoxSearchResults.Location = new Point(3, 3);
        listBoxSearchResults.Name = "listBoxSearchResults";
        listBoxSearchResults.Size = new Size(338, 656);
        listBoxSearchResults.TabIndex = 0;
        listBoxSearchResults.DoubleClick += listBoxSearchResults_DoubleClick;
        // 
        // tabPageBacklinks
        // 
        tabPageBacklinks.Controls.Add(listBoxBacklinks);
        tabPageBacklinks.Location = new Point(4, 24);
        tabPageBacklinks.Name = "tabPageBacklinks";
        tabPageBacklinks.Padding = new Padding(3);
        tabPageBacklinks.Size = new Size(344, 662);
        tabPageBacklinks.TabIndex = 1;
        tabPageBacklinks.Text = "Backlinks";
        tabPageBacklinks.UseVisualStyleBackColor = true;
        // 
        // listBoxBacklinks
        // 
        listBoxBacklinks.Dock = DockStyle.Fill;
        listBoxBacklinks.FormattingEnabled = true;
        listBoxBacklinks.ItemHeight = 15;
        listBoxBacklinks.Location = new Point(3, 3);
        listBoxBacklinks.Name = "listBoxBacklinks";
        listBoxBacklinks.Size = new Size(338, 656);
        listBoxBacklinks.TabIndex = 0;
        listBoxBacklinks.DoubleClick += listBoxBacklinks_DoubleClick;
        // 
        // tabPageOutline
        // 
        tabPageOutline.Controls.Add(listBoxOutline);
        tabPageOutline.Location = new Point(4, 24);
        tabPageOutline.Name = "tabPageOutline";
        tabPageOutline.Padding = new Padding(3);
        tabPageOutline.Size = new Size(344, 662);
        tabPageOutline.TabIndex = 2;
        tabPageOutline.Text = "Gliederung";
        tabPageOutline.UseVisualStyleBackColor = true;
        // 
        // listBoxOutline
        // 
        listBoxOutline.Dock = DockStyle.Fill;
        listBoxOutline.FormattingEnabled = true;
        listBoxOutline.ItemHeight = 15;
        listBoxOutline.Location = new Point(3, 3);
        listBoxOutline.Name = "listBoxOutline";
        listBoxOutline.Size = new Size(338, 656);
        listBoxOutline.TabIndex = 0;
        listBoxOutline.DoubleClick += listBoxOutline_DoubleClick;
        // 
        // tabPageInfo
        // 
        tabPageInfo.Controls.Add(textBoxInfo);
        tabPageInfo.Location = new Point(4, 24);
        tabPageInfo.Name = "tabPageInfo";
        tabPageInfo.Padding = new Padding(3);
        tabPageInfo.Size = new Size(344, 662);
        tabPageInfo.TabIndex = 3;
        tabPageInfo.Text = "Info";
        tabPageInfo.UseVisualStyleBackColor = true;
        // 
        // textBoxInfo
        // 
        textBoxInfo.Dock = DockStyle.Fill;
        textBoxInfo.Location = new Point(3, 3);
        textBoxInfo.Multiline = true;
        textBoxInfo.Name = "textBoxInfo";
        textBoxInfo.ReadOnly = true;
        textBoxInfo.ScrollBars = ScrollBars.Vertical;
        textBoxInfo.Size = new Size(338, 656);
        textBoxInfo.TabIndex = 0;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1400, 761);
        Controls.Add(splitContainerLeftRight);
        Controls.Add(statusStripMain);
        Controls.Add(toolStripMain);
        Controls.Add(menuStripMain);
        MainMenuStrip = menuStripMain;
        MinimumSize = new Size(1100, 700);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SASD Notes";
        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
        menuStripMain.ResumeLayout(false);
        menuStripMain.PerformLayout();
        toolStripMain.ResumeLayout(false);
        toolStripMain.PerformLayout();
        statusStripMain.ResumeLayout(false);
        statusStripMain.PerformLayout();
        splitContainerLeftRight.Panel1.ResumeLayout(false);
        splitContainerLeftRight.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainerLeftRight).EndInit();
        splitContainerLeftRight.ResumeLayout(false);
        splitContainerEditorContext.Panel1.ResumeLayout(false);
        splitContainerEditorContext.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainerEditorContext).EndInit();
        splitContainerEditorContext.ResumeLayout(false);
        panelEditorHeader.ResumeLayout(false);
        tabControlContext.ResumeLayout(false);
        tabPageSearch.ResumeLayout(false);
        tabPageBacklinks.ResumeLayout(false);
        tabPageOutline.ResumeLayout(false);
        tabPageInfo.ResumeLayout(false);
        tabPageInfo.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}

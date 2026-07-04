using System.Drawing;
namespace Sasd.Notes.App.WinForms.Dialogs;

partial class NewNoteDialog
{
    private System.ComponentModel.IContainer? components = null;
    private Label labelTargetFolder;
    private TextBox textBoxTargetFolder;
    private Label labelNoteTitle;
    private TextBox textBoxNoteTitle;
    private Button buttonOk;
    private Button buttonCancel;

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
    /// Initialisiert alle Steuerelemente des Dialogs.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        labelTargetFolder = new Label();
        textBoxTargetFolder = new TextBox();
        labelNoteTitle = new Label();
        textBoxNoteTitle = new TextBox();
        buttonOk = new Button();
        buttonCancel = new Button();
        SuspendLayout();
        // 
        // labelTargetFolder
        // 
        labelTargetFolder.AutoSize = true;
        labelTargetFolder.Location = new Point(12, 15);
        labelTargetFolder.Name = "labelTargetFolder";
        labelTargetFolder.Size = new Size(89, 15);
        labelTargetFolder.TabIndex = 0;
        labelTargetFolder.Text = "Zielordner/Vault";
        // 
        // textBoxTargetFolder
        // 
        textBoxTargetFolder.Location = new Point(12, 33);
        textBoxTargetFolder.Name = "textBoxTargetFolder";
        textBoxTargetFolder.ReadOnly = true;
        textBoxTargetFolder.Size = new Size(460, 23);
        textBoxTargetFolder.TabIndex = 1;
        // 
        // labelNoteTitle
        // 
        labelNoteTitle.AutoSize = true;
        labelNoteTitle.Location = new Point(12, 71);
        labelNoteTitle.Name = "labelNoteTitle";
        labelNoteTitle.Size = new Size(77, 15);
        labelNoteTitle.TabIndex = 2;
        labelNoteTitle.Text = "Titel der Notiz";
        // 
        // textBoxNoteTitle
        // 
        textBoxNoteTitle.Location = new Point(12, 89);
        textBoxNoteTitle.Name = "textBoxNoteTitle";
        textBoxNoteTitle.Size = new Size(460, 23);
        textBoxNoteTitle.TabIndex = 3;
        // 
        // buttonOk
        // 
        buttonOk.Location = new Point(316, 130);
        buttonOk.Name = "buttonOk";
        buttonOk.Size = new Size(75, 27);
        buttonOk.TabIndex = 4;
        buttonOk.Text = "OK";
        buttonOk.UseVisualStyleBackColor = true;
        buttonOk.Click += buttonOk_Click;
        // 
        // buttonCancel
        // 
        buttonCancel.Location = new Point(397, 130);
        buttonCancel.Name = "buttonCancel";
        buttonCancel.Size = new Size(75, 27);
        buttonCancel.TabIndex = 5;
        buttonCancel.Text = "Abbrechen";
        buttonCancel.UseVisualStyleBackColor = true;
        buttonCancel.Click += buttonCancel_Click;
        // 
        // NewNoteDialog
        // 
        AcceptButton = buttonOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = buttonCancel;
        ClientSize = new Size(484, 171);
        Controls.Add(buttonCancel);
        Controls.Add(buttonOk);
        Controls.Add(textBoxNoteTitle);
        Controls.Add(labelNoteTitle);
        Controls.Add(textBoxTargetFolder);
        Controls.Add(labelTargetFolder);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "NewNoteDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Neue Notiz";
        ResumeLayout(false);
        PerformLayout();
    }
}

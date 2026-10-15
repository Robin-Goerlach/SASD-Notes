using System.Drawing;
using System.Windows.Forms;

namespace Sasd.Notes.App.WinForms.Dialogs;

/// <summary>
/// Fragt den Namen eines neuen Vault-Unterordners ab.
/// </summary>
public sealed class NewFolderDialog : Form
{
    private readonly TextBox _folderNameTextBox;

    /// <summary>
    /// Initialisiert einen neuen Ordnerdialog.
    /// </summary>
    public NewFolderDialog()
    {
        Text = "Neuen Ordner anlegen";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(380, 125);

        var label = new Label
        {
            AutoSize = true,
            Location = new Point(12, 15),
            Text = "Ordnername:"
        };

        _folderNameTextBox = new TextBox
        {
            Location = new Point(12, 38),
            Width = 356
        };

        var createButton = new Button
        {
            DialogResult = DialogResult.OK,
            Location = new Point(212, 78),
            Size = new Size(75, 28),
            Text = "Anlegen"
        };

        var cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Location = new Point(293, 78),
            Size = new Size(75, 28),
            Text = "Abbrechen"
        };

        AcceptButton = createButton;
        CancelButton = cancelButton;
        Controls.AddRange(new Control[] { label, _folderNameTextBox, createButton, cancelButton });
    }

    /// <summary>
    /// Name des vom Benutzer eingegebenen Ordners.
    /// </summary>
    public string FolderName => _folderNameTextBox.Text.Trim();
}

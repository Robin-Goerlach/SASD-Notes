namespace Sasd.Notes.App.WinForms.Dialogs;

/// <summary>
/// Dialog zum Erstellen einer neuen Markdown-Notiz.
/// </summary>
public partial class NewNoteDialog : Form
{
    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="NewNoteDialog"/>-Klasse.
    /// </summary>
    /// <param name="relativeFolderPath">Zielordner innerhalb des aktuellen Vaults.</param>
    public NewNoteDialog(string? relativeFolderPath)
    {
        InitializeComponent();
        textBoxTargetFolder.Text = string.IsNullOrWhiteSpace(relativeFolderPath) ? "/" : relativeFolderPath;
    }

    /// <summary>
    /// Eingegebener Titel der neuen Notiz.
    /// </summary>
    public string NoteTitle => textBoxNoteTitle.Text.Trim();

    /// <summary>
    /// Bestätigt den Dialog, sofern eine sinnvolle Eingabe vorliegt.
    /// </summary>
    private void buttonOk_Click(object? sender, EventArgs e)
    {
        // Ohne Titel soll keine leere Notiz angelegt werden.
        if (string.IsNullOrWhiteSpace(NoteTitle))
        {
            MessageBox.Show(this, "Bitte geben Sie einen Titel für die neue Notiz ein.", "SASD Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            textBoxNoteTitle.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>
    /// Bricht den Dialog ab.
    /// </summary>
    private void buttonCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}

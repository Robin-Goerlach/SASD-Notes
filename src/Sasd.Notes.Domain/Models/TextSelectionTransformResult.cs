namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Enthält das Ergebnis einer Texttransformation im Editor.
/// </summary>
public sealed class TextSelectionTransformResult
{
    /// <summary>
    /// Gesamter neuer Dokumenttext.
    /// </summary>
    public required string UpdatedText { get; init; }

    /// <summary>
    /// Neue Cursor- oder Startposition der Auswahl.
    /// </summary>
    public int SelectionStart { get; init; }

    /// <summary>
    /// Neue Auswahl-Länge.
    /// </summary>
    public int SelectionLength { get; init; }
}

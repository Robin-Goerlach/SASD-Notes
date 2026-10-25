namespace Sasd.Notes.Domain.Models;

/// <summary>
/// Beschreibt einen ausgehenden Wiki-Link und sein optional aufgelöstes Ziel.
/// </summary>
public sealed class OutgoingLinkInfo
{
    /// <summary>
    /// Zielangabe aus der Wiki-Link-Syntax, ohne Klammern und Alias.
    /// </summary>
    public required string Target { get; init; }

    /// <summary>
    /// Sichtbarer Linktext; bei vorhandenem Alias ist dies der Alias.
    /// </summary>
    public required string DisplayText { get; init; }

    /// <summary>
    /// Ursprüngliche Wiki-Link-Syntax einschließlich Klammern.
    /// </summary>
    public required string RawText { get; init; }

    /// <summary>
    /// Relativer Pfad der eindeutig aufgelösten Zielnotiz oder <see langword="null"/>.
    /// </summary>
    public string? ResolvedRelativePath { get; init; }

    /// <summary>
    /// Gibt an, ob der Link eindeutig auf eine geladene Notiz aufgelöst wurde.
    /// </summary>
    public bool IsResolved => ResolvedRelativePath is not null;
}

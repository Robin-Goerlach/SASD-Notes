namespace Sasd.Notes.Application.Services;

/// <summary>
/// Verwaltet den Vorwärts- und Rückwärtsverlauf der eigenständigen Rechercheansicht.
/// </summary>
/// <remarks>
/// Der Dienst kennt ausschließlich logische relative Notizpfade. Dadurch bleibt er
/// unabhängig von Windows Forms, einem konkreten Renderer und dem Dateisystem.
/// </remarks>
public sealed class ResearchNavigationService
{
    private readonly List<string> _backHistory = new();
    private readonly List<string> _forwardHistory = new();

    /// <summary>
    /// Ruft den relativen Pfad der aktuell angezeigten Notiz ab.
    /// </summary>
    public string? CurrentRelativePath { get; private set; }

    /// <summary>
    /// Ruft ab, ob ein vorheriger Rechercheeintrag vorhanden ist.
    /// </summary>
    public bool CanGoBack => _backHistory.Count > 0;

    /// <summary>
    /// Ruft ab, ob ein nachfolgender Rechercheeintrag vorhanden ist.
    /// </summary>
    public bool CanGoForward => _forwardHistory.Count > 0;

    /// <summary>
    /// Nimmt eine Notiz in den Verlauf auf und verwirft einen nicht mehr erreichbaren Vorwärtszweig.
    /// </summary>
    /// <param name="relativePath">Logischer relativer Pfad der angezeigten Notiz.</param>
    public void NavigateTo(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        // Ein erneuter Klick auf dieselbe Notiz soll keinen künstlichen Verlaufsschritt erzeugen.
        if (string.Equals(CurrentRelativePath, relativePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (CurrentRelativePath is not null)
        {
            _backHistory.Add(CurrentRelativePath);
        }

        CurrentRelativePath = relativePath;
        _forwardHistory.Clear();
    }

    /// <summary>
    /// Geht zum vorherigen Rechercheeintrag zurück.
    /// </summary>
    /// <returns>Relativer Zielpfad oder <see langword="null"/>, wenn kein Eintrag vorhanden ist.</returns>
    public string? GoBack()
    {
        if (!CanGoBack || CurrentRelativePath is null)
        {
            return null;
        }

        string previousPath = _backHistory[^1];
        _backHistory.RemoveAt(_backHistory.Count - 1);
        _forwardHistory.Add(CurrentRelativePath);
        CurrentRelativePath = previousPath;
        return previousPath;
    }

    /// <summary>
    /// Geht zum nächsten Rechercheeintrag vor.
    /// </summary>
    /// <returns>Relativer Zielpfad oder <see langword="null"/>, wenn kein Eintrag vorhanden ist.</returns>
    public string? GoForward()
    {
        if (!CanGoForward || CurrentRelativePath is null)
        {
            return null;
        }

        string nextPath = _forwardHistory[^1];
        _forwardHistory.RemoveAt(_forwardHistory.Count - 1);
        _backHistory.Add(CurrentRelativePath);
        CurrentRelativePath = nextPath;
        return nextPath;
    }

    /// <summary>
    /// Löscht den Verlauf, beispielsweise nach einem Vault-Wechsel.
    /// </summary>
    /// <param name="initialRelativePath">Optionaler erster Eintrag des neuen Vaults.</param>
    public void Reset(string? initialRelativePath = null)
    {
        _backHistory.Clear();
        _forwardHistory.Clear();
        CurrentRelativePath = string.IsNullOrWhiteSpace(initialRelativePath)
            ? null
            : initialRelativePath;
    }
}

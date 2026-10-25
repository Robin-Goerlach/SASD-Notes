using Sasd.Notes.Application.Services;

namespace Sasd.Notes.Tests;

/// <summary>
/// Prüft den UI-unabhängigen Vorwärts- und Rückwärtsverlauf der Rechercheansicht.
/// </summary>
internal static class ResearchNavigationTests
{
    /// <summary>
    /// Führt die deterministischen Navigationsprüfungen aus.
    /// </summary>
    public static void Run()
    {
        var navigation = new ResearchNavigationService();
        AssertEx.True(navigation.CurrentRelativePath is null,
            "Ein neuer Rechercheverlauf darf keine aktuelle Notiz besitzen.");
        AssertEx.False(navigation.CanGoBack,
            "Ein neuer Rechercheverlauf darf keinen Rückwärtsschritt anbieten.");

        navigation.NavigateTo("Inbox/Alpha.md");
        navigation.NavigateTo("Projekte/Beta.md");
        navigation.NavigateTo("Wissen/Gamma.md");

        AssertEx.Equal("Projekte/Beta.md", navigation.GoBack(),
            "Der erste Rückwärtsschritt muss zur unmittelbar vorherigen Notiz führen.");
        AssertEx.Equal("Inbox/Alpha.md", navigation.GoBack(),
            "Mehrere Rückwärtsschritte müssen ihre ursprüngliche Reihenfolge erhalten.");
        AssertEx.False(navigation.CanGoBack,
            "Am Anfang des Verlaufs darf kein weiterer Rückwärtsschritt möglich sein.");

        AssertEx.Equal("Projekte/Beta.md", navigation.GoForward(),
            "Vorwärtsnavigation muss den zuletzt verlassenen Eintrag wiederherstellen.");
        navigation.NavigateTo("Archiv/Delta.md");
        AssertEx.False(navigation.CanGoForward,
            "Eine neue Navigation nach einem Rückwärtsschritt muss den alten Vorwärtszweig verwerfen.");

        navigation.NavigateTo("archiv/delta.md");
        AssertEx.True(navigation.CanGoBack,
            "Ein nur in der Groß-/Kleinschreibung abweichender gleicher Pfad darf keinen zusätzlichen Schritt erzeugen.");
        AssertEx.Equal("Projekte/Beta.md", navigation.GoBack(),
            "Die doppelte Navigation darf den vorherigen Verlaufseintrag nicht verdrängen.");
        AssertEx.Equal("Inbox/Alpha.md", navigation.GoBack(),
            "Der ältere Verlaufseintrag muss nach dem übersprungenen Duplikat weiterhin erreichbar sein.");

        navigation.Reset("Start.md");
        AssertEx.Equal("Start.md", navigation.CurrentRelativePath,
            "Reset darf einen ausdrücklich angegebenen Starteintrag übernehmen.");
        AssertEx.False(navigation.CanGoBack,
            "Reset muss den alten Rückwärtsverlauf vollständig löschen.");
        AssertEx.False(navigation.CanGoForward,
            "Reset muss den alten Vorwärtsverlauf vollständig löschen.");
    }
}

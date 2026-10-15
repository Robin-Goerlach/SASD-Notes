# V1-Release-Checkliste

Diese Checkliste beschreibt die technische Prüfung eines SASD-Notes-V1-Stands.

## Automatisierte Prüfung

Der Windows-Workflow muss für den zu veröffentlichenden Commit erfolgreich sein:

- .NET-8-SDK wird eingerichtet.
- Die Solution wird wiederherstellbar (dotnet restore).
- Die Solution wird im Release-Modus kompiliert.
- dotnet test läuft erfolgreich.
- Der paketfreie Test-Harness läuft erfolgreich.
- Das WinForms-Projekt lässt sich als Release veröffentlichen.
- Es werden keine Merge-Konfliktmarker in Projekt-, Test- oder Dokumentationsdateien gefunden.

## Lokale Prüfung unter Windows

```powershell
dotnet clean
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project .\tests\Sasd.Notes.Tests\Sasd.Notes.Tests.csproj --configuration Release
dotnet publish .\src\Sasd.Notes.App.WinForms\Sasd.Notes.App.WinForms.csproj --configuration Release --self-contained false
```

## Manuelle Smoke-Prüfung

- Die Anwendung startet ohne geöffneten Vault.
- Ein Vault mit Unterordnern und leeren Ordnern lässt sich öffnen.
- Markdown-Dateien werden rekursiv im TreeView angezeigt.
- Eine Notiz lässt sich öffnen, ändern und speichern.
- Der Dirty-State verhindert unbeabsichtigtes Verwerfen.
- Eine neue Notiz lässt sich im gewählten Ordner anlegen.
- Ein neuer Ordner lässt sich anlegen und bleibt auch leer sichtbar.
- Wiki-Links, Backlinks, Suche und Outline reagieren auf den aktuellen Inhalt.
- Eine extern geänderte Notiz wird gemeldet und kann kontrolliert neu geladen werden.
- Reservierte Windows-Namen werden bei neuen Dateien und Ordnern sicher behandelt.

## Bekannte V1-Grenzen

Diese Funktionen sind bewusst später vorgesehen:

- Markdown-Preview und Live-Preview
- FileSystemWatcher-basierte automatische Aktualisierung
- Tabs und Split-View
- Rename mit automatischer Link-Aktualisierung
- Frontmatter-/Properties-Editor
- Plugin-System
- Graph View

Die Grenzen ändern nichts am zentralen V1-Prinzip: Markdown-Dateien bleiben die primäre und portable Datenquelle.

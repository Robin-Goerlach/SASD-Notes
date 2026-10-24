# V1-Release-Checkliste

Diese Checkliste beschreibt die technische Prüfung eines SASD-Notes-V1-Stands.

## Automatisierte Prüfung

Der Windows-Workflow muss für den zu veröffentlichenden Commit erfolgreich sein:

- .NET-8-SDK wird eingerichtet.
- Die Solution wird wiederherstellbar (dotnet restore).
- Die Solution wird im Release-Modus kompiliert.
- dotnet test läuft erfolgreich.
- Der paketfreie Test-Harness läuft erfolgreich.
- Der Windows-Test-Harness prüft verzögerte Schließbestätigung, Abbruch und erneuten Schließversuch.
- Der Windows-Test-Harness prüft Bedienungssperre und abgefangene Schließversuche während eines verzögerten Vault-Ladevorgangs.
- Startargumente werden ohne UI getestet; der Windows-Harness prüft Standard-, Ordner- und Dateistart mit Editor- und Baumauswahl sowie verzögerter Einstellungen-Initialisierung.
- Das WinForms-Projekt lässt sich als Release veröffentlichen.
- Die veröffentlichte WinForms-Ausgabe wird als ZIP-CI-Artefakt bereitgestellt.
- Es werden keine Merge-Konfliktmarker in Projekt-, Test- oder Dokumentationsdateien gefunden.

## Lokale Prüfung unter Windows

```powershell
dotnet clean
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project ./tests/Sasd.Notes.WinForms.Tests/Sasd.Notes.WinForms.Tests.csproj --configuration Release
dotnet run --project .\tests\Sasd.Notes.Tests\Sasd.Notes.Tests.csproj --configuration Release
dotnet publish .\src\Sasd.Notes.App.WinForms\Sasd.Notes.App.WinForms.csproj --configuration Release --self-contained false
```

## Manuelle Smoke-Prüfung

- Die Anwendung startet ohne geöffneten Vault.
- Ohne Argument bleibt das bisherige Verhalten mit zuletzt verwendetem Vault erhalten.
- Ein expliziter Ordnerpfad übersteuert den gemerkten Vault.
- Eine explizite Markdown-Datei öffnet deren Elternordner als Vault und die richtige Datei im Editor.
- Pfade mit Leerzeichen und Umlauten funktionieren in Anführungszeichen, auch relativ zum Arbeitsverzeichnis.
- Ein fehlendes Ziel oder eine .txt-Datei zeigt einen Fehler; kein Ersatz-Vault und keine neue Datei werden geöffnet bzw. erzeugt.
- Ein Vault mit Unterordnern und leeren Ordnern lässt sich öffnen.
- Markdown-Dateien werden rekursiv im TreeView angezeigt.
- Eine Notiz lässt sich öffnen, ändern und speichern.
- Der Dirty-State verhindert unbeabsichtigtes Verwerfen.
- Während eines Vault-Wechsels sind Editor und weitere Befehle gesperrt; nach Ladefehlern und nach erfolgreichem Abschluss ist die Bedienung wieder möglich.
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

## Gelegentliche lokale Windows-Prüfung

Nach größeren Änderungen sowie nach Änderungen am Programmstart, an Dialogen,
am Editor oder am Speichern die oben aufgeführten Build- und Harness-Befehle
auch lokal ausführen. Zusätzlich einen kurzen Bedienungstest mit einem
Test-Vault durchführen. Die Windows-CI ersetzt diesen manuellen Test auf dem
eigenen Rechner nicht.

Für die Startargumente im Repository-Root nach einem Release-Build:

```powershell
dotnet run --project .\src\Sasd.Notes.App.WinForms\Sasd.Notes.App.WinForms.csproj --configuration Release --no-build -- ".\sample-vault"
dotnet run --project .\src\Sasd.Notes.App.WinForms\Sasd.Notes.App.WinForms.csproj --configuration Release --no-build -- ".\sample-vault\01_Projekte\Projektplan.md"
```

# Technisches Design – SASD Notes (Windows Forms)

Version: 0.1  
Datum: 2026-07-04  
Status: Entwurfsfassung  
Autor: ChatGPT für SASD / OpenAI

---

## 1. Dokumentzweck

Dieses Dokument beschreibt das technische Zielbild für **SASD Notes V1** als Windows-Forms-Anwendung auf Basis von **.NET 8**.  
Es konkretisiert das Pflichtenheft in Richtung Moduldesign, Datenfluss, UI-Struktur, Verantwortlichkeiten und technischer Entscheidungen.

Das Dokument beantwortet insbesondere:

- welche fachlichen Module die Anwendung benötigt,
- wie diese Module logisch voneinander getrennt werden,
- wie der Datenfluss zwischen UI, Anwendungsschicht und Dateisystem erfolgen soll,
- welche Kernobjekte und Services vorgesehen sind,
- welche Erweiterungspunkte bereits für spätere Versionen vorbereitet werden.

---

## 2. Architekturprinzipien

### 2.1 Schichtenmodell

Die Anwendung wird in klar getrennte Schichten aufgeteilt:

- **Sasd.Notes.App.WinForms**
  - Windows-Forms-Oberfläche
  - Menüs, Commands, Dialoge, Fenster, Event-Anbindung
  - Darstellung von TreeView, Editor, Suchtreffern, Backlinks, Statusleiste

- **Sasd.Notes.Application**
  - Anwendungslogik
  - Use Cases / Services für Öffnen, Speichern, Suchen, Link-Auflösung, Navigation, Recent Folders
  - Koordination zwischen UI und Fachlogik

- **Sasd.Notes.Domain**
  - Fachmodelle
  - Regeln rund um Notizen, Vaults, Links, Tags, Suche, Backlinks
  - Parser-nahe Objekte und Validierungslogik ohne UI-Bezug

- **Sasd.Notes.Infrastructure**
  - Dateisystemzugriff
  - Konfigurationsspeicherung
  - Dateisystemüberwachung
  - optional spätere Indizes oder Cache-Speicher

- **Sasd.Notes.Tests**
  - Unit-Tests
  - Integrationsnahe Tests
  - Parser- und Suchtests

### 2.2 Leitprinzipien

1. **Dateien sind die primäre Wahrheit.**  
   Markdown-Dateien im Dateisystem bleiben der führende Speicher.

2. **UI bleibt dünn.**  
   Fachlogik darf nicht in Event-Handlern des Forms-Codes verteilt werden.

3. **Kleine, testbare Services.**  
   Parser, Resolver, Suche und Vault-Verwaltung werden getrennt testbar umgesetzt.

4. **Saubere Erweiterungspunkte.**  
   Plugin-System, Preview-Ausbau, alternative UI und Suchoptimierungen werden vorbereitet, aber nicht vorweg implementiert.

5. **Lesbarkeit vor Cleverness.**  
   Der Code soll nachvollziehbar, kommentiert und wartbar bleiben.

---

## 3. Zielstruktur des Repositorys

```text
SASD-Notes/
├─ README.md
├─ .gitignore
├─ global.json
├─ docs/
│  ├─ ...
├─ src/
│  ├─ Sasd.Notes.App.WinForms/
│  ├─ Sasd.Notes.Application/
│  ├─ Sasd.Notes.Domain/
│  └─ Sasd.Notes.Infrastructure/
└─ tests/
   └─ Sasd.Notes.Tests/
```

---

## 4. Hauptmodule

### 4.1 Vault Management

Verantwortung:

- Vault öffnen
- Vault schließen
- Vault validieren
- zuletzt verwendete Vaults verwalten
- Scan initialisieren
- Dateiänderungen beobachten

Wesentliche Services:

- `VaultService`
- `RecentFoldersService`
- `VaultScanner`
- `FileSystemWatcherAdapter`

### 4.2 Note Management

Verantwortung:

- Notizen laden
- neue Notizen anlegen
- speichern
- umbenennen
- löschen
- Pfadänderungen verarbeiten

Wesentliche Services:

- `NoteService`
- `FileNameSanitizer`
- `RenameCoordinator`

### 4.3 Markdown Editing Support

Verantwortung:

- Editortext laden/speichern
- Formatierungshilfen anwenden
- Überschriften, Fett, Kursiv, Code, Listen einfügen
- Wiki-Link-Einfügung vereinfachen
- Dirty-State verwalten

Wesentliche Services:

- `EditorFormattingService`
- `SelectionTextService`
- `DirtyStateTracker`

### 4.4 Linking & Backlinks

Verantwortung:

- Wiki-Links parsen
- Linkziele auflösen
- fehlende Ziele erkennen
- Backlinks berechnen
- Mehrdeutigkeiten markieren

Wesentliche Services:

- `WikiLinkParser`
- `LinkResolver`
- `BacklinkService`

### 4.5 Search

Verantwortung:

- Suche nach Dateinamen
- Suche im Inhalt
- Filter nach Ordner / Tags / Metadaten
- Trefferkontext erzeugen
- Ranking für einfache Relevanz

Wesentliche Services:

- `SearchService`
- `SearchSnippetBuilder`
- optional später `SearchIndexService`

### 4.6 Outline & Metadata

Verantwortung:

- Gliederung aus Überschriften erzeugen
- Frontmatter / Properties lesen
- Tags extrahieren
- Dokumentinformationen anzeigen

Wesentliche Services:

- `OutlineService`
- `FrontmatterParser`
- `TagExtractionService`

---

## 5. Fachmodelle

### 5.1 VaultInfo

Enthält:

- Name
- RootPath
- LastOpenedUtc
- optional Einstellungen

### 5.2 NoteDocument

Enthält:

- Id
- Title
- FullPath
- RelativePath
- Content
- LastModifiedUtc
- erkannte Wiki-Links
- Properties
- Tags

### 5.3 WikiLink

Enthält:

- RawText
- Target
- Alias
- StartIndex
- Length

### 5.4 SearchHit

Enthält:

- NoteTitle
- RelativePath
- PreviewText
- Score
- Trefferart

### 5.5 BacklinkInfo

Enthält:

- SourceTitle
- SourceRelativePath
- ContextSnippet

---

## 6. UI-Design für WinForms

### 6.1 Hauptfenster

Das Hauptfenster wird in folgende Zonen gegliedert:

1. **MenuStrip**
   - Datei
   - Bearbeiten
   - Ansicht
   - Gehe zu
   - Werkzeuge
   - Hilfe

2. **ToolStrip**
   - Vault öffnen
   - Neue Notiz
   - Speichern
   - Formatierung
   - Suche
   - Navigation zurück/vor

3. **SplitContainer links/rechts**
   - links Navigationsbereich
   - rechts Arbeitsbereich

4. **Linker Navigationsbereich**
   - optional ComboBox für Vault-Auswahl
   - TreeView für Ordner und Markdown-Dateien

5. **Rechter Arbeitsbereich**
   - oberer Bereich: Editor oder TabControl
   - rechter Zusatzbereich: Suche / Backlinks / Gliederung / Eigenschaften

6. **StatusStrip**
   - aktueller Vault
   - Änderungsstatus
   - Anzahl Backlinks
   - Wort-/Zeichenzahl
   - ggf. Warnhinweise

### 6.2 UI-Steuerelemente

Geplante Controls:

- `MenuStrip`
- `ToolStrip`
- `StatusStrip`
- `SplitContainer`
- `TreeView`
- `RichTextBox` oder `TextBox` für V1-Editor
- `TabControl`
- `ListView` oder `ListBox`
- `PropertyGrid` optional später
- Standarddialoge: `FolderBrowserDialog`, `OpenFileDialog`, `SaveFileDialog`

### 6.3 Stilprinzipien

- klassische Windows-Desktop-Bedienung
- klare Trennung zwischen Navigation, Bearbeitung und Kontext
- keine überladene Oberfläche in V1
- funktional vor dekorativ
- spätere Theme- oder Dark-Mode-Unterstützung vorbereitbar

---

## 7. Datenfluss

### 7.1 Vault öffnen

1. Benutzer wählt einen Ordner.
2. UI ruft `VaultService.OpenVault(...)` auf.
3. `VaultScanner` lädt alle Markdown-Dateien.
4. `Infrastructure` liest Dateien aus dem Dateisystem.
5. `Domain`-Objekte werden aufgebaut.
6. TreeView wird mit dem Ergebnis befüllt.
7. Recent-Folders-Liste wird aktualisiert.

### 7.2 Notiz öffnen

1. Benutzer klickt auf TreeView-Eintrag.
2. UI fordert die Notiz über `NoteService` an.
3. Inhalt wird in den Editor geladen.
4. Parser berechnet:
   - Wiki-Links
   - Gliederung
   - Tags
   - Properties
5. Zusatzpaneele werden aktualisiert.

### 7.3 Speichern

1. Benutzer klickt Speichern oder Auto-Save wird ausgelöst.
2. `DirtyStateTracker` prüft Änderungen.
3. `NoteService.Save(...)` validiert den Speicherzustand.
4. Datei wird im Dateisystem überschrieben.
5. Änderungsstatus und Zeitstempel werden aktualisiert.

### 7.4 Link-Navigation

1. Benutzer klickt im Editor oder Kontext auf Wiki-Link.
2. `LinkResolver` versucht Zielnotiz zu finden.
3. Bei Erfolg wird Zielnotiz geladen.
4. Bei Fehlen kann der Benutzer:
   - neue Notiz erzeugen,
   - Suche öffnen,
   - Link unbearbeitet lassen.

---

## 8. Technische Einzelentscheidungen

### 8.1 Editor in V1

Für V1 wird ein einfacher, robuster Texteditor verwendet.  
Keine vollständige Rich-Markdown-WYSIWYG-Oberfläche in der ersten Version.

Formatierungshilfen arbeiten textbasiert:

- markierten Text mit `**...**`
- `_..._`
- `` `...` ``
- `[[...]]`
- `#`, `##`, `###`
- Listenpräfixe

### 8.2 Linksyntax

Unterstützung in V1:

- `[[Notiz]]`
- `[[Ordner/Notiz]]`
- `[[Notiz|Alias]]`

Nicht zwingend in V1:

- Abschnittsanker
- Blockreferenzen
- Transclusion

### 8.3 Frontmatter

YAML-Frontmatter wird in V1 lesend vorbereitet, jedoch nicht vollumfänglich ausgebaut.  
Properties können zunächst in einem einfachen Parser erfasst und angezeigt werden.

### 8.4 Suche

V1-Suche arbeitet direkt auf geladenen Notizen:

- case-insensitive
- Dateiname + Inhalt
- einfache Trefferbewertung
- Trefferkontext mit kurzem Ausschnitt

Spätere Ausbaustufen:

- invertierter Index
- SQLite-FTS oder eigener Cache
- Filterlogik
- fuzzy matching

### 8.5 Konfiguration

Lokale Anwendungseinstellungen werden in einer einfachen JSON-Datei abgelegt, z. B.:

- letzte Fenstergröße
- letzter Vault
- Recent Folders
- Editoroptionen
- optionale Feature-Flags

---

## 9. Fehlerbehandlung

Die Anwendung soll erwartbare Fehlerfälle kontrolliert behandeln:

- nicht lesbarer Vault
- fehlende Datei
- Dateisperre
- Zugriff verweigert
- ungültiger Dateiname
- doppelte Zielnotiz
- ungültige Zeichen im Pfad
- externe Dateiänderungen

Fehlerbehandlung erfolgt mit:

- Dialog für benutzerrelevante Fehler
- Statusleistenhinweis für weiche Warnungen
- optionalem lokalem Logfile für Diagnosen

---

## 10. Erweiterungspunkte

### 10.1 Plugin-Vorbereitung

Bereits in V1 wird auf lose Kopplung geachtet.  
Spätere Plugins sollen voraussichtlich an folgenden Punkten andocken können:

- Menü- und Toolbar-Erweiterungen
- Notiz-Post-Processing
- Exporter
- Suchprovider
- Syntax-/Parser-Erweiterungen
- Metadaten- oder Preview-Erweiterungen

### 10.2 Spätere Funktionsblöcke

- Markdown-Preview
- Tabs für mehrere Notizen
- Diagramm-/Graphansicht
- eingebettete Assets
- Drag & Drop
- Export nach HTML/PDF
- Community-Plugin-System
- Theme- / Dark-Mode-Engine

---

## 11. Nicht-funktionale Qualitätsziele

- **Wartbarkeit:** klare Modulgrenzen, gute Kommentare, nachvollziehbare Klassen
- **Testbarkeit:** Fachlogik ohne UI testbar
- **Robustheit:** keine Datenbankabhängigkeit für Kernfunktionen
- **Performance:** normale Vaults mit mehreren hundert Dateien flüssig nutzbar
- **Portabilität der Inhalte:** offene Markdown-Dateien
- **Nachvollziehbarkeit:** verständliche Dokumentation und strukturierter Code

---

## 12. Implementierungsreihenfolge

### Phase 1 – Kern
- Repository-Struktur
- Domain-Modelle
- Vault öffnen
- TreeView
- Notiz laden/speichern
- Editor
- Recent Folders

### Phase 2 – Wissensfunktionen
- WikiLinkParser
- LinkResolver
- Backlinks
- Suche
- Gliederung

### Phase 3 – Komfort
- neue Notiz
- Ordner anlegen
- Umbenennen
- Formatierungshilfen
- Statusanzeigen
- Dateisystemüberwachung

### Phase 4 – Ausbau
- Preview
- Tabs
- Frontmatter-Ansicht
- Export
- Plugin-Vorbereitung konkretisieren

---

## 13. Ergebnis

Dieses technische Design bildet die Brücke zwischen Lastenheft, Pflichtenheft und späterer Implementierung.  
Es definiert ein **realistisches, gut erklärbares und testbares Zielsystem**, das mit .NET 8 und Windows Forms sauber umgesetzt werden kann und zugleich offen genug für spätere Erweiterungen bleibt.

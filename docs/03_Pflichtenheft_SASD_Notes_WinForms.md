# Pflichtenheft – SASD Notes (Windows Forms)

Version: 1.0  
Datum: 2026-07-04  
Status: Arbeitsfassung / Umsetzungsgrundlage  
Autor: OpenAI / ChatGPT im Auftrag von SASD

---

## 1. Dokumentzweck

Dieses Pflichtenheft beschreibt die technische Umsetzung der ersten Windows-Forms-Version von **SASD Notes**. Grundlage ist das bereits erstellte Lastenheft für SASD Notes sowie die zuvor festgelegten Projektziele:

- lokale Verwaltung von Markdown-Notizen
- interne Verlinkung zwischen Markdown-Dateien
- Editor-artige Bearbeitung mit Markdown-Formatierungshilfen
- Verwaltung verschiedener Themenverzeichnisse bzw. Arbeitsordner/Vaults
- Suchfunktion über Dateinamen und Inhalte
- technische Vorbereitung für spätere Erweiterbarkeit, insbesondere ein mögliches Plugin-System in späteren Versionen

Das Dokument beschreibt **wie** die Anforderungen umgesetzt werden sollen. Es dient als konkrete technische Arbeitsgrundlage für Entwicklung, Tests, Dokumentation und spätere Erweiterungen.

---

## 2. Ausgangslage und Rahmenbedingungen

### 2.1 Technischer Rahmen

Die Anwendung soll als **Windows-Desktop-Anwendung mit Windows Forms** umgesetzt werden. Zielplattform ist Windows mit **.NET 8** als technischer Basis, da auf dem Zielrechner das .NET-8-SDK verfügbar ist. Die Entwicklungsumgebung ist Visual Studio 2022.

### 2.2 Projektcharakter

SASD Notes ist als **lokales, dateibasiertes Wissens- und Notizsystem** geplant. Die Anwendung soll nicht von Cloud-Diensten abhängen. Die Markdown-Dateien bleiben die primäre Datenbasis. Die Anwendung verwaltet und verarbeitet diese Dateien, ohne sie in ein proprietäres Primärformat umzuwandeln.

### 2.3 Abgrenzung der ersten Version

Die erste Version ist bewusst funktional fokussiert. Nicht Bestandteil von V1 sind unter anderem:

- Plugin-Markt oder Plugin-Verwaltung
- Synchronisation mit Cloud-Diensten
- Publishing ins Web
- Canvas-/Whiteboard-Funktion
- Mobile Clients
- Mehrbenutzerbetrieb mit gleichzeitiger Bearbeitung
- komplexe Themen-/Theme-Engines
- vollständige Obsidian-Kompatibilität in allen Sonderfällen

Das Plugin-System wird in der Architektur jedoch bereits **mitgedacht**, so dass spätere Erweiterungspunkte vorbereitet werden.

---

## 3. Zielsystem – Kurzbeschreibung

SASD Notes V1 ist eine Windows-Forms-Anwendung mit klassischer Desktop-Bedienung.

Die Anwendung bietet:

- ein Hauptfenster mit Menüleiste, Werkzeugleiste, Statusleiste und mehrspaltiger Arbeitsoberfläche
- einen Navigationsbereich für Vaults, Ordner und Markdown-Dateien
- einen zentralen Editorbereich zur Bearbeitung von Markdown-Dateien
- einen Kontextbereich für Suche, Backlinks, Gliederung und Dateiinformationen
- Funktionen zum Öffnen, Erstellen, Speichern, Umbenennen und Verlinken von Markdown-Dateien
- Verwaltung mehrerer Arbeitsordner bzw. „Vaults“ und zusätzlich eine Liste der zuletzt verwendeten Ordner (**Recent Folders**)

---

## 4. Fachliche Grundentscheidungen

### 4.1 Dateisystem als Primärspeicher

Die primäre Datenhaltung erfolgt ausschließlich im Dateisystem:

- eine Notiz entspricht genau einer Markdown-Datei (`.md`)
- Ordner im Dateisystem entsprechen Themenbereichen, Sammlungen oder Unterstrukturen
- Assets wie Bilder oder Anhänge können später in separaten Unterordnern liegen
- die Anwendung darf keine Zwangsdatenbank als Primärspeicher einführen

### 4.2 Vault-/Ordner-Konzept

Die Anwendung unterstützt das Konzept eines **Arbeitsordners/Vaults**. Ein Vault ist ein Wurzelverzeichnis, in dem Notizen und Unterordner organisiert werden.

Für V1 gelten folgende Regeln:

- ein geöffneter Arbeitsordner ist immer genau ein aktiver Vault
- innerhalb eines Vaults können beliebig viele Unterordner existieren
- über das Menü können verschiedene Vaults geöffnet werden
- zusätzlich wird eine Liste der zuletzt verwendeten Ordner angeboten
- spätere Versionen können optional eine Arbeitsbereichsverwaltung mit mehreren parallel registrierten Vaults ergänzen

### 4.3 Markdown als Bearbeitungsformat

SASD Notes arbeitet intern mit Markdown-Text. Die Formatierung erfolgt nicht über ein proprietäres Rich-Text-Format, sondern durch Einfügen bzw. Bearbeiten von Markdown-Syntax.

Das bedeutet:

- der Editor zeigt den Rohtext bearbeitbar an
- Formatierungshilfen setzen Markdown-Syntax ein, zum Beispiel `**fett**`, `*kursiv*`, `` `Code` ``, `# Überschrift`
- interne Notizverweise werden mit Wiki-Link-Syntax wie `[[Notiz]]` oder `[[Ordner/Notiz]]` realisiert
- eine spätere Vorschau-/Preview-Funktion ist möglich, aber nicht zwingender Kern von V1

---

## 5. Systemkontext und Benutzerrollen

### 5.1 Benutzerrolle

Für V1 wird nur eine Rolle betrachtet:

- **lokaler Anwender** – erstellt, organisiert, sucht und bearbeitet eigene Markdown-Notizen

### 5.2 Externe Systeme

Direkt angebundene externe Systeme sind in V1 nicht vorgesehen. Interaktion findet primär mit folgenden Ressourcen statt:

- lokales Windows-Dateisystem
- Zwischenablage
- ggf. Dateiauswahldialoge und Standard-Windows-Dialoge

---

## 6. Gesamtarchitektur

### 6.1 Architekturstil

Obwohl die Anwendung auf Windows Forms basiert, soll die fachliche Logik **nicht** direkt in den Forms verdrahtet werden. Stattdessen wird eine saubere Schichtung vorgesehen.

Empfohlene Lösung:

```text
Sasd.Notes.sln
 ├─ Sasd.Notes.WinForms.App         // Windows-Forms-Oberfläche
 ├─ Sasd.Notes.Application          // Anwendungslogik / Use Cases / Services
 ├─ Sasd.Notes.Domain               // Fachmodelle und Regeln
 ├─ Sasd.Notes.Infrastructure       // Dateisystem, Konfiguration, Suchindex
 └─ Sasd.Notes.Tests                // Unit- und Integrationstests
```

### 6.2 Schichtenbeschreibung

#### Sasd.Notes.WinForms.App

Verantwortlich für:

- Forms, Dialoge, Controls und Layout
- Benutzerinteraktion
- Menü- und Toolbar-Events
- Ansteuerung der Use Cases aus der Application-Schicht
- Anzeige von Daten, Statusmeldungen und Fehlermeldungen

#### Sasd.Notes.Application

Verantwortlich für:

- Orchestrierung der Anwendungsfälle
- Laden und Speichern von Notizen
- Erstellen neuer Notizen
- Link-Auflösung
- Suchen, Backlinks, Gliederung
- Umbenennen und Aktualisieren referenzierender Links
- Aufruf von Infrastruktur-Komponenten

#### Sasd.Notes.Domain

Verantwortlich für:

- Domänenmodelle wie `Vault`, `NoteDocument`, `WikiLink`, `SearchHit`, `BacklinkInfo`
- Fachregeln, z. B. Link-Normalisierung, Dateinamensregeln, Parserregeln
- unabhängig von Windows Forms

#### Sasd.Notes.Infrastructure

Verantwortlich für:

- Lesen und Schreiben von Dateien
- Rekursives Einlesen von Vault-Inhalten
- Beobachtung von Dateisystemänderungen
- optionale Konfigurationsspeicherung
- optionaler lokaler Suchindex oder Cache in späteren Versionen

#### Sasd.Notes.Tests

Verantwortlich für:

- fachliche Unit-Tests
- Parser-Tests
- Link-Auflösungs-Tests
- Suchtests
- Tests für Dateinamens- und Umbenennungslogik

---

## 7. Projekt- und Codekonventionen

### 7.1 Programmiersprache und Stil

- Sprache: C#
- Ziel-Framework: .NET 8
- UI-Technologie: Windows Forms
- Nullable Reference Types: aktiviert
- implizite Usings: erlaubt, sofern die Lesbarkeit nicht leidet

### 7.2 Kommentarstandard

Der Quellcode soll verständlich dokumentiert werden. Es gelten folgende Regeln:

- Klassen, Interfaces, Properties und Methoden erhalten `///` XML-Kommentare
- innerhalb von Methoden werden erklärende `//` Kommentare verwendet
- Kommentare sollen Fachlogik, Absichten und wichtige Entscheidungen erklären
- triviale Selbstverständlichkeiten sollen nicht überkommentiert werden

### 7.3 Namenskonventionen

- PascalCase für Klassen, Methoden und Properties
- camelCase für lokale Variablen und Parameter
- sprechende Namen statt kryptischer Abkürzungen
- Fachbegriffe konsistent verwenden: Vault, Note, Link, Backlink, Search, Outline, Recent Folders

### 7.4 Fehlerbehandlung

- keine stillen Fehlerunterdrückungen ohne Protokollierung oder Rückmeldung
- Benutzerfehler und Dateifehler sollen sauber behandelt werden
- fachliche Validierungsfehler werden benutzerverständlich zurückgegeben
- technische Details können für spätere Protokollierung zusätzlich intern festgehalten werden

---

## 8. Benutzeroberfläche – Sollaufbau

## 8.1 Hauptfenster

Das Hauptfenster ist das zentrale Arbeitsfenster der Anwendung.

Es besteht aus:

- Titelleiste
- Menüleiste
- optionale Werkzeugleiste
- Hauptarbeitsbereich mit drei Zonen
- Statusleiste

### 8.2 Hauptlayout

Empfohlenes Grundlayout:

- **links**: Navigationsbereich (Vault, Ordner, Notizen)
- **mitte**: Editorbereich
- **rechts**: Kontextbereich (Backlinks, Gliederung, Suchtreffer, Eigenschaften)
- **unten**: Statusleiste

Das Layout soll in Windows Forms mit SplitContainern umgesetzt werden:

- äußerer `SplitContainer` für linke Navigation und rechten Arbeitsbereich
- zweiter `SplitContainer` für mittleren Editor und rechten Kontextbereich

### 8.3 Menüleiste

Die Menüleiste soll mindestens folgende Einträge enthalten:

#### Datei

- Neuer Arbeitsordner / Vault öffnen…
- Zuletzt verwendete Ordner (Recent Folders)
- Neue Notiz
- Neuer Unterordner
- Speichern
- Speichern unter… (optional für spätere Version)
- Notiz umbenennen
- Notiz löschen
- Ordner im Explorer öffnen
- Beenden

#### Bearbeiten

- Rückgängig
- Wiederholen
- Ausschneiden
- Kopieren
- Einfügen
- Suchen
- Ersetzen (optional V1.1)

#### Format

- Überschrift 1
- Überschrift 2
- Überschrift 3
- Fett
- Kursiv
- Durchgestrichen
- Code Inline
- Codeblock
- Aufzählungsliste
- Nummerierte Liste
- Checkbox-Liste
- Link zu Notiz einfügen

#### Ansicht

- Dateibaum ein-/ausblenden
- Kontextbereich ein-/ausblenden
- Statusleiste ein-/ausblenden
- Gliederung anzeigen
- Backlinks anzeigen
- Suchpanel anzeigen

#### Werkzeuge

- Vault neu einlesen
- Verweise prüfen
- Fehlende Links anzeigen
- Einstellungen

#### Hilfe

- Über SASD Notes
- Technische Informationen

### 8.4 Werkzeugleiste

Die Werkzeugleiste bietet Schnellzugriff auf häufige Aktionen:

- Vault öffnen
- Neue Notiz
- Speichern
- Suche
- Fett
- Kursiv
- Überschrift
- Liste
- Checkbox
- Link einfügen
- Backlinks ein-/ausblenden

### 8.5 Statusleiste

Die Statusleiste zeigt kontextbezogen an:

- aktiver Vault
- relativer Pfad der aktuellen Datei
- Änderungsstatus (gespeichert / geändert)
- Anzahl Wörter
- Anzahl Zeichen
- Anzahl Backlinks
- optional Cursorposition Zeile/Spalte

---

## 9. Fachfunktionen – Sollbeschreibung

## 9.1 Vault öffnen

### Ziel

Ein Benutzer soll einen Ordner auf dem Dateisystem auswählen und als aktiven Arbeitsordner öffnen können.

### Umsetzung

- Nutzung eines FolderBrowserDialog oder eines geeigneten Ordnerauswahldialogs
- Pfadvalidierung vor dem Öffnen
- rekursives Einlesen aller `.md`-Dateien im gewählten Ordner
- Aufbau des Navigationsbaums im linken Bereich
- Aktualisierung der Recent-Folders-Liste

### Fehlerfälle

- Ordner existiert nicht mehr
- keine Berechtigung zum Lesen
- I/O-Fehler beim Einlesen
- einzelne fehlerhafte Dateien dürfen den Gesamtladevorgang möglichst nicht komplett blockieren

## 9.2 Recent Folders / Zuletzt verwendete Ordner

### Ziel

Der Benutzer soll schnell auf kürzlich verwendete Vaults/Arbeitsordner zugreifen können.

### Umsetzung

- Liste in lokaler Konfigurationsdatei speichern
- maximale Anzahl konfigurierbar, Standard z. B. 10 Einträge
- nicht mehr vorhandene Pfade beim Öffnen erkennen und bereinigen oder als ungültig markieren
- Menüeintrag unter „Datei → Zuletzt verwendete Ordner“

## 9.3 Anzeige von Ordnern und Notizen

### Ziel

Der Benutzer soll die Inhalte eines Vaults in einer hierarchischen Baumstruktur sehen.

### Umsetzung

- `TreeView` für Ordner und Dateien
- Ordner und `.md`-Dateien mit unterschiedlichen Icons
- Dateiauswahl lädt die Notiz in den Editor
- Kontextmenü auf Ordnern und Dateien

### Kontextmenü Dateiknoten

- Öffnen
- Umbenennen
- Löschen
- Im Explorer anzeigen
- Link einfügen / Pfad kopieren (optional)

### Kontextmenü Ordnerknoten

- Neue Notiz
- Neuer Unterordner
- Umbenennen
- Löschen
- Im Explorer öffnen

## 9.4 Neue Notiz erstellen

### Ziel

Benutzer können neue Markdown-Dateien anlegen.

### Umsetzung

- Dialog mit Eingabefeldern für Titel und optional Zielordner
- Dateiname wird aus dem Titel abgeleitet und validiert
- automatisch `.md` anhängen, falls nicht vorhanden
- leere Datei erzeugen
- Datei im TreeView markieren und im Editor öffnen

### Validierung

- keine ungültigen Dateisystemzeichen
- keine leeren Titel
- Konfliktprüfung bei bereits vorhandenem Dateinamen
- bei Konflikt Vorschlag zur automatischen Nummerierung oder Abbruch

## 9.5 Notiz bearbeiten

### Ziel

Markdown-Dateien sollen komfortabel als Text bearbeitet werden können.

### Umsetzung

- zentraler Editor auf Basis eines Text-Controls
- bevorzugt ein Editor-Control mit guter Textbearbeitung, Zeilenumbrüchen, Scrollbars und Auswahlunterstützung
- optional Syntaxhervorhebung in V1.1 oder später
- Änderungsstatus tracken
- Speichern per Menü, Toolbar und `Ctrl+S`

### Mindestfunktionen

- mehrzeilige Texteingabe
- Laden des Dateiinhalts
- Speichern in Originaldatei
- Markieren/Ersetzen von Textbereichen
- Undo/Redo, soweit vom Control unterstützt

## 9.6 Markdown-Formatierungshilfen

### Ziel

Der Benutzer soll Markdown wie in einem Editor komfortabel formatieren können, ohne die Syntax vollständig manuell tippen zu müssen.

### Umsetzung

Formatierungsaktionen arbeiten grundsätzlich über Textmanipulation:

- bei markiertem Text wird Syntax um die Auswahl gelegt
- ohne Markierung werden Platzhalter oder die notwendige Syntax eingefügt
- Cursorposition soll sinnvoll nachgeführt werden

### Unterstützte Aktionen in V1

- Überschrift 1/2/3
- Fett
- Kursiv
- Durchgestrichen
- Inline-Code
- Codeblock
- Aufzählungsliste
- Nummerierte Liste
- Checkbox-Liste
- horizontale Linie (optional)

### Beispielverhalten

- Auswahl `Text` + Klick „Fett“ → `**Text**`
- Auswahl `Text` + Klick „Kursiv“ → `*Text*`
- keine Auswahl + Klick „Inline-Code“ → Einfügen von `` `Code` `` mit Cursor im Codebereich

## 9.7 Interne Wiki-Links

### Ziel

Markdown-Dateien sollen untereinander über Wiki-Link-Syntax verlinkt werden können.

### Unterstützte Formen

- `[[Notiz]]`
- `[[Ordner/Notiz]]`
- `[[Notiz|Alias]]`
- optional später `[[Ordner/Notiz|Alias]]`

### Umsetzung

- eigener Parser für Wiki-Link-Erkennung
- Speicherung der erkannten Links im internen Modell
- Auflösung gegen bekannte Notizen im aktiven Vault
- Klick auf Link im Editor oder in Kontextansichten öffnet die Zielnotiz

### Fachregeln

- Vergleich standardmäßig case-insensitiv
- `.md` wird intern als technisches Dateiende betrachtet, aber im Wiki-Link normalerweise nicht verlangt
- relative Pfade im Vault werden normalisiert
- bei Mehrdeutigkeit soll V1 zunächst definierte Regeln nutzen, z. B. exakter Pfad vor Dateiname

## 9.8 Backlinks

### Ziel

Zu einer geöffneten Notiz soll angezeigt werden, welche anderen Notizen auf sie verweisen.

### Umsetzung

- Berechnung auf Basis der geparsten Links aller Notizen im Vault
- Anzeige im rechten Kontextbereich
- Ein Klick auf einen Backlink öffnet die referenzierende Notiz
- Anzeige eines kurzen Kontexts oder mindestens der Quellnotiz

## 9.9 Gliederung / Outline

### Ziel

Die Struktur der aktuellen Notiz soll anhand ihrer Überschriften sichtbar sein.

### Umsetzung

- Analyse von Markdown-Überschriften (`#`, `##`, `###`, …)
- Anzeige im rechten Kontextbereich
- Sprung zur entsprechenden Stelle im Editor

## 9.10 Suche

### Ziel

Benutzer sollen Notizen schnell nach Dateinamen und Inhalten durchsuchen können.

### Umsetzung V1

- Suche über alle geladenen Notizen im aktiven Vault
- Durchsuchung von Titel/Dateiname und Inhalt
- Trefferliste mit Dateiname, relativer Pfadangabe und Vorschautext
- Sortierung nach einfacher Relevanzlogik

### Spätere Ausbaustufen

- Fuzzy-Suche
- Tagsuche
- Suchoperatoren
- Highlighting im Editor
- optional lokaler Suchindex

## 9.11 Umbenennen von Notizen

### Ziel

Der Benutzer kann eine Datei umbenennen, ohne die Linkintegrität unnötig zu verlieren.

### Umsetzung

- Dialog zur Eingabe des neuen Titels/Dateinamens
- Datei im Dateisystem umbenennen
- internes Modell aktualisieren
- optional in V1 bereits referenzierende Wiki-Links anpassen, wenn sie eindeutig auf diese Datei zeigen

### Hinweis

Die automatische Aktualisierung referenzierender Links ist fachlich wichtig, aber technisch etwas aufwendiger. Sie sollte im Pflichtenheft als Ziel für V1 enthalten sein, darf jedoch in der ersten technisch lauffähigen Iteration als gesonderter Ausbauschritt umgesetzt werden.

## 9.12 Löschen von Notizen

### Ziel

Notizen können aus dem Vault entfernt werden.

### Umsetzung

- Sicherheitsabfrage vor dem Löschen
- Löschen der Datei im Dateisystem
- TreeView und interner Zustand aktualisieren
- Links, die auf gelöschte Notizen zeigen, werden anschließend als ungelöst behandelt

## 9.13 Ungelöste Links anzeigen

### Ziel

Nicht auflösbare Wiki-Links sollen erkannt und sichtbar gemacht werden.

### Umsetzung

- Link-Resolver markiert Links ohne passendes Ziel
- Anzeige im Werkzeuge-/Prüfbereich oder im Kontextpanel
- spätere Komfortfunktion: „Notiz aus fehlendem Link erstellen“

## 9.14 Speichern von Einstellungen

### Ziel

Wichtige Benutzereinstellungen und letzte Zustände sollen lokal gespeichert werden.

### Inhalte

- Liste der zuletzt verwendeten Ordner
- Fenstergröße und Position
- zuletzt sichtbare Panels
- optional zuletzt geöffneter Vault

### Umsetzung

- einfache JSON-Konfigurationsdatei im Benutzerprofil
- robuste Lade-/Fallback-Logik

---

## 10. Nichtfunktionale Anforderungen

## 10.1 Bedienbarkeit

- klassische Desktop-Bedienung mit Menü, Toolbar, TreeView und Statusleiste
- Tastaturkürzel für häufige Aktionen
- klare und ruhige Oberfläche ohne unnötige Überladung
- nachvollziehbares Verhalten bei Dateioperationen

## 10.2 Performance

- kleine bis mittlere Vaults sollen ohne spürbare Wartezeit geöffnet werden
- UI darf beim Laden größerer Verzeichnisse nicht dauerhaft blockieren
- Suchfunktion soll für typische lokale Wissensbestände alltagstauglich reagieren

## 10.3 Wartbarkeit

- klare Schichtentrennung
- testbare Fachlogik
- kommentierter Code im SASD-Stil
- keine Vermischung von UI-Logik und Dateisystemlogik in den Forms

## 10.4 Robustheit

- fehlerhafte Einzeldateien sollen soweit möglich isoliert behandelt werden
- unvollständige oder kaputte Einstellungen dürfen nicht zum Startabbruch führen
- Speichern darf keine unbemerkten Datenverluste erzeugen

## 10.5 Erweiterbarkeit

Die Architektur soll Erweiterungspunkte vorsehen für:

- Vorschau-/Preview-Komponente
- Tags und Properties
- Exportfunktionen
- optionalen Suchindex
- spätere Plugin-Schnittstellen

---

## 11. Technische Entwurfsdetails

## 11.1 Empfohlene Hauptklassen in Domain

- `VaultInfo`
- `NoteDocument`
- `WikiLink`
- `ResolvedLink`
- `SearchHit`
- `BacklinkInfo`
- `OutlineEntry`
- `AppSettings`

## 11.2 Empfohlene Services in Application

- `VaultService`
- `NoteService`
- `WikiLinkParser`
- `LinkResolver`
- `SearchService`
- `BacklinkService`
- `OutlineService`
- `RenameService`
- `SettingsService`

## 11.3 Empfohlene Infrastruktur-Komponenten

- `FileSystemVaultRepository`
- `SettingsRepository`
- `FileNameHelper`
- `VaultScanner`
- `FileSystemWatcherService` (optional V1.1)

## 11.4 Empfohlene Forms und Dialoge

- `MainForm`
- `CreateNoteDialog`
- `RenameItemDialog`
- `SettingsDialog`
- `AboutDialog`
- optional `SearchPanelControl`, `BacklinksPanelControl`, `OutlinePanelControl` als UserControls

## 11.5 UI-Control-Empfehlungen

- `MenuStrip` für Menüleiste
- `ToolStrip` für Werkzeugleiste
- `StatusStrip` für Statusleiste
- `SplitContainer` für Hauptlayout
- `TreeView` für Ordner- und Dateibaum
- Editor-Control: zunächst `TextBox` multiline oder besser ein geeigneteres Texteditor-Control; die Auswahl ist projektspezifisch zu treffen
- `TabControl` oder Panelumschaltung für Suchtreffer, Backlinks und Gliederung

---

## 12. Ablauflogik wichtiger Anwendungsfälle

## 12.1 Anwendung starten

1. Anwendung lädt Einstellungen.
2. Fensterzustand wird wiederhergestellt.
3. Menüstruktur und UI werden initialisiert.
4. Falls konfiguriert, kann der letzte Vault optional automatisch angeboten oder geladen werden.
5. Ist kein Vault aktiv, bleibt die Anwendung im leeren Startzustand betriebsbereit.

## 12.2 Vault öffnen

1. Benutzer wählt „Arbeitsordner öffnen…“.
2. Ordnerdialog wird angezeigt.
3. Nach Auswahl erfolgt Validierung.
4. Vault wird rekursiv eingelesen.
5. Navigationsbaum wird aufgebaut.
6. Recent-Folders-Liste wird aktualisiert.
7. Erste oder zuletzt geöffnete Notiz kann optional geladen werden.

## 12.3 Neue Notiz anlegen

1. Benutzer wählt „Neue Notiz“.
2. Dialog fragt Titel und Zielordner ab.
3. Titel wird validiert.
4. Datei wird erstellt.
5. Navigationsbaum aktualisiert.
6. Neue Datei wird geöffnet.

## 12.4 Notiz speichern

1. Benutzer bearbeitet Inhalt.
2. Änderungsstatus wird gesetzt.
3. Benutzer speichert oder Auto-Save wird später ergänzt.
4. Datei wird auf das Dateisystem geschrieben.
5. Parser und Indexdaten werden aktualisiert.
6. Backlinks und Suchbasis werden neu berechnet oder aktualisiert.

## 12.5 Link anklicken

1. Benutzer klickt auf einen internen Link.
2. Parser/Resolver identifiziert das Ziel.
3. Zielnotiz wird geöffnet.
4. Falls kein Ziel gefunden wird, erfolgt eine definierte Reaktion, z. B. Hinweis oder Angebot zur Erstellung.

---

## 13. Plugin-System – Berücksichtigung für spätere Versionen

Das Plugin-System ist nicht Bestandteil von V1. Dennoch sollen folgende architektonische Vorbereitungen berücksichtigt werden:

- Kernfunktionen werden über Services und Interfaces gekapselt
- fachliche Logik bleibt außerhalb der Forms
- mögliche Erweiterungspunkte werden identifiziert, z. B. neue Panels, Exporter, Parser-Erweiterungen, Zusatzbefehle
- Menüs und Panels sollen nicht so starr gebaut werden, dass spätere Erweiterungen nur mit umfangreicher Umbauarbeit möglich sind

Für V1 wird ausdrücklich **kein** dynamisches Nachladen fremder Assemblies verlangt.

---

## 14. Testkonzept

## 14.1 Unit-Tests

Zu testen sind mindestens:

- Wiki-Link-Parser
- Link-Auflösung
- Suchservice
- Gliederungsanalyse
- Dateinamensvalidierung
- Umbenennungslogik

## 14.2 Integrationstests

Zu testen sind mindestens:

- Laden eines Beispiel-Vaults
- Erstellen und Speichern einer Notiz
- Umbenennen einer Notiz
- Aktualisierung von Such- und Backlink-Daten
- Umgang mit fehlenden Dateien oder beschädigten Inhalten

## 14.3 Manuelle UI-Tests

Zu prüfen sind mindestens:

- Menüfunktionen
- Toolbar-Funktionen
- Editorverhalten
- Auswahl im TreeView
- Statusleistenanzeige
- Recent-Folders-Verhalten
- Performance bei mehreren Ordnern und vielen Dateien

---

## 15. Sicherheits- und Datenschutzaspekte

- Anwendung arbeitet lokal ohne Cloud-Zwang
- keine unnötige Telemetrie in V1
- keine heimliche Hintergrundkommunikation
- lokale Dateien dürfen nur im Rahmen der Benutzeraktion gelesen und geschrieben werden
- spätere Funktionen mit externen Diensten müssen separat spezifiziert werden

---

## 16. Umsetzungsreihenfolge / Meilensteine

## 16.1 Iteration 1 – Fundament

- Solution und Projekte anlegen
- Grundarchitektur aufbauen
- MainForm mit Menü, Toolbar, Statusleiste
- Vault öffnen
- TreeView anzeigen
- Notiz laden und im Editor anzeigen
- Speichern implementieren

## 16.2 Iteration 2 – Wissensfunktionen

- Wiki-Link-Parser
- Link-Auflösung
- interne Link-Navigation
- Suche
- Backlinks
- Gliederung

## 16.3 Iteration 3 – Komfortfunktionen

- Neue Notiz
- Neuer Ordner
- Umbenennen
- ungelöste Links anzeigen
- Recent Folders
- Einstellungen speichern

## 16.4 Iteration 4 – Stabilisierung

- bessere Fehlerbehandlung
- Tests ausbauen
- Performance prüfen
- UI verfeinern
- optionale Vorbereitung für Preview und Erweiterungspunkte

---

## 17. Abnahmekriterien für V1

SASD Notes V1 gilt als fachlich umsetzungsbereit bzw. abnahmefähig, wenn mindestens folgende Punkte erfüllt sind:

1. Ein lokaler Arbeitsordner kann geöffnet werden.
2. Markdown-Dateien werden im Navigationsbaum dargestellt.
3. Eine Notiz kann geöffnet, bearbeitet und gespeichert werden.
4. Markdown-Formatierungsbefehle fügen korrekt Syntax ein.
5. Interne Wiki-Links können erkannt und aufgelöst werden.
6. Backlinks zur aktuellen Notiz werden angezeigt.
7. Eine Suche über Titel und Inhalte funktioniert.
8. Neue Notizen und Unterordner können erstellt werden.
9. Zuletzt verwendete Ordner werden gespeichert und angeboten.
10. Die Anwendung bleibt bei typischen Bedienfällen stabil und nachvollziehbar bedienbar.

---

## 18. Offene Punkte / spätere Entscheidungen

- Auswahl des endgültigen Editor-Controls
- Entscheidung über Preview in V1 oder V1.1
- Entscheidung über automatische Link-Aktualisierung beim Umbenennen bereits in erster Produktivfassung oder kurz danach
- Entscheidung über Dateisystem-Watcher bereits in V1 oder V1.1
- Entscheidung über Tag- und Property-Unterstützung in V1.1

---

## 19. Zusammenfassung

Dieses Pflichtenheft definiert SASD Notes als lokal arbeitende Windows-Forms-Anwendung zur strukturierten Verwaltung verlinkter Markdown-Notizen. Im Mittelpunkt stehen eine saubere Desktop-Bedienung, eine klare Dateisystembasis, Markdown-Formatierungshilfen, interne Wiki-Links, Suchfunktion, Backlinks und die Verwaltung verschiedener Themenordner bzw. Vaults. Die Architektur ist so angelegt, dass spätere Erweiterungen – insbesondere Preview, Properties, Tags und ein Plugin-System – ohne grundlegende Neuentwicklung möglich bleiben.

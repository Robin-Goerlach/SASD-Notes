# Funktionskatalogheft: Obsidian  
**Stand der Recherche:** 04.07.2026  
**Dokumenttyp:** Funktionskatalog / Referenzheft für die Ableitung von **SASD Notes**  
**Sprache:** Deutsch

---

## 1. Ziel und Zweck des Dokuments

Dieses Heft sammelt die **offiziell dokumentierten Funktionen von Obsidian** so vollständig wie praktikabel und beschreibt sie in einer Form, die für die Planung eines eigenen Systems wie **SASD Notes** nutzbar ist.

Das Dokument verfolgt drei Ziele:

1. **Funktionsinventar:** Welche Fähigkeiten bringt Obsidian im Kern mit?
2. **Strukturhilfe:** Welche Funktionsgruppen sind für einen eigenen Nachbau relevant?
3. **Projektableitung:** Welche Funktionen sollten in SASD Notes in welcher Ausbaustufe landen?

---

## 2. Wichtige Abgrenzung

Dieses Heft versucht **nicht**, jedes einzelne Community-Plugin von Obsidian einzeln zu katalogisieren. Das wäre praktisch nicht seriös möglich, weil das Ökosystem laut offizieller Hilfe aus **tausenden Plugins und Themes** besteht. Stattdessen erfasst dieses Dokument:

- die **Kernfunktionen** des Produkts,
- die **offiziell dokumentierten Core Plugins**,
- die **offiziellen Zusatzdienste**,
- die **offiziellen Erweiterungsmechanismen**,
- die **von Obsidian selbst gepflegten Zusatzwerkzeuge** wie Importer, Maps, Web Clipper und CLI.

Damit ist der Katalog **für Architektur- und Produktplanung belastbar**, ohne eine Scheingenauigkeit vorzutäuschen.

---

## 3. Kurzprofil von Obsidian

Obsidian ist ein lokales, dateibasiertes Wissens- und Notizsystem. Die Grundidee ist nicht „Cloud-Dokument zuerst“, sondern **lokale Vaults**, die aus **Markdown-Dateien** bestehen. Ein Vault ist schlicht ein Ordner im Dateisystem. Dadurch bleiben die Inhalte grundsätzlich außerhalb der Anwendung nutzbar.

Wesentliche Produktprinzipien:

- **Local-first**
- **Markdown als Primärformat**
- **Links zwischen Notizen als zentrales Strukturprinzip**
- **stark erweiterbar** durch Core Plugins, Community Plugins, Themes und CSS
- **Mehrgeräte-Nutzung** per Sync-Dienst oder anderer Dateisynchronisation
- **Wissensnetz statt linearer Ordnerlogik** als wichtiges Anwendungsmodell

### Konsequenz für SASD Notes

Für einen Clone oder ein inspiriertes Eigenprodukt ist das wichtigste Merkmal nicht der Editor allein, sondern die Kombination aus:

- Dateiordner als Vault,
- Markdown-Dateien,
- interne Wiki-Links,
- Rückverweise,
- Suche,
- Graph-/Netzsicht,
- Metadaten/Properties,
- erweiterbare Ansichten über dieselben Dateien.

---

## 4. Fundamentale Daten- und Strukturmodelle

### 4.1 Vault

Ein **Vault** ist ein Ordner auf dem lokalen Dateisystem. Ein Benutzer kann ein neues leeres Vault anlegen oder einen bestehenden Ordner als Vault verwenden.

**Bedeutung für den Nachbau:**

- Die Anwendung muss mit einem realen Ordner arbeiten.
- Ein Vault ist keine proprietäre Datenbank.
- Mehrere Vaults sind möglich.
- Ein Vault kann externe Änderungen erkennen bzw. nachziehen.

### 4.2 Primäre Dateiformate

Obsidian erkennt offiziell unter anderem folgende Formate:

- Markdown-Dateien: `.md`
- Bases-Dateien: `.base`
- Canvas-Dateien: `.canvas`
- Bilder: `.avif`, `.bmp`, `.gif`, `.jpeg`, `.jpg`, `.png`, `.svg`, `.webp`
- Audio: `.flac`, `.m4a`, `.mp3`, `.ogg`, `.wav`, `.webm`, `.3gp`
- Video: `.mkv`, `.mov`, `.mp4`, `.ogv`, `.webm`
- PDF: `.pdf`

### 4.3 Plain-Text-Orientierung

Notizen werden als **Markdown-Plain-Text-Dateien** gespeichert. Das hat mehrere Folgen:

- Die Dateien bleiben außerhalb der App lesbar.
- Andere Editoren können auf dieselben Dateien zugreifen.
- Git, Backup-Tools und normale Dateisynchronisation bleiben nutzbar.
- Ein Vendor-Lock-in wird verringert.

### 4.4 Externe Änderungen

Obsidian aktualisiert laut offizieller Dokumentation das Vault automatisch, um **externe Änderungen** mitzuziehen. Das ist für einen Nachbau sehr wichtig, weil die Anwendung nie annehmen darf, alleiniger Besitzer der Dateien zu sein.

---

## 5. Funktionskatalog nach Fachbereichen

## 5.1 Notizerstellung und Grundbearbeitung

### 5.1.1 Neue Notizen anlegen
Notizen können auf mehreren Wegen entstehen:

- über den Dateibaum / File Explorer,
- über Tastatur und Quick Switcher,
- über Tagesnotizen,
- über eindeutige Zettel/Unique Notes,
- über Links auf noch nicht existierende Dateien,
- über URI/Automation,
- durch Import.

**Ableitung für SASD Notes:**  
Die Anwendung sollte mindestens folgende Wege erlauben:

- neue Notiz im Standardordner,
- neue Notiz in ausgewähltem Ordner,
- neue Notiz aus einem nicht aufgelösten Link,
- neue Notiz aus Such-/Switcher-Eingabe.

### 5.1.2 Bearbeitungsmodus
Obsidian unterstützt die Bearbeitung von Markdown mit unterschiedlichen Anzeigeformen. Für einen Nachbau besonders relevant sind:

- Quelle / Source-orientierte Bearbeitung
- gerenderte Vorschau
- eine hybride Benutzererfahrung, bei der Markdown weiterhin die Wahrheit bleibt

### 5.1.3 Mehrere Notizen gleichzeitig
Obsidian unterstützt das parallele Arbeiten mit mehreren Notizen in Tabs und Splits. Das ist für ernsthafte Wissensarbeit sehr wichtig:

- Quellnotiz links, Zielnotiz rechts
- Vergleich
- Abschreiben / Refactoring
- Recherche mit mehreren Fenstern
- Arbeitsbereiche / Workspaces

### 5.1.4 Dateibenennung
Notizen orientieren sich an Dateinamen. Gleichzeitig existieren zusätzliche Mechanismen wie Aliases und Properties, um denselben Inhalt unter mehreren Namen adressierbar zu machen.

---

## 5.2 Markdown- und Inhaltsfunktionen

### 5.2.1 Markdown-Grundsyntax
Obsidian baut auf Markdown auf und unterstützt die üblichen Strukturen:

- Überschriften
- Absätze
- Listen
- Aufgabenlisten
- Zitate
- Tabellen
- Codeblöcke
- Hervorhebung
- Links
- Bilder
- Fußnoten
- Kommentare
- horizontale Trenner

### 5.2.2 Fortgeschrittene Formatierung
Zusätzlich zu normalem Markdown dokumentiert Obsidian unter anderem:

- **Callouts** (Hinweis-/Warn-/Info-Blöcke mit Typen)
- **Fußnoten**
- **Kommentare** per `%%`
- Einbettungen von Notizen und Dateien
- Block- und Abschnittslinks

### 5.2.3 Callouts
Callouts sind strukturierte Hinweisblöcke, z. B. für Info, Warnung, Tipp, Todo. Sie sind nicht nur optisch, sondern fachlich relevant, weil sie strukturierte semantische Elemente in Markdown einführen.

**Bedeutung für SASD Notes:**  
Für V1 nicht zwingend notwendig, aber mittelfristig wertvoll. Vor allem dann, wenn Preview oder PDF-/HTML-Ausgabe später dazukommt.

### 5.2.4 Fußnoten
Obsidian unterstützt Markdown-Fußnoten und besitzt zusätzlich eine eigene **Footnotes view**, um sie in der aktiven Notiz zu verwalten.

### 5.2.5 Kommentare
Kommentare können in der Bearbeitung sichtbar sein, ohne in Reader-/Publish-Szenarien normal dargestellt zu werden. Das ist für Arbeitsentwürfe, Redaktionsnotizen und „unsichtbare“ Bearbeitungshinweise nützlich.

---

## 5.3 Links, Verweise und Wissensnetz

Das ist der eigentliche Kern von Obsidian.

### 5.3.1 Interne Links
Obsidian unterstützt interne Links zwischen Notizen und Dateien. Dabei können Links nicht nur auf ganze Notizen gehen, sondern auch auf:

- Überschriften
- Blöcke
- Abschnitte
- andere akzeptierte Dateien

### 5.3.2 Automatisches Aktualisieren von Links
Wenn eine Datei umbenannt wird, kann Obsidian interne Links im Vault automatisch aktualisieren. Diese Funktion ist für Wissenssysteme extrem wichtig, weil Umbenennungen sonst zu einem schleichenden Zerfall des Linknetzes führen.

**Für SASD Notes zwingend wichtig.**

### 5.3.3 Aliases
Ein Alias erlaubt, dass eine Notiz unter mehreren Namen verlinkt und gefunden werden kann. Das ist sehr nützlich für:

- Synonyme,
- Namensvarianten,
- Abkürzungen,
- Schreibweisen,
- Mehrsprachigkeit.

### 5.3.4 Links auf Überschriften
Es kann auf eine bestimmte Überschrift innerhalb derselben oder einer anderen Notiz verlinkt werden.

### 5.3.5 Links auf Blöcke
Es kann auf einzelne Blöcke innerhalb von Notizen verlinkt werden. Das ist besonders nützlich bei:

- granularer Wissensvernetzung,
- wissenschaftlichen Notizen,
- Literaturarbeit,
- modularen Textbausteinen.

### 5.3.6 Einbettungen
Mit Embed-Syntax können Inhalte **inline** angezeigt werden. Einbettbar sind u. a.:

- Notizen,
- Überschriften,
- Blöcke,
- Bilder,
- Audio,
- Video,
- PDFs,
- andere unterstützte Dateien.

### 5.3.7 Backlinks
Backlinks zeigen an, **wer auf die aktuelle Notiz verweist**. Obsidian unterscheidet dabei u. a. zwischen:

- verlinkten Erwähnungen,
- unverlinkten Erwähnungen (je nach Kontext/Funktion),
- Filterung der Treffer.

Backlinks sind ein zentrales Wissensmerkmal, weil sie Navigation in Gegenrichtung erlauben.

### 5.3.8 Outgoing Links
Neben Backlinks gibt es die Gegenperspektive: welche Links von der aktiven Notiz ausgehen. Outgoing Links zeigen außerdem potenzielle neue Verlinkungsmöglichkeiten.

### 5.3.9 Page Preview
Beim Überfahren interner Links können verlinkte Seiten vorab angezeigt werden. Das reduziert Navigationskosten und fördert exploratives Arbeiten.

**Für SASD Notes:**  
Sehr sinnvoll, aber kein V1-Muss. Gute Kandidatfunktion für V1.1 oder V2.

---

## 5.4 Metadaten, Properties und strukturierte Information

### 5.4.1 Properties
Properties sind strukturierte Metadaten in Notizen. Sie basieren auf YAML und können Werte wie folgende Typen speichern:

- Text
- Zahl
- Checkbox
- Datum
- Datum/Zeit
- Listen

### 5.4.2 Standard-Properties
Obsidian nennt insbesondere:

- `tags`
- `cssclasses`
- `aliases`

### 5.4.3 Property-Typisierung
Properties haben nicht nur Namen und Werte, sondern auch **Typen**, die bestimmen, wie Obsidian sie interpretiert und verarbeitet.

### 5.4.4 Anzeige-Modi für Properties
Properties können in unterschiedlichen Modi im Dokument gezeigt werden:

- sichtbar
- verborgen
- als reine YAML-Quelle

### 5.4.5 Properties View
Die Core-Funktion **Properties view** bringt zwei Seitenleistenansichten:

- **File properties** für die aktive Notiz
- **All properties** für den gesamten Vault

Damit wird aus losen YAML-Daten ein halbstrukturiertes Datenmodell auf Vault-Ebene.

### 5.4.6 Tags
Tags sind ein eigenes Organisationsmittel, aber gleichzeitig mit Properties und Suche verknüpft. Offizielle Eigenschaften:

- Tags sind case-insensitive
- sie erlauben verschachtelte / verschränkte Strukturen wie `#projekt/kunde`
- sie erscheinen in Tags View
- sie sind direkt suchbar

### 5.4.7 Tags View
Die Tags View zeigt alle Tags im Vault samt Häufigkeit. Sie unterstützt Sortierung und die Darstellung verschachtelter Tags als Baum oder flache Liste.

**Für SASD Notes:**  
Properties und Tags sollten früh mitgedacht werden, selbst wenn V1 nur einfache Properties unterstützt. Sonst verbaut man sich Bases-ähnliche Funktionen.

---

## 5.5 Dateiorganisation und Dateiverwaltung

### 5.5.1 File Explorer
Der File Explorer ist ein Kernbaustein mit umfangreichen Dateifunktionen:

- Dateien und Ordner erstellen
- umbenennen
- löschen
- verschieben
- Drag & Drop
- Sortierung nach Name, Änderungszeit, Erstellungszeit
- Auto-Reveal der aktiven Datei
- Ordner komplett ein-/ausklappen

### 5.5.2 Ordner als Organisationsmittel
Obsidian erlaubt klassische Ordnerstrukturen, zwingt aber nicht zu ihnen. Das ist wichtig: Obsidian kombiniert **hierarchische Ablage** und **netzartige Verknüpfung**.

### 5.5.3 Attachments
Anhänge sind reguläre Dateien im Vault und können auf mehreren Wegen eingebracht werden:

- Drag & Drop
- Dateimanager
- Copy/Paste
- spätere Einbettung

### 5.5.4 Akzeptierte Dateiformate
Neben Markdown akzeptiert Obsidian definierte Medien- und Dokumenttypen. Das erlaubt, dass ein Vault nicht nur Text, sondern auch Artefakte wie PDFs und Bilder enthält.

### 5.5.5 Standardort für neue Notizen
Die Dokumentation weist darauf hin, dass der Standardort für neue Notizen konfigurierbar ist. Das ist für produktives Arbeiten und Organisationsmuster sehr wichtig.

---

## 5.6 Suche und Wiederfinden

### 5.6.1 Volltextsuche
Die Suchfunktion ist in Obsidian ausgesprochen mächtig und geht deutlich über ein einfaches `Contains` hinaus.

### 5.6.2 Grundfähigkeiten der Suche
Offiziell dokumentiert sind u. a.:

- Suche nach Begriffen und Phrasen
- exakte Phrasen mit Anführungszeichen
- `OR`
- Negation mit `-`
- Gruppierung mit Klammern
- Sortierung der Treffer
- Kontextanzeige
- Kopieren von Suchergebnissen
- Einbetten von Suchergebnissen in Notizen

### 5.6.3 Suchoperatoren
Obsidian dokumentiert u. a. diese Suchoperatoren:

- `file:`
- `path:`
- `content:`
- `match-case:`
- `ignore-case:`
- `tag:`
- `line:`
- `block:`
- `section:`
- `task:`
- `task-todo:`
- `task-done:`

### 5.6.4 Suche in Properties
Properties sind direkt in Suchausdrücken nutzbar:

- `[property]`
- `[property:value]`
- Abfragen gegen leere Werte
- Kombination mit `OR`, Klammern und regulären Ausdrücken

### 5.6.5 Reguläre Ausdrücke
Obsidian unterstützt reguläre Ausdrücke in Suchbegriffen. Laut offizieller Hilfe verwendet Obsidian **JavaScript-RegEx**.

### 5.6.6 Explain Search Term
Eine besonders interessante Komfortfunktion ist die Erklärung komplexer Suchbegriffe in Klartext. Das zeigt, dass die Suche nicht nur mächtig, sondern auch erklärbar werden soll.

### 5.6.7 Search-Embedding
Suchabfragen können per `query`-Codeblock in Notizen eingebettet werden. Damit wird Suche von einer reinen Bedienfunktion zu einem Teil des Inhaltsmodells.

**Für SASD Notes:**  
Die Suche ist ein eigener großer Fachbereich. V1 braucht nur einen Kern. Langfristig ist eine Obsidian-artige Suchsprache ein erheblicher Mehrwert.

---

## 5.7 Navigation und Bedienung

### 5.7.1 Quick Switcher
Der Quick Switcher ist eine Tastatur-zentrierte Navigation:

- Suche nach Name oder Alias
- Öffnen per Enter
- neue Notiz erstellen, wenn keine passt
- exakten Namen erzwingen mit `Shift+Enter`
- Öffnen in neuem Tab
- Umschalten zu zuletzt genutzten Notizen
- Performance-Anpassung bei sehr großen Vaults (ab 10.000 Elementen laut Doku)

### 5.7.2 Command Palette
Die Command Palette ist die Schaltzentrale für Befehle:

- beliebige Kommandos ausführen
- Hotkeys erkunden
- fuzzy matching
- zuletzt verwendete Kommandos
- anheftbare / pinnbare Befehle

### 5.7.3 Slash Commands
Slash Commands erlauben Befehle direkt im Editor per `/`. Das ist ein moderner, editornaher Bedienweg.

### 5.7.4 Hotkeys
Für Obsidian-Kommandos können benutzerdefinierte Tastenkombinationen vergeben werden.

### 5.7.5 Bookmarks
Bookmarks sind keine simplen Dateisterne, sondern können u. a. speichern:

- Dateien
- Ordner
- Graphen
- Suchen
- Überschriften
- Blöcke
- Links

Damit sind Bookmarks ein echter Arbeitsbereichsspeicher.

### 5.7.6 Random Note
Random Note öffnet zufällig eine Notiz. Für Wissensarbeit ist das überraschend wertvoll:

- Wiederentdeckung alter Gedanken
- Review
- serendipitische Verknüpfung
- kreative Impulse

### 5.7.7 Outline
Outline zeigt das Inhaltsverzeichnis der aktiven Notiz und erlaubt nicht nur Navigation, sondern laut offizieller Hilfe auch das Umordnen von Abschnitten per Drag & Drop.

### 5.7.8 Workspaces
Workspaces speichern komplette Layouts:

- offene Dateien
- Tabs
- Sichtbarkeit/Breite der Seitenleisten

Sie erlauben damit kontextabhängige Arbeitsräume wie:

- Journaling
- Schreiben
- Lesen
- Recherche
- Review

---

## 5.8 Visualisierung und nichtlineares Arbeiten

### 5.8.1 Graph View
Die Graph View visualisiert Notizen als Knoten und Links als Kanten. Es gibt:

- globale Graphsicht
- lokale Graphsicht
- Interaktion per Hover und Klick
- Filter- und Anzeigeoptionen

### 5.8.2 Canvas
Canvas ist eine visuelle 2D-Arbeitsfläche mit unendlichem Raum. Darin können angeordnet und verbunden werden:

- Notizen
- Anhänge
- Webseiten
- Medien
- Gruppen / Zusammenhänge

Canvas wird als `.canvas` im offenen **JSON Canvas**-Format gespeichert.

### 5.8.3 Publish-Graph
Obsidian Publish kann den Wissensgraphen auch im Web sichtbar machen, allerdings mit geringeren Optionen als in der App.

### 5.8.4 Eingebettete Medien
Canvas und Notizen können Medien, PDFs und andere unterstützte Inhalte inline darstellen. Das macht Obsidian nicht nur zu einem Texttool, sondern zu einem gemischten Wissensraum.

---

## 5.9 Bases: datenbankartige Ansichten auf Markdown-Dateien

Bases ist eine der wichtigsten neueren Funktionen und für die Zukunft von Obsidian sehr bedeutend.

### 5.9.1 Grundidee
Bases erzeugt **datenbankartige Ansichten** über Notizen und deren Properties. Die Daten bleiben dabei in Markdown-Dateien und deren Metadaten gespeichert.

### 5.9.2 Mögliche Aktionen
Mit Bases kann man Dateien:

- anzeigen
- editieren
- sortieren
- filtern
- gruppieren
- berechnen / auswerten

### 5.9.3 Speicherform
Bases-Definitionen können:

- als `.base`-Datei gespeichert werden,
- oder in Markdown eingebettet sein.

### 5.9.4 View-Typen
Offiziell dokumentiert sind derzeit:

- **Table**
- **Cards**
- **List**
- **Map** (mit Maps-Plugin)

#### Table View
- Dateien als Zeilen
- Properties als Spalten
- Zusammenfassungen / Summaries auf Spaltenebene
- unterschiedliche Zeilenhöhen

#### Cards View
- Galerieartige Darstellung
- optionale Coverbilder
- Bild aus lokaler Verknüpfung, externer URL oder Farbe
- Bildanpassung / Seitenverhältnis

#### List View
- Dateien als Listen
- nummeriert, unnummeriert oder ohne Marker
- Property-Einrückung / Trenner

#### Map View
- Dateien als Marker auf einer Karte
- Koordinaten, Farbe, Icon als Properties
- interaktive Geodaten-Sicht
- benötigt Maps-Plugin

### 5.9.5 Views, Filter und Formeln
Bases unterstützt:

- mehrere Views pro Base
- globale und view-spezifische Filter
- Formeln
- Funktionen
- YAML-basierte Base-Syntax

### 5.9.6 Funktionsumfang der Bases-Sprache
Die dokumentierte Syntax erlaubt u. a.:

- Listenfunktionen
- Link-/Dateifunktionen
- Tag-/Ordnerprüfungen
- Formeln
- reguläre Ausdrücke
- Aggregationen und Summaries

**Für SASD Notes:**  
Bases ist kein V1-Thema. Aber es zeigt, wie wichtig ein sauberer Umgang mit Properties ist. Wenn SASD Notes später einmal „Datei-Tabellen über Markdown“ können soll, müssen Properties und Such-/Filterlogik von Anfang an robust modelliert werden.

---

## 5.10 Capture, Vorlagen und wiederkehrende Notizarten

### 5.10.1 Daily Notes
Daily Notes öffnen oder erzeugen eine Notiz anhand des aktuellen Datums. Typische Anwendungsfälle:

- Journal
- Tageslog
- Tagesplanung
- Sammelblatt für den Tag

### 5.10.2 Templates
Templates erlauben das Einfügen vordefinierter Inhalte in Notizen. Dazu gehören auch Variablen wie aktuelles Datum und aktuelle Zeit.

### 5.10.3 Templates-Ordner
Die Vorlagen liegen in einem konfigurierbaren Templates-Ordner.

### 5.10.4 Template-Variablen
Templates unterstützen dynamische Platzhalter, insbesondere für Datum und Zeit.

### 5.10.5 Unique Note Creator
Diese Funktion erstellt Notizen mit zeitbasierten Namen, typischerweise im Stil von Zettelkasten-IDs wie `202401010945`.

### 5.10.6 Audio Recorder
Mit dem Audio Recorder können Sprach- bzw. Tonaufnahmen direkt in einer Notiz aufgezeichnet und gespeichert werden.

### 5.10.7 Web Clipper
Der offizielle Obsidian Web Clipper ist eine Browser-Erweiterung, mit der Webinhalte in den Vault übernommen werden können. Die offizielle Hilfe hebt hervor:

- Webseiten markieren / clippen
- Inhalte ins Vault speichern
- eigene Clipper-Templates
- Erfassen und Organisieren von Web-Metadaten

### 5.10.8 Importer
Der offizielle Importer dient dem Umzug aus anderen Programmen und Formaten. Die offizielle Import-Liste umfasst u. a.:

- Notion
- Microsoft OneNote
- Evernote
- Apple Notes
- Apple Journal
- Google Keep
- Bear
- Craft
- Roam Research
- HTML
- CSV
- Markdown
- Textbundle
- Zettelkasten-Notizen

### 5.10.9 Format Converter
Der Format Converter wandelt Markdown aus anderen Anwendungen in Obsidian-kompatibles Format um. Das ist vor allem beim Migrationspfad wichtig.

---

## 5.11 Schutz, Wiederherstellung, Backup und Sync

### 5.11.1 File Recovery
File Recovery legt automatisch Snapshots an, um Datenverlust durch versehentliche Löschung, Beschädigung oder unerwünschte Änderungen abzufedern.

Wichtige Punkte:

- komplette Snapshots, nicht nur Diffs
- standardmäßig mindestens 5 Minuten Abstand
- standardmäßig 7 Tage Aufbewahrung
- Speicherung außerhalb des Vaults
- **kein Ersatz für echte Backups**

### 5.11.2 Backup-Denken
Die offizielle Hilfe betont ausdrücklich: **Sync ist kein Backup**. Das ist architektonisch wichtig.

### 5.11.3 Obsidian Sync
Der offizielle Sync-Dienst bietet:

- Synchronisation über Geräte hinweg
- Ende-zu-Ende-Verschlüsselung
- Versionshistorie
- selective sync
- gemeinsame Vaults / Team-Kollaboration
- Headless Sync / CLI-bezogene Automatisierung

### 5.11.4 Sicherheit von Sync
Offiziell dokumentiert sind:

- Ende-zu-Ende-Verschlüsselung
- AES-256-GCM
- Schlüsselerzeugung über scrypt
- Hinweise zur Verifizierbarkeit der Verschlüsselung

### 5.11.5 Selective Sync
Selective Sync erlaubt, Dateitypen und Konfigurationen gezielt ein- oder auszuschließen. Standardmäßig werden große Binärdateien wie Bilder, Audio, Video und PDFs speziell behandelt.

### 5.11.6 Version History
Sync führt eine Versionshistorie über synchronisierte Dateien. Diese erlaubt:

- frühere Versionen ansehen
- wiederherstellen
- gelöschte Dateien zurückholen

### 5.11.7 Shared Vaults / Team-Nutzung
Obsidian dokumentiert Team-Szenarien mit geteilten Vaults. Außerdem weist die Team-Dokumentation darauf hin, dass wegen der Markdown-Dateien grundsätzlich auch andere Synchronisations- oder Versionskontrollwege möglich sind.

**Für SASD Notes:**  
V1 sollte mindestens lokale Sicherheit mitdenken:

- Autosave
- Snapshots / Recovery
- Dateikonflikt-Erkennung
- Dateisystemänderungen
- klarer Unterschied zwischen „Speichern“, „Recovery“, „Backup“ und „Sync“.

---

## 5.12 Veröffentlichung und Web-Ausgabe

### 5.12.1 Obsidian Publish
Obsidian Publish ist ein offizieller Cloud-Dienst, um Inhalte als Website/Wiki/Dokumentation/Digital Garden zu veröffentlichen.

### 5.12.2 Publikationsmodell
- gezielte Auswahl einzelner Notizen
- Veröffentlichung über Publish-Dialog
- Obsidian kann verlinkte Inhalte beim Veröffentlichen mit berücksichtigen
- Änderungen können aus der App heraus ausgerollt werden

### 5.12.3 Web-spezifische Funktionen
Obsidian Publish bietet laut offizieller Produktseite u. a.:

- Hover Previews
- Graph View
- Stacked Pages
- Backlinks auf der Website
- Navigationsanzeige
- responsive/mobile-freundliche Darstellung

### 5.12.4 Site-Verwaltung
Die Hilfe dokumentiert u. a.:

- Site-Adresse
- Homepage-Datei
- Logo
- Navigation
- Kollaboration an einer Publish-Site

### 5.12.5 Anpassung der Site
Offiziell dokumentiert sind u. a.:

- Custom Domain
- Suchmaschinen-Indizierung erlauben/verbieten
- SEO-Metadaten
- Social-Media-Link-Previews
- Passwortschutz
- Analytics optional
- individuelle Gestaltung über `publish.css`
- teils auch JavaScript-Anpassung laut Produktseite

### 5.12.6 Publish-Sicherheit
Die Hilfe betont:

- nur explizit veröffentlichte Notizen gehen an die Server
- unveröffentlichte Notizen bleiben lokal
- Passwortschutz ist möglich
- standardmäßig keine Besucherdatenerfassung durch Publish selbst
- Site-Betreiber bleiben für Datenschutz/GDPR bei eigener Analytics-Anpassung verantwortlich

### 5.12.7 Headless Publish
Obsidian Publish besitzt laut offizieller Hilfe einen headless Client für automatisierte Workflows und CI-Pipelines.

**Für SASD Notes:**  
Ein kompletter Publish-Dienst ist später. Aber die Idee, aus demselben Markdown-Bestand auch HTML/Website zu erzeugen, ist strategisch sehr interessant.

---

## 5.13 Präsentation, Recherche und Arbeitsunterstützung

### 5.13.1 Slides
Mit dem Core Plugin Slides lassen sich Präsentationen direkt aus Markdown-Notizen erzeugen. Slides werden per `---` voneinander getrennt.

### 5.13.2 Web Viewer
Der Web Viewer öffnet externe Links **innerhalb** von Obsidian in Tabs. Die Hilfe beschreibt ausdrücklich:

- Tabs
- Splits
- Pop-out-Fenster
- Nutzung für Web-Recherche
- Einbettung / Öffnung von Canvas-Webkarten als Web-Viewer-Tabs

### 5.13.3 Word Count
Word Count zeigt Wörter und Zeichen der aktiven Notiz an:

- auf Desktop in der Statusleiste
- auf Mobil in der rechten Seitenleiste
- mit Unterstützung auch für CJK-Sprachen

### 5.13.4 Status Bar
Die Status Bar kann Informationen und Aktionen verschiedener Funktionen bündeln, z. B. Word Count oder Sync-Status.

---

## 5.14 Erweiterbarkeit, Automatisierung und Ökosystem

### 5.14.1 Core Plugins
Core Plugins sind mitgelieferte, offiziell unterstützte Funktionen. Einige sind standardmäßig deaktiviert und können gezielt zugeschaltet werden.

### 5.14.2 Community Plugins
Community Plugins erweitern Obsidian um zusätzliche Formate, Dienste, Automationen und Views. Offizielle Hinweise:

- Plugins laufen als Fremdcode
- Community Plugins aktualisieren sich nicht automatisch
- sie werden manuell aktualisiert
- Obsidian warnt vor Drittcode-Risiken
- es gibt laut Sicherheitsdokumentation automatische Prüfungen/Scans und Scorecards für Plugin-Versionen

### 5.14.3 Themes
Themes verändern das gesamte Erscheinungsbild. Laut offizieller Hilfe sind hunderte Community-Themes direkt in der App installierbar.

### 5.14.4 CSS Snippets
CSS Snippets erlauben kleinere Designanpassungen, ohne gleich ein komplettes Theme zu bauen. Änderungen werden nach Speichern automatisch übernommen; ein Neustart ist in der Regel nicht nötig.

### 5.14.5 Obsidian URI
Über `obsidian://...` können Aktionen aus externen Programmen, Skripten und Automationen heraus ausgelöst werden. Dokumentierte Aktionsklassen umfassen u. a.:

- Notiz öffnen
- Notiz erstellen
- Daily Note
- Unique Note
- Suche / weitere Aktionen je nach URI

### 5.14.6 Obsidian CLI
Die offizielle Hilfe dokumentiert eine CLI mit:

- Einzelbefehlen
- TUI
- Autocomplete
- History
- Reverse Search

### 5.14.7 Headless Sync / Headless Publish
Die offiziellen Dokumente zeigen, dass Obsidian sich zunehmend auch für skriptbare und pipelinefähige Szenarien öffnet.

**Für SASD Notes:**  
Ein URI-Schema oder CLI ist für spätere Automatisierung hochinteressant. Für V1 aber nicht nötig.

---

## 5.15 Plattform- und Gerätefunktionen

### 5.15.1 Desktop
Die klassische Obsidian-Erfahrung ist stark desktoporientiert mit:

- Seitenleisten
- Tabs
- Splits
- Drag & Drop
- Statusleiste
- Web Viewer
- umfangreicher Tastaturbedienung

### 5.15.2 Mobile App
Die mobile Hilfe nennt speziell:

- Toolbar
- Quick Action
- Navigation Bar
- Widgets
- plattformspezifische Android-/iOS-Merkmale

### 5.15.3 Android
Für Android nennt die Hilfe u. a.:

- Widgets
- Quick Settings Integration
- Shortcuts
- APK-Download zusätzlich zum Play Store

### 5.15.4 Sandbox Vault
Das Sandbox Vault dient als Lern- und Debugging-Umgebung. Es ist auf Mobil nicht direkt als eingebaute Sandbox verfügbar, kann aber aus dem Help-Repository bezogen werden.

---

## 6. Vollständige Liste der offiziell dokumentierten Core Plugins (Stand der Recherche)

Die folgende Liste basiert auf der offiziellen Core-Plugins-Seite und den Einzelhilfen.

### 6.1 Audio recorder
Zeichnet Audio direkt in einer Notiz auf und speichert die Aufnahme im Vault. Geeignet für Meetings, Vorlesungen, Sprachmemos.

### 6.2 Backlinks
Zeigt eingehende Verweise auf die aktive Notiz sowie weitere Erwähnungen/Analysesichten abhängig von Kontext und Einstellungen.

### 6.3 Bases
Erzeugt datenbankartige Ansichten über Markdown-Dateien und Properties. Unterstützt Filtern, Sortieren, Editieren, Formeln und mehrere View-Typen.

### 6.4 Bookmarks
Erstellt Schnellzugriffe auf Dateien, Ordner, Suchen, Überschriften, Blöcke, Graphen und Links.

### 6.5 Canvas
Bietet eine unendliche visuelle 2D-Arbeitsfläche zur Anordnung und Verbindung von Notizen, Medien und Webseiten.

### 6.6 Command palette
Erlaubt das Ausführen beliebiger Befehle per Tastatur; unterstützt fuzzy matching und pinnbare Befehle.

### 6.7 Daily notes
Öffnet oder erzeugt die Notiz zum aktuellen Datum.

### 6.8 File explorer
Verwaltet Dateien und Ordner im Vault: anlegen, umbenennen, verschieben, löschen, sortieren, auto-reveal, expand/collapse.

### 6.9 File recovery
Erzeugt Snapshots als Schutz gegen unbeabsichtigte Datenverluste.

### 6.10 Footnotes view
Listet alle Fußnoten der aktiven Notiz und erleichtert Navigation und Bearbeitung.

### 6.11 Format converter
Konvertiert Markdown aus anderen Programmen in Obsidian-kompatibles Markdown.

### 6.12 Graph view
Visualisiert Notizen und Links als Netzwerkgraph.

### 6.13 Note composer
Unterstützt das Zusammenführen zweier Notizen und das Herauslösen von Teilen einer Notiz in eine neue Notiz. Aktualisiert dabei Verweise.

### 6.14 Outgoing links
Zeigt alle ausgehenden Links der aktiven Notiz und potenzielle weitere Linkziele.

### 6.15 Outline
Zeigt die Überschriftenstruktur der aktiven Notiz und unterstützt Navigation sowie Reorganisation.

### 6.16 Page preview
Zeigt eine Vorschau verlinkter Inhalte beim Hover, ohne die aktuelle Notiz verlassen zu müssen.

### 6.17 Properties view
Bietet Seitenleisten für die Properties der aktiven Notiz und für alle Properties im Vault.

### 6.18 Publish
Integriert die Veröffentlichung ausgewählter Notizen als Website.

### 6.19 Quick switcher
Schnelle Tastaturnavigation zum Öffnen oder Erzeugen von Notizen.

### 6.20 Random note
Öffnet zufällig eine Notiz im Vault.

### 6.21 Search
Leistungsfähige Volltext- und Struktur-Suche mit Operatoren, RegEx und Property-Abfragen.

### 6.22 Slash commands
Editornahe Befehlsausführung per `/`.

### 6.23 Slides
Erstellt Präsentationen aus Markdown-Notizen.

### 6.24 Sync
Synchronisiert Vaults und Einstellungen geräteübergreifend über den offiziellen Dienst.

### 6.25 Tags view
Listet Tags und Häufigkeiten im Vault.

### 6.26 Templates
Fügt vordefinierte Inhalte und Datums-/Zeitvariablen in Notizen ein.

### 6.27 Unique note creator
Erzeugt Notizen mit zeitbasierten, eindeutigen Namen.

### 6.28 Web viewer
Öffnet externe Webseiten in Obsidian selbst.

### 6.29 Word count
Zeigt Wort- und Zeichenanzahl der aktiven Notiz.

### 6.30 Workspaces
Speichert und lädt komplette Layouts/Arbeitsumgebungen.

---

## 7. Weitere von Obsidian offiziell gepflegte Erweiterungen/Werkzeuge

### 7.1 Web Clipper
Offizielle Browser-Erweiterung zum Erfassen und Strukturieren von Webinhalten im Vault.

### 7.2 CLI
Offizielle Kommandozeilen-Schnittstelle zum Arbeiten mit einer laufenden Obsidian-Instanz.

### 7.3 Importer
Offizielles Community-Plugin des Obsidian-Teams für Migration aus anderen Programmen.

### 7.4 Maps
Offiziell gepflegtes Community-Plugin, das Map-Views für Bases ermöglicht.

---

## 8. Was davon für SASD Notes fachlich am wichtigsten ist

Wenn das Ziel **„Obsidian in C# nachprogrammieren, zunächst für verlinkte Markdown-Dateien, Anzeige und Suche“** ist, dann sind diese Funktionsgruppen die eigentliche Kernmenge:

### 8.1 Unverzichtbarer Kern für V1
1. Vault als Ordner
2. Markdown-Dateien laden/speichern
3. Dateibaum
4. Notiz öffnen/bearbeiten
5. interne Wiki-Links
6. Link-Auflösung
7. Backlinks
8. Volltextsuche
9. Quick Switcher-ähnliche Navigation
10. Properties-Grundlage
11. automatische Link-Aktualisierung bei Rename
12. mehrere geöffnete Tabs / Splits
13. Attachments-Grundfunktion
14. einfache Preview
15. Recovery/Autosave-Basis

### 8.2 Sehr sinnvoll für V1.1 / V2
1. Outline
2. Outgoing Links
3. Page Preview
4. Bookmarks
5. Templates
6. Daily Notes
7. Workspaces
8. Word Count
9. Tags View
10. Properties View
11. URI-/Automation-Basis

### 8.3 Späterer Ausbau
1. Graph View
2. Canvas
3. Slides
4. Web Viewer
5. Audio Recorder
6. Publish
7. Sync-Dienst
8. Bases / `.base`
9. CLI
10. Headless Automation

---

## 9. Priorisierte Ableitung als SASD-Notes-Roadmap auf Funktionsbasis

## Phase A – Dateibasierte Wissensnotizen
- Vault öffnen
- Dateibaum
- Notizeditor
- Speichern
- Neuanlage
- Rename/Delete/Move
- Attachments-Basis
- mehrere Tabs

## Phase B – Wissensvernetzung
- Wiki-Link-Parser
- Link-Navigation
- Backlinks
- Outgoing Links
- unaufgelöste Links
- Link-Update bei Rename
- Alias-Basis

## Phase C – Wiederfinden und Struktur
- Volltextsuche
- Quick Switcher
- Outline
- Bookmarks
- Tags
- Properties-Grundsystem

## Phase D – Komfort und Sicherheit
- Autosave
- Recovery/Snapshots
- Workspaces
- Templates
- Daily Notes
- Page Preview
- Word Count

## Phase E – visuelle und strukturierte Sichten
- Graph View
- Canvas
- Properties View
- datenbankartige Views / Bases-ähnliche Ansichten

## Phase F – Veröffentlichung und Automatisierung
- Export HTML/PDF
- Publish-ähnliche Ausgabe
- URI-Schema
- CLI
- später Sync

---

## 10. Fazit

Obsidian ist funktional **deutlich mehr** als ein Markdown-Editor. Die offizielle Dokumentation zeigt ein System mit mehreren Schichten:

1. **Dateibasierter Markdown-Kern**
2. **Wissensvernetzung durch Links**
3. **starke Wiederfindung per Suche, Backlinks und Switcher**
4. **strukturierte Metadaten über Properties**
5. **visuelle und datenbankartige Sichten (Graph, Canvas, Bases)**
6. **Erweiterbarkeit, Automatisierung und Zusatzdienste**
7. **lokale Datenhoheit bei gleichzeitiger Web-/Sync-Erweiterbarkeit**

Für **SASD Notes** heißt das:  
Ein sinnvoller Nachbau sollte nicht versuchen, sofort alles zu imitieren. Der richtige Weg ist:

- erst **stabiler lokaler Markdown-Kern**,
- dann **Linknetz + Suche + Navigation**,
- dann **Properties + Workspaces + Komfort**,
- erst danach **Graph, Canvas, Bases, Publish, Sync**.

So bleibt das Projekt realistisch, nachvollziehbar und technisch sauber.

---

## 11. Quellenbasis (offizielle Quellen, Stand der Recherche)

Die folgende Liste enthält die maßgeblichen offiziellen Quellen, auf denen dieses Heft basiert.

- [Q1] Obsidian Help – Home  
  https://obsidian.md/help/Home

- [Q2] Obsidian Help – Create a vault  
  https://obsidian.md/help/vault

- [Q3] Obsidian Help – How Obsidian stores data  
  https://obsidian.md/help/data-storage

- [Q4] Obsidian Help – Accepted file formats  
  https://obsidian.md/help/file-formats

- [Q5] Obsidian Help – Internal links  
  https://obsidian.md/help/links

- [Q6] Obsidian Help – Properties  
  https://obsidian.md/help/properties

- [Q7] Obsidian Help – Tags  
  https://obsidian.md/help/tags

- [Q8] Obsidian Help – Attachments  
  https://obsidian.md/help/attachments

- [Q9] Obsidian Help – Basic formatting syntax  
  https://obsidian.md/help/syntax

- [Q10] Obsidian Help – Callouts  
  https://obsidian.md/help/callouts

- [Q11] Obsidian Help – Core plugins  
  https://obsidian.md/help/plugins

- [Q12] Obsidian Help – File explorer  
  https://obsidian.md/help/plugins/file-explorer

- [Q13] Obsidian Help – Search  
  https://obsidian.md/help/plugins/search

- [Q14] Obsidian Help – Quick switcher  
  https://obsidian.md/help/plugins/quick-switcher

- [Q15] Obsidian Help – Command palette  
  https://obsidian.md/help/plugins/command-palette

- [Q16] Obsidian Help – Workspaces  
  https://obsidian.md/help/plugins/workspaces

- [Q17] Obsidian Help – Backlinks  
  https://obsidian.md/help/plugins/backlinks

- [Q18] Obsidian Help – Outgoing links  
  https://obsidian.md/help/plugins/outgoing-links

- [Q19] Obsidian Help – Page preview  
  https://obsidian.md/help/plugins/page-preview

- [Q20] Obsidian Help – Outline  
  https://obsidian.md/help/plugins/outline

- [Q21] Obsidian Help – Bookmarks  
  https://obsidian.md/help/plugins/bookmarks

- [Q22] Obsidian Help – Random note  
  https://obsidian.md/help/plugins/random-note

- [Q23] Obsidian Help – Tags view  
  https://obsidian.md/help/plugins/tags

- [Q24] Obsidian Help – Properties view  
  https://obsidian.md/help/plugins/properties

- [Q25] Obsidian Help – Templates  
  https://obsidian.md/help/plugins/templates

- [Q26] Obsidian Help – Daily notes  
  https://obsidian.md/help/plugins/daily-notes

- [Q27] Obsidian Help – Unique note creator  
  https://obsidian.md/help/plugins/unique-note

- [Q28] Obsidian Help – Audio recorder  
  https://obsidian.md/help/plugins/audio-recorder

- [Q29] Obsidian Help – Footnotes view  
  https://obsidian.md/help/plugins/footnotes

- [Q30] Obsidian Help – Graph view  
  https://obsidian.md/help/plugins/graph

- [Q31] Obsidian Help – Canvas  
  https://obsidian.md/help/plugins/canvas

- [Q32] Obsidian Help – Note composer  
  https://obsidian.md/help/plugins/note-composer

- [Q33] Obsidian Help – Slash commands  
  https://obsidian.md/help/plugins/slash-commands

- [Q34] Obsidian Help – Slides  
  https://obsidian.md/help/plugins/slides

- [Q35] Obsidian Help – Web viewer  
  https://obsidian.md/help/plugins/web-viewer

- [Q36] Obsidian Help – Word count  
  https://obsidian.md/help/plugins/word-count

- [Q37] Obsidian Help – Introduction to Bases  
  https://obsidian.md/help/bases

- [Q38] Obsidian Help – Views  
  https://obsidian.md/help/bases/views

- [Q39] Obsidian Help – Table view  
  https://obsidian.md/help/bases/views/table

- [Q40] Obsidian Help – Cards view  
  https://obsidian.md/help/bases/views/cards

- [Q41] Obsidian Help – List view  
  https://obsidian.md/help/bases/views/list

- [Q42] Obsidian Help – Map view  
  https://obsidian.md/help/bases/views/map

- [Q43] Obsidian Help – Functions  
  https://obsidian.md/help/bases/functions

- [Q44] Obsidian Help – Bases syntax  
  https://obsidian.md/help/bases/syntax

- [Q45] Obsidian Help – Obsidian Sync  
  https://obsidian.md/help/sync

- [Q46] Obsidian Help – Sync security and privacy  
  https://obsidian.md/help/sync/security

- [Q47] Obsidian Help – Sync settings and selective syncing  
  https://obsidian.md/help/sync/settings

- [Q48] Obsidian Help – Version history  
  https://obsidian.md/help/sync/version-history

- [Q49] Obsidian Help – Set up Obsidian Sync  
  https://obsidian.md/help/sync/setup

- [Q50] Obsidian Help – Back up your Obsidian files  
  https://obsidian.md/help/backup

- [Q51] Obsidian Help – Introduction to Obsidian Publish  
  https://obsidian.md/help/publish

- [Q52] Obsidian Help – Publish your content  
  https://obsidian.md/help/publish/publish

- [Q53] Obsidian Help – Manage sites  
  https://obsidian.md/help/publish/sites

- [Q54] Obsidian Help – Customize your site  
  https://obsidian.md/help/publish/customize

- [Q55] Obsidian Help – Publish security and privacy  
  https://obsidian.md/help/publish/security

- [Q56] Obsidian Help – SEO  
  https://obsidian.md/help/publish/seo

- [Q57] Obsidian Help – Custom domains  
  https://obsidian.md/help/publish/domains

- [Q58] Obsidian Help – Headless Publish  
  https://obsidian.md/help/publish/headless

- [Q59] Obsidian product page – Obsidian Publish  
  https://obsidian.md/publish

- [Q60] Obsidian product page – Obsidian Sync  
  https://obsidian.md/sync

- [Q61] Obsidian Help – Web Clipper  
  https://obsidian.md/help/web-clipper

- [Q62] Obsidian Help – Web Clipper Templates  
  https://obsidian.md/help/web-clipper/templates

- [Q63] Obsidian Help – Obsidian CLI  
  https://obsidian.md/help/cli

- [Q64] Obsidian Help – Obsidian URI  
  https://obsidian.md/help/uri

- [Q65] Obsidian Help – Community plugins  
  https://obsidian.md/help/community-plugins

- [Q66] Obsidian Help – Themes  
  https://obsidian.md/help/themes

- [Q67] Obsidian Help – Appearance  
  https://obsidian.md/help/appearance

- [Q68] Obsidian Help – CSS snippets  
  https://obsidian.md/help/snippets

- [Q69] Obsidian Help – Plugin security  
  https://github.com/obsidianmd/obsidian-help/blob/master/en/Extending%20Obsidian/Plugin%20security.md

- [Q70] Obsidian Help – Import notes  
  https://obsidian.md/help/import

- [Q71] Obsidian Help – Importer  
  https://obsidian.md/help/plugins/importer

- [Q72] Obsidian Help – Mobile app  
  https://obsidian.md/help/mobile

- [Q73] Obsidian Help – Obsidian for Android  
  https://obsidian.md/help/android

- [Q74] Obsidian Help – Sandbox vault  
  https://obsidian.md/help/sandbox
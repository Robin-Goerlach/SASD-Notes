# Lastenheft – SASD Notes
Version: 0.1  
Datum: 2026-07-04  
Status: Arbeitsfassung / Projektstart  
Autor: ChatGPT für SASD-GmbH

---

## 1. Dokumentzweck

Dieses Lastenheft beschreibt die fachlichen und organisatorischen Anforderungen an **SASD Notes**.  
SASD Notes ist als lokal arbeitende Desktop-Anwendung für Windows geplant und soll einen praxistauglichen, erweiterbaren Notiz- und Wissensmanager nach dem Vorbild moderner Markdown-basierter Wissenswerkzeuge ermöglichen.

Das System soll insbesondere:

- Markdown-Dateien lokal verwalten,
- Notizen durch interne Links miteinander verknüpfen,
- Notizen in einem editorähnlichen Arbeitsmodus komfortabel erstellen und formatieren,
- Themenbereiche in getrennten Ordnern bzw. Verzeichnisstrukturen organisieren,
- Inhalte schnell durchsuchen,
- mittelfristig für spätere Erweiterungen wie Plugin-Mechanismen vorbereitet sein.

Dieses Lastenheft beschreibt **was** das System leisten soll, nicht im Detail **wie** es technisch umgesetzt wird.

---

## 2. Ausgangssituation

Für Wissensarbeit, Projektdokumentation, technische Notizen, Lernunterlagen, Ideen und Referenzsammlungen wird ein lokal kontrollierbares System benötigt, das nicht auf eine zentrale Cloud-Plattform angewiesen ist und auf normalen Dateisystemstrukturen basiert.

Markdown-basierte Systeme haben den Vorteil, dass Inhalte in offenen Textdateien gespeichert werden und damit langfristig lesbar, portierbar, versionsverwaltbar und mit anderen Werkzeugen weiterverarbeitbar bleiben. Eine Verlinkung zwischen Notizen ermöglicht dabei ein netzartiges Wissensmodell, das über reine Ordnerstrukturen hinausgeht.

Vorhandene Referenzprodukte zeigen, dass insbesondere folgende Eigenschaften im Markt als wertvoll gelten:

- lokale Vault-/Ordner-basierte Datenhaltung,
- interne Links zwischen Notizen,
- Dateiexplorer und Suchfunktionen,
- strukturierte Metadaten/Properties,
- Editor- und Leseansichten,
- Tagging, Graph-/Beziehungsansichten und Erweiterbarkeit durch Plugins.

SASD Notes soll diese Grundidee aufgreifen, aber zunächst bewusst mit einem klar beherrschbaren V1-Umfang starten.

---

## 3. Projektziel

### 3.1 Oberziel

Entwicklung einer lokal arbeitenden Windows-Desktop-Anwendung, mit der Benutzer Markdown-Notizen in frei wählbaren Ordnerstrukturen verwalten, verlinken, durchsuchen und bearbeiten können.

### 3.2 Teilziele

SASD Notes soll:

1. einen oder mehrere lokale Notizbestände („Vaults“ bzw. thematische Ordnerbestände) nutzbar machen,
2. Markdown-Dateien komfortabel erstellen, öffnen, bearbeiten und speichern,
3. interne Links zwischen Notizen erzeugen und auflösen,
4. Notizen in einer gut navigierbaren Benutzeroberfläche darstellen,
5. schnelle Suche über Dateinamen, Inhalte, Tags und Metadaten ermöglichen,
6. eine saubere Grundlage für spätere Erweiterungen schaffen,
7. ohne Plugin-System bereits als eigenständig nützliches Produkt funktionieren.

---

## 4. Begriffe

### 4.1 Vault
Ein Vault ist ein lokaler Wurzelordner, in dem Markdown-Dateien sowie zugehörige Anhänge und Konfigurationsdaten gespeichert werden.

### 4.2 Notiz
Eine Notiz ist eine einzelne Markdown-Datei, in der Inhalte, Formatierungen, Links und optional strukturierte Metadaten enthalten sind.

### 4.3 Interner Link / Wiki-Link
Ein interner Link ist eine Verknüpfung zu einer anderen Notiz oder zu einem anderen Objekt innerhalb des Vaults. Unterstützt werden sollen insbesondere Wiki-Link-Formate wie `[[Notizname]]`.

### 4.4 Property / Metadatum
Strukturierte Zusatzinformationen zu einer Notiz, beispielsweise Status, Kategorie, Datum, Tags oder Referenzen.

### 4.5 Themenordner
Verzeichnisse innerhalb eines Vaults zur organisatorischen Trennung von Themenkomplexen wie Projekt, Privat, Forschung, Dokumentation, Archiv oder Referenzen.

---

## 5. Stakeholder

### 5.1 Auftraggeber / Betreiber
- SASD-GmbH

### 5.2 Primäre Benutzer
- technisch orientierte Einzelanwender
- Wissensarbeiter
- Entwickler, Administratoren, Dokumentationsverantwortliche
- Benutzer mit Bedarf an lokal kontrollierter Wissensablage

### 5.3 Sekundäre Benutzer
- spätere Mitnutzer innerhalb kleiner Teams
- Personen, die exportierte oder gemeinsam gepflegte Markdown-Bestände verwenden

### 5.4 Technische Stakeholder
- Entwickler der Desktop-Anwendung
- spätere Entwickler möglicher Erweiterungen / Plugins / Integrationen

---

## 6. Abgrenzung des Projektumfangs

### 6.1 Im Fokus der ersten Produktversion
- lokale Markdown-Verwaltung
- interne Verlinkung
- editornahe Bearbeitung
- Ordner-/Dateiverwaltung
- Suche
- Grundnavigation
- grundlegende Metadaten
- gute Benutzbarkeit im Desktop-Kontext

### 6.2 Vorläufig nicht im Fokus von V1
- vollständiges Plugin-System
- Cloud-Synchronisation als Pflichtbestandteil
- kollaboratives Mehrbenutzer-Editing in Echtzeit
- vollwertiger Publish-/Website-Generator
- mobile Apps
- komplexe Datenbankfunktionen
- sehr große Marketplace- oder Theme-Ökosysteme
- vollständige Nachbildung aller Spezialfunktionen eines Referenzprodukts

### 6.3 Später zu berücksichtigen
- Plugin-/Erweiterungsmodell
- Import-/Export-Assistenten
- Diagramm-/Graph-Darstellungen
- Templates, Automationen, Skripting
- optionale Verschlüsselung / sichere Bereiche
- optionale Datenbank-/Index-Unterstützung für große Bestände

---

## 7. Produkt-Einsatz

### 7.1 Einsatzbereich
SASD Notes soll als lokale Desktop-Anwendung unter Windows eingesetzt werden.

### 7.2 Einsatzkontext
Der Benutzer arbeitet überwiegend mit lokalen Dateien in einem oder mehreren Ordnerbeständen. Die Anwendung soll für technische Dokumentation, Projektnotizen, Wissenssammlungen, Lerninhalte und strukturierte Referenzablagen geeignet sein.

### 7.3 Betriebsart
- primär lokal/offline
- ohne zwingende Cloud-Anbindung
- mit normalem Dateisystemzugriff
- für Einzelplatzbetrieb optimiert
- später optional erweiterbar

---

## 8. Rahmenbedingungen und Randbedingungen

### 8.1 Fachliche Randbedingungen
- Die Daten sollen in offenen Dateiformaten gespeichert werden.
- Markdown ist das Primärformat für Notizinhalte.
- Die Verlinkung zwischen Notizen ist Kernfunktion und keine optionale Zusatzfunktion.
- Die Anwendung soll sowohl einfache Notizen als auch strukturierte Wissensnetze unterstützen.

### 8.2 Organisatorische Randbedingungen
- V1 soll überschaubar, verständlich und wartbar bleiben.
- Funktionsumfang und Architektur sollen später erweiterbar sein.
- Das Plugin-System soll berücksichtigt, aber nicht in V1 umgesetzt werden.

### 8.3 Technische Randbedingungen
- Zielplattform: Windows-Desktop
- Entwicklungs- und Laufzeitkontext: .NET 8 / Windows-kompatible Desktop-Technologie
- Lokaler Dateisystemzugriff muss unterstützt werden.
- Die Anwendung muss mit normalen Ordnerstrukturen und Markdown-Dateien arbeiten können.
- Die Anwendung soll auch mit mehreren thematisch getrennten Verzeichnissen/Vaults umgehen können.
- Ein Menüeintrag zum Öffnen bzw. Wechseln thematischer Ordner/Vaults ist erforderlich.

### 8.4 Qualitätsrandbedingungen
- stabile Dateiverarbeitung
- nachvollziehbare Benutzerführung
- verständliche Fehlerbehandlung
- gute Lesbarkeit und Wartbarkeit
- keine unnötige technische Komplexität in V1

---

## 9. Gesamtvision des Produkts

SASD Notes soll ein lokales Wissenswerkzeug werden, das die Stärken von Markdown, Dateisystemtransparenz und interner Verlinkung kombiniert. Die Anwendung soll sich wie ein professioneller Editor anfühlen, gleichzeitig aber konzeptionell als Wissenssystem funktionieren.

Der Benutzer soll:

- Notizen schnell anlegen,
- sie angenehm formatieren,
- sie miteinander verknüpfen,
- sie thematisch in Ordnern organisieren,
- Beziehungen zwischen Inhalten erkennen,
- relevante Informationen schnell wiederfinden.

---

## 10. Muss-, Soll- und Kann-Anforderungen – Übersicht

### 10.1 Muss-Anforderungen
Diese Anforderungen sind für V1 zwingend erforderlich:

1. Lokale Vault-/Ordnerverwaltung
2. Öffnen eines bestehenden Ordners als Arbeitsbereich
3. Verwaltung mehrerer thematischer Verzeichnisse/Vaults
4. Dateiexplorer mit Ordner- und Dateiansicht
5. Anlegen, Umbenennen, Löschen und Verschieben von Notizen und Ordnern
6. Markdown-Editor
7. Formatierungshilfen im Editor
8. Interne Verlinkung zwischen Markdown-Dateien
9. Auflösung interner Links beim Öffnen / Navigieren
10. Suchfunktion über Dateien und Inhalte
11. Speichern von Änderungen
12. Erkennung externer Dateiveränderungen
13. Status- und Fehlerrückmeldung
14. Grundlegende Metadaten/Properties
15. Saubere Behandlung nicht auflösbarer Links
16. Windows-taugliche Desktop-Bedienung
17. Vorbereitung der Architektur auf spätere Erweiterbarkeit

### 10.2 Soll-Anforderungen
Diese Anforderungen sind für V1 sehr wünschenswert:

1. Editor- und Vorschauansicht
2. Backlinks
3. Tag-Unterstützung
4. Zuletzt geöffnete Vaults / Ordner
5. Suchfilter
6. Schnellnavigation / Schnellöffnen
7. Tabs oder Verlauf für geöffnete Notizen
8. Einfache Vorlagen/Templates
9. Import vorhandener Markdown-Bestände per Ordnerübernahme
10. Anpassbare Oberfläche / Theme-Grundlagen

### 10.3 Kann-Anforderungen
Diese Anforderungen sind für spätere Versionen geeignet:

1. Graphansicht
2. Canvas-/Whiteboard-Ansatz
3. erweiterte Tabellen-/Datenansichten
4. Plugin-System
5. Kommando-Palette
6. Hotkey-Verwaltung
7. lokale Versionierung / Historie
8. Synchronisationsoptionen
9. Publikations-/Exportfunktionen
10. API / Skripting / Automatisierung

---

## 11. Funktionale Anforderungen im Detail

## FA-01 Vault- und Ordnerverwaltung

### Ziel
Der Benutzer muss mit lokalen Ordnerbeständen arbeiten können, die als Wissensräume dienen.

### Beschreibung
Die Anwendung muss einen lokalen Ordner als Arbeitsbereich öffnen können. Dieser Ordner bildet die Wurzel des aktiven Wissensbestands. Zusätzlich soll der Benutzer zwischen mehreren thematisch getrennten Ordnern/Vaults wechseln können.

### Muss-Inhalte
- Ordner als Arbeitsbereich öffnen
- neue Arbeitsbereiche/Vaults anlegen
- Liste zuletzt verwendeter Arbeitsbereiche
- aktiven Arbeitsbereich schließen
- zwischen Arbeitsbereichen wechseln
- Menüeintrag zum Öffnen bzw. Wechseln eines thematischen Ordners/Vaults
- saubere Behandlung nicht verfügbarer oder verschobener Ordner

### Akzeptanzkriterien
- Ein Benutzer kann einen beliebigen lokalen Ordner öffnen.
- Nach dem Öffnen werden unterstützte Inhalte sichtbar.
- Die Anwendung merkt sich zuletzt verwendete Ordner.
- Der Benutzer kann zwischen mehreren Themenordnern/Vaults wechseln, ohne Datenverlust zu riskieren.

---

## FA-02 Dateiexplorer / Navigationsbereich

### Ziel
Der Benutzer muss Dateien und Verzeichnisse bequem durchsuchen und verwalten können.

### Beschreibung
Ein linker Navigationsbereich soll die Verzeichnisstruktur des aktiven Arbeitsbereichs abbilden. Der Benutzer soll Ordner ein- und ausklappen, Dateien auswählen und typische Dateivorgänge ausführen können.

### Muss-Inhalte
- Baumansicht für Ordner und Dateien
- Erkennen von Markdown-Dateien
- Öffnen per Klick oder Doppelklick
- Kontextmenü für Datei-/Ordneroperationen
- neue Datei anlegen
- neuen Ordner anlegen
- Umbenennen
- Löschen
- Verschieben innerhalb des Arbeitsbereichs
- Aktualisierung nach Dateisystemänderungen

### Soll-Inhalte
- Drag & Drop innerhalb des Explorers
- Anzeige zusätzlicher Dateitypen
- Favoriten / Bookmarks

### Akzeptanzkriterien
- Der Benutzer kann eine Datei im Explorer auswählen und öffnen.
- Neue Dateien und Ordner sind im Explorer direkt sichtbar.
- Strukturänderungen werden korrekt reflektiert.

---

## FA-03 Markdown-Editor

### Ziel
Der Benutzer muss Notizen in Markdown komfortabel erstellen, bearbeiten und formatieren können.

### Beschreibung
Der zentrale Arbeitsbereich soll ein editorähnliches Arbeiten ermöglichen. Die Notiz muss als Text bearbeitet werden können, wobei Markdown-Formatierungen schnell eingegeben oder unterstützt werden sollen.

### Muss-Inhalte
- Textbearbeitung für Markdown-Dateien
- Speichern
- automatische Markierung ungespeicherter Änderungen
- Unterstützung mehrzeiliger Bearbeitung
- grundlegende Bearbeitungsfunktionen wie Kopieren, Ausschneiden, Einfügen, Rückgängig, Wiederholen
- Zeilenumbrüche, Listen, Überschriften, Codeblöcke, Zitate, Tabellen, Kontrollkästchen
- Einfügen interner Links
- gut lesbare Schrift- und Editoransicht

### Soll-Inhalte
- Toolbar oder Kontextfunktionen für Formatierungen
- Tastenkürzel für häufige Markdown-Elemente
- Umschaltung zwischen Bearbeiten und Vorschau
- Syntax-Hervorhebung
- automatische Einrückung / Listenfortsetzung

### Akzeptanzkriterien
- Ein Benutzer kann eine Notiz neu erstellen und Markdown formatiert eingeben.
- Änderungen lassen sich speichern und erneut öffnen.
- Der Editor ist für längere Texte praktisch nutzbar.

---

## FA-04 Markdown-Formatierungshilfen

### Ziel
Markdown soll nicht nur als Rohtext, sondern mit editornahen Komfortfunktionen bearbeitbar sein.

### Beschreibung
Die Anwendung soll den Benutzer bei häufigen Formatierungen unterstützen, damit Markdown auch für längere Wissensarbeit alltagstauglich bleibt.

### Muss-Inhalte
- Eingabe von Überschriften
- Fett/Kursiv/Code
- Listen und Aufgabenlisten
- Links und Bilder
- Tabellen als Textformat
- Blockquotes
- Codeblöcke

### Soll-Inhalte
- Toolbar-Schaltflächen
- kontextsensitive Einfügefunktionen
- Tastenkombinationen
- Vorschau auf Formatierung
- automatische Markdown-Erzeugung bei Befehlen

### Akzeptanzkriterien
- Ein Benutzer kann ohne externen Editor eine brauchbar formatierte Markdown-Notiz erstellen.
- Häufige Formatierungen sind mit geringer Bedienhürde möglich.

---

## FA-05 Interne Verlinkung zwischen Notizen

### Ziel
Die interne Verlinkung ist Kernfunktion des Systems.

### Beschreibung
Benutzer müssen Notizen einfach miteinander verknüpfen können. Das System soll vor allem Wiki-Link-Notation unterstützen. Interne Links sollen sowohl beim Schreiben als auch beim späteren Navigieren zuverlässig funktionieren.

### Muss-Inhalte
- Unterstützung von `[[Notizname]]`
- Unterstützung von `[[Pfad/Notizname]]`
- Unterstützung von Aliassen wie `[[Notizname|Anzeigetext]]`
- Navigation beim Anklicken eines Links
- Auflösung der Zielnotiz anhand Name/Pfad
- Kennzeichnung nicht auflösbarer Links
- Möglichkeit, aus einem nicht auflösbaren Link eine neue Notiz zu erzeugen
- Aktualisierung von Links bei Umbenennung/Verschiebung, soweit möglich

### Soll-Inhalte
- Autovervollständigung beim Eingeben von Links
- Anzeige möglicher Zielnotizen
- Verlinkung zu Überschriften oder Abschnitten
- Einbettung anderer Inhalte

### Akzeptanzkriterien
- Ein Benutzer kann eine Notiz per Wiki-Link mit einer anderen verbinden.
- Ein Klick auf einen gültigen internen Link öffnet die Zielnotiz.
- Nicht vorhandene Ziele werden sichtbar als offen/unaufgelöst behandelt.

---

## FA-06 Link-Integrität und Umbenennungslogik

### Ziel
Verlinkungen sollen auch nach Strukturänderungen möglichst stabil bleiben.

### Beschreibung
Wenn der Benutzer Dateien umbenennt oder verschiebt, sollen interne Verweise möglichst automatisch oder zumindest kontrolliert aktualisiert werden.

### Muss-Inhalte
- Erkennen von Datei-/Pfadänderungen
- Prüfung betroffener interner Links
- Vermeidung stiller Linkzerstörung
- Benutzerhinweis bei Konflikten oder Mehrdeutigkeit

### Soll-Inhalte
- automatische Linkanpassung
- Protokollierung geänderter Referenzen
- Vorschau geplanter Linkänderungen

### Akzeptanzkriterien
- Nach einer einfachen Umbenennung bleiben Links möglichst funktionsfähig oder der Benutzer wird klar informiert.
- Es entstehen keine unbemerkten Massenfehler.

---

## FA-07 Suche

### Ziel
Benutzer müssen Informationen schnell wiederfinden können.

### Beschreibung
Das System muss eine Suche über Dateinamen, Inhalte und – soweit vorhanden – Metadaten ermöglichen.

### Muss-Inhalte
- Suchfeld in der Oberfläche
- Volltextsuche über Markdown-Inhalte
- Suche nach Dateinamen/Titeln
- Trefferliste mit Kontextausschnitten
- Öffnen der Zielnotiz aus den Suchergebnissen

### Soll-Inhalte
- Suche nach Tags
- Suche nach Properties/Metadaten
- Suchoperatoren / Filter
- Hervorhebung des Treffers
- inkrementelle Suche während der Eingabe
- Sortierung und Eingrenzung

### Akzeptanzkriterien
- Ein Benutzer kann einen Begriff suchen und relevante Notizen finden.
- Treffer führen direkt zur Zielnotiz.

---

## FA-08 Backlinks und Beziehungsansicht

### Ziel
Der Benutzer soll nachvollziehen können, welche Notizen auf die aktuelle Notiz verweisen.

### Beschreibung
Neben ausgehenden Links sollen auch eingehende Verknüpfungen sichtbar gemacht werden.

### Soll-Inhalte
- Liste aller Notizen, die auf die aktuelle Notiz verweisen
- Anzeige im Seitenpanel
- einfache Kontextanzeige

### Akzeptanzkriterien
- Der Benutzer erkennt, welche anderen Notizen auf die aktive Notiz Bezug nehmen.

---

## FA-09 Metadaten / Properties

### Ziel
Notizen sollen mit strukturierten Zusatzinformationen versehen werden können.

### Beschreibung
Neben dem Fließtext sollen Metadaten gepflegt werden können, beispielsweise Typ, Status, Kategorie, Datum, Quelle oder Priorität.

### Muss-Inhalte
- Unterstützung einfacher strukturierter Properties
- Bearbeiten grundlegender Property-Typen
- Sichtbarkeit der Properties pro Datei
- Speichern gemeinsam mit der Notiz

### Soll-Inhalte
- Text, Zahl, Datum, Boolesch, Link, Mehrfachwerte
- Property-Anzeige im Seitenbereich
- Property-basierte Suche / Filterung
- Property-Vorlagen

### Akzeptanzkriterien
- Ein Benutzer kann einer Notiz strukturierte Metadaten zuordnen.
- Diese Metadaten bleiben beim erneuten Öffnen erhalten.

---

## FA-10 Tags

### Ziel
Notizen sollen zusätzlich zur Ordnerstruktur thematisch markierbar sein.

### Beschreibung
Benutzer sollen Schlagwörter für flexible Querstrukturen nutzen können.

### Soll-Inhalte
- Inline-Tags
- strukturierte Tag-Liste
- Anzeige vorhandener Tags
- Tag-basierte Suche
- verschachtelte Tags

### Akzeptanzkriterien
- Ein Benutzer kann Notizen mit Tags versehen und danach suchen.

---

## FA-11 Vorschau / Leseansicht

### Ziel
Markdown-Inhalte sollen nicht nur editierbar, sondern auch angenehm lesbar dargestellt werden.

### Beschreibung
Zusätzlich zur Textbearbeitung soll eine Vorschau- oder Leseansicht bereitstehen, in der Formatierungen visuell gerendert dargestellt werden.

### Soll-Inhalte
- Umschaltung Editieren / Lesen
- Anzeige von Überschriften, Listen, Codeblöcken, Tabellen, Links
- klickbare interne und externe Links
- mediale Anhänge soweit unterstützt

### Akzeptanzkriterien
- Eine Notiz kann in lesbarer Form betrachtet werden.
- Links bleiben nutzbar.

---

## FA-12 Anhänge und unterstützte Dateitypen

### Ziel
Neben Markdown sollen auch bestimmte weitere Dateitypen eingebunden werden können.

### Beschreibung
Die Anwendung soll zumindest einen grundlegenden Umgang mit Anhängen und referenzierten Dateien unterstützen.

### Muss-Inhalte
- Referenzierung von Bildern und anderen Dateien innerhalb des Arbeitsbereichs
- Anzeige unterstützter Dateitypen im Explorer
- sauberes Verhalten bei nicht direkt darstellbaren Dateien

### Soll-Inhalte
- Vorschau für Bilder und PDFs
- Anhänge-Ordnerkonzept
- Drag & Drop von Dateien in eine Notiz

### Akzeptanzkriterien
- Referenzierte Dateien werden nicht „unsichtbar“ verwaltet.
- Ein Benutzer kann Anhänge im Projektkontext nachvollziehen.

---

## FA-13 Mehrere Themenverzeichnisse / getrennte Wissensräume

### Ziel
Verschiedene Themenkomplexe sollen getrennt verwaltbar sein.

### Beschreibung
Die Anwendung soll verschiedene Ordnerstrukturen als unterschiedliche Wissensräume behandeln können, etwa „Arbeit“, „Privat“, „Forschung“ oder „Archiv“. Dies kann in Form separater Vaults oder sauber trennbarer Themenordner realisiert werden.

### Muss-Inhalte
- getrennte Arbeitsbereiche bzw. sauber strukturierte Themenordner
- Menüeintrag zum Öffnen oder Wechseln
- klare Anzeige des aktiven Bereichs
- kein unbeabsichtigtes Vermischen der Daten

### Soll-Inhalte
- Startdialog „letzte Arbeitsbereiche“
- einfacher Wechsel ohne Neustart
- eigene Einstellungen pro Arbeitsbereich

### Akzeptanzkriterien
- Ein Benutzer kann unterschiedliche Themenbestände getrennt verwalten.
- Der aktive Bereich ist eindeutig erkennbar.

---

## FA-14 Schnellzugriff und Navigation

### Ziel
Häufig benötigte Inhalte sollen schnell erreichbar sein.

### Beschreibung
Neben Explorer und Suche sind schnelle Navigationsmechanismen sinnvoll, um direkt zu Dateien, zuletzt genutzten Inhalten oder markierten Notizen zu springen.

### Soll-Inhalte
- Zuletzt geöffnet
- Favoriten / Bookmarks
- Schnellöffnen
- Verlauf vor/zurück
- Tabs oder Dokumentverlauf

### Akzeptanzkriterien
- Wiederkehrend genutzte Notizen können schneller erreicht werden als nur über tiefes Durchklicken.

---

## FA-15 Statusanzeigen und Bedienrückmeldung

### Ziel
Der Benutzer muss jederzeit erkennen können, was der aktuelle Zustand der Anwendung ist.

### Beschreibung
Die Oberfläche soll eindeutig anzeigen, ob eine Datei geändert, gespeichert, geladen oder fehlerhaft verarbeitet wurde.

### Muss-Inhalte
- Anzeige des aktiven Dateinamens/Pfads
- Anzeige „geändert/ungespeichert“
- Fehlermeldungen bei Datei- oder Linkproblemen
- Rückmeldung zu Suchergebnissen
- Rückmeldung zu Lade- und Speicheraktionen

### Akzeptanzkriterien
- Der Benutzer verliert nicht unbeabsichtigt Änderungen.
- Probleme werden verständlich und rechtzeitig angezeigt.

---

## FA-16 Einstellungen

### Ziel
Grundlegende Benutzereinstellungen müssen verwaltbar sein.

### Beschreibung
Die Anwendung soll wenigstens eine Basis an konfigurierbaren Einstellungen bereitstellen.

### Muss-Inhalte
- Standardspeicher-/Öffnungsverhalten
- Editor-Grundoptionen
- Dateiverhalten für Links und Umbenennungen

### Soll-Inhalte
- Theme / Hell-Dunkel
- Schriftarten / Schriftgröße
- Verhalten bei Tabs, Startansicht und Explorer
- Standardordner für Anhänge

### Akzeptanzkriterien
- Wichtige Basisoptionen sind ohne Codeänderung einstellbar.

---

## FA-17 Dateisystemänderungen und Parallelzugriff

### Ziel
Externe Änderungen am Notizbestand sollen nachvollziehbar behandelt werden.

### Beschreibung
Da die Daten als offene Dateien vorliegen, können Änderungen auch außerhalb der Anwendung entstehen. SASD Notes muss dies erkennen und sinnvoll behandeln.

### Muss-Inhalte
- Erkennen externer Änderungen
- Erkennen gelöschter oder verschobener Dateien
- Hinweis bei Konflikten
- Neuladen oder Bestätigen ermöglichen

### Akzeptanzkriterien
- Externe Änderungen führen nicht still zu Datenverlust oder veralteten Anzeigen.

---

## FA-18 Import vorhandener Markdown-Bestände

### Ziel
Bestehende Markdown-Sammlungen sollen nutzbar bleiben.

### Beschreibung
Benutzer sollen vorhandene Ordner mit Markdown-Dateien als Arbeitsbereiche übernehmen können, ohne ein proprietäres Importformat zu benötigen.

### Muss-Inhalte
- Öffnen bestehender Ordner
- Übernahme vorhandener Unterordner
- Erkennung vorhandener Markdown-Dateien
- kein Zwang zur internen Datenmigration

### Akzeptanzkriterien
- Ein vorhandener Markdown-Ordner kann direkt genutzt werden.

---

## FA-19 Erweiterbarkeit / Plugin-Vorbereitung

### Ziel
Die erste Version muss noch kein Plugin-System enthalten, soll aber spätere Erweiterungen nicht verbauen.

### Beschreibung
Architektur, Datenhaltung und Benutzerführung sollen so vorbereitet werden, dass spätere Erweiterungspunkte möglich bleiben.

### Muss-Inhalte
- klare Trennung von Oberfläche, Fachlogik und Infrastruktur
- keine unnötige Verflechtung
- definierbare Erweiterungspunkte in späteren Versionen möglich

### Soll-Inhalte
- spätere Plugin-Ladepunkte
- Befehls-/Aktionsmodell
- offene interne Schnittstellen

### Akzeptanzkriterien
- V1 ist ohne Plugin-System lauffähig.
- Eine spätere Erweiterung ist architektonisch nicht ausgeschlossen.

---

## FA-20 Hilfe, Bedienbarkeit und Einstieg

### Ziel
Neue Benutzer sollen die Anwendung schnell verstehen können.

### Beschreibung
Das Produkt soll einen niedrigschwelligen Einstieg ermöglichen.

### Soll-Inhalte
- Startnotiz / Willkommen
- kurze Hilfetexte
- sinnvolle Standardansicht
- klare Menüs und Bezeichnungen

### Akzeptanzkriterien
- Ein neuer Benutzer kann ohne längere Einarbeitung erste Notizen anlegen, verlinken und wiederfinden.

---

## 12. Nichtfunktionale Anforderungen

## NFA-01 Benutzbarkeit
- Die Anwendung soll für tägliche Arbeit mit längeren Texten geeignet sein.
- Häufige Aktionen sollen ohne Umwege erreichbar sein.
- Die Oberfläche soll sachlich, ruhig und professionell wirken.

## NFA-02 Performance
- Normale Vaults mit typischen Wissensbeständen sollen flüssig nutzbar sein.
- Explorer, Öffnen, Speichern und Suche sollen in üblichen Nutzungsszenarien ohne störende Verzögerung reagieren.
- Große Bestände sollen später skalierbar bleiben.

## NFA-03 Zuverlässigkeit
- Speichervorgänge müssen robust sein.
- Datenverlust ist soweit wie möglich zu vermeiden.
- Fehlerzustände müssen kontrolliert behandelt werden.

## NFA-04 Wartbarkeit
- Die Lösung soll klar strukturiert und erweiterbar sein.
- Fachlogik und UI sollen sauber getrennt sein.
- Dokumentation und verständlicher Code sind wichtig.

## NFA-05 Portabilität der Daten
- Die Inhalte sollen in offenen Dateien vorliegen.
- Markdown-Dateien müssen außerhalb der Anwendung nutzbar bleiben.
- Ordnerstrukturen dürfen nicht proprietär versteckt werden.

## NFA-06 Sicherheit
- Keine unnötige Cloud-Abhängigkeit.
- Lokale Daten bleiben lokal, sofern keine spätere Zusatzfunktion anderes vorsieht.
- Dateipfade und Benutzeraktionen sollen nachvollziehbar und kontrolliert verarbeitet werden.

## NFA-07 Erweiterbarkeit
- Spätere Features wie Plugins, Graph, Templates oder Sync sollen nachrüstbar sein.
- V1 darf diese Optionen nicht unnötig erschweren.

## NFA-08 Barrierearmut
- Lesbare Schriftgrößen
- kontrastfähige Darstellung
- Tastaturbedienbarkeit wichtiger Funktionen

---

## 13. Benutzeroberfläche – fachliche Zielvorstellung

Die Benutzeroberfläche soll sich an einer klaren Desktop-Arbeitslogik orientieren.

### Zielbild
- **oben**: Menü / Werkzeuge / Suche
- **links**: Explorer für Vault, Ordner und Dateien
- **Mitte**: Editor bzw. Leseansicht der aktiven Notiz
- **rechts oder unten**: Zusatzbereiche wie Suche, Backlinks, Properties oder Vorschau
- **unten**: Statusleiste

### Wichtige Menüpunkte
- Datei
  - Neuer Arbeitsbereich / Vault
  - Ordner öffnen...
  - Zuletzt verwendete Arbeitsbereiche
  - Notiz neu
  - Ordner neu
  - Speichern
  - Beenden
- Bearbeiten
  - Rückgängig / Wiederholen
  - Ausschneiden / Kopieren / Einfügen
  - Suchen
  - Formatieren
- Ansicht
  - Explorer
  - Vorschau
  - Backlinks
  - Properties
  - Statusleiste
- Navigation
  - Zurück / Vor
  - Schnell öffnen
  - Nächste / vorige Notiz
- Hilfe

---

## 14. Wichtige Nutzungsszenarien

### US-01 Erste Notiz anlegen
Ein Benutzer öffnet einen Arbeitsbereich, erstellt eine neue Markdown-Datei, schreibt Text, formatiert Überschriften und speichert die Notiz.

### US-02 Notizen verlinken
Ein Benutzer schreibt in einer Notiz einen Wiki-Link auf eine andere Notiz und öffnet diese per Klick.

### US-03 Themenbereiche trennen
Ein Benutzer verwaltet getrennte Arbeitsbereiche für unterschiedliche Themen und wechselt zwischen ihnen über einen Menüeintrag.

### US-04 Wissen wiederfinden
Ein Benutzer sucht nach einem Begriff und öffnet aus der Trefferliste die passende Notiz.

### US-05 Ordnerstruktur pflegen
Ein Benutzer legt Unterordner für Projekte oder Referenzen an und verschiebt Notizen dorthin.

### US-06 Bestehende Markdown-Sammlung nutzen
Ein Benutzer wählt einen vorhandenen Markdown-Ordner aus und kann diesen sofort verwenden.

### US-07 Notiz umbenennen
Ein Benutzer benennt eine Datei um und das System behandelt bestehende Links nachvollziehbar.

### US-08 Properties pflegen
Ein Benutzer gibt einer Notiz strukturierte Zusatzinformationen wie Status oder Datum.

---

## 15. Daten und Informationsobjekte

### Primäre Informationsobjekte
- Vault / Arbeitsbereich
- Ordner / Themenordner
- Markdown-Datei / Notiz
- interner Link
- Tag
- Property / Metadatum
- Suchtreffer
- Bookmark / Favorit (optional)
- Anhang / Datei

### Sekundäre Informationsobjekte
- lokale Einstellungen
- Verlauf
- UI-Zustände
- ggf. später Indexdaten / Cache

---

## 16. Priorisierte V1-Funktionsmatrix

### Unbedingt in V1
- Ordner/Vault öffnen
- thematische Trennung über Verzeichnisse/Vaults
- Explorer
- neue Datei / neuer Ordner
- Markdown-Editor
- Speichern
- Wiki-Links
- Link-Navigation
- Suche
- einfache Properties
- Statusanzeigen
- robustes Dateiverhalten

### Wenn realistisch in V1 zusätzlich
- Vorschau
- Backlinks
- Tags
- letzte Arbeitsbereiche
- Schnellöffnen

### Nach V1
- Graph
- Canvas
- Plugin-System
- komplexere Datenansichten
- Sync / Publish / Automationen

---

## 17. Abnahmekriterien auf Produktebene

Das Produkt gilt als fachlich abnahmefähig, wenn mindestens folgende Punkte erfüllt sind:

1. Ein Benutzer kann einen lokalen Ordner als Arbeitsbereich öffnen.
2. Markdown-Dateien und Unterordner werden in einer Navigationsansicht dargestellt.
3. Neue Notizen und Ordner können angelegt werden.
4. Notizen können bearbeitet und gespeichert werden.
5. Interne Links im Wiki-Link-Format können erstellt und genutzt werden.
6. Nicht auflösbare Links werden erkennbar behandelt.
7. Inhalte können per Suche gefunden werden.
8. Mehrere Themenbereiche/Vaults können verwaltet bzw. gewechselt werden.
9. Die Anwendung ist für alltägliche lokale Wissensarbeit praktisch nutzbar.
10. Die V1-Lösung ist so strukturiert, dass spätere Erweiterungen möglich bleiben.

---

## 18. Risiken und offene Punkte

### Risiken
- Zu großer Funktionsumfang in V1
- Übernahme zu vieler Komfortfunktionen auf einmal
- spätere Link- und Dateikonsistenz bei Umbenennen/Verschieben
- Abgrenzung zwischen einfacher V1 und wachsender Obsidian-Ähnlichkeit
- Balance zwischen Editor-Komfort und technischer Einfachheit

### Offene Punkte
- genauer Umfang der ersten Vorschauansicht
- exakte Property-Typen in V1
- Umfang von Drag & Drop
- Umfang von Tabs / Verlauf
- Umfang von Backlink- und Tag-Ansichten
- späteres Plugin-Konzept
- spätere Import-/Export-Funktionen

---

## 19. Empfohlene Projektstrategie

Für SASD Notes wird empfohlen, das Produkt in Stufen zu entwickeln:

### Stufe 1 – Kernsystem
- Vault öffnen
- Explorer
- Editor
- Speichern
- Wiki-Links
- Suche

### Stufe 2 – Wissenskomfort
- Backlinks
- Tags
- Vorschau
- letzte Arbeitsbereiche
- bessere Navigation

### Stufe 3 – Ausbau
- Properties-Ausbau
- Templates
- Graph
- Canvas-nahe Funktionen
- Plugin-Vorbereitung vertiefen

### Stufe 4 – Erweiterungsplattform
- Plugin-System
- Automationen
- Integrationen
- Sync-/Publish-nahe Zusatzfunktionen

---

## 20. Zusammenfassung

SASD Notes soll als lokal kontrollierbares, dateibasiertes Wissenswerkzeug für Windows entstehen. Die Kernidee besteht darin, Markdown-Dateien nicht nur in Ordnern abzulegen, sondern sie aktiv zu vernetzen, komfortabel zu bearbeiten und schnell wiederzufinden.

Für die erste Version stehen insbesondere folgende Punkte im Vordergrund:

- Markdown als offenes Primärformat,
- komfortabler Editor,
- interne Verlinkung,
- Suche,
- thematisch getrennte Verzeichnis-/Vault-Verwaltung,
- klare Desktop-Bedienung,
- spätere Erweiterbarkeit ohne Plugin-Zwang in V1.

Dieses Lastenheft bildet die fachliche Grundlage für das nachfolgende Pflichtenheft und die weitere Architektur- und Umsetzungsplanung.

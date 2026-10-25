# Projektplan – SASD Notes MVP / V1

Version: 0.1  
Datum: 2026-07-04  
Status: Entwurf

---

## 1. Ziel

Dieses Dokument beschreibt eine realistische Umsetzungsreihenfolge für **SASD Notes V1**.

---

## 2. MVP-Ziel

Ein lokales Windows-Desktop-Werkzeug, das:

- einen Vault/Ordner öffnet,
- Markdown-Dateien anzeigt und speichert,
- neue Notizen erzeugt,
- interne Wiki-Links erkennt,
- Suchtreffer liefert,
- Backlinks anzeigt,
- verschiedene Themenordner verwaltet.

---

## 3. Meilensteine

### M1 – Repository und Fundament

- Repository-Struktur anlegen
- Dokumente einchecken
- Solution anlegen
- Projekte `App`, `Application`, `Domain`, `Infrastructure`, `Tests`
- `global.json` und `.gitignore`

### M2 – Domain und Infrastructure

- `NoteDocument`
- `VaultInfo`
- `WikiLink`
- `SearchHit`
- Dateisystem-Repository
- Vault-Scan
- Grundkonfiguration

### M3 – Kernfunktionen

- Vault öffnen
- TreeView füllen
- Notiz laden
- Notiz speichern
- neue Notiz erzeugen
- Recent Folders

### M4 – Wissensfunktionen

- WikiLinkParser
- LinkResolver
- Backlinks
- einfache Suche
- Gliederung aus Überschriften

### M5 – Editor-Komfort

- Toolbar
- Formatierungshilfen
- Dirty-State
- Statusleiste
- Fehlermeldungen verbessern

### M6 – Stabilisierung

- Testfälle automatisieren
- Fehlerbehandlung härten
- Dokumentation aktualisieren
- Release Candidate aufbauen

### Priorisierte Fortsetzung nach der stabilen V1-Basis

Nach Abschluss und manueller Abnahme der V1-Basis folgt der verbindlich
vorgesehene Ausbau zum Lesen und Recherchieren:

1. formatierte, browserähnlich bedienbare Markdown-Leseansicht,
2. Navigationsverlauf und verbesserte Suchdarstellung,
3. testbares Wissensgraph-Modell,
4. lokaler 2D-Graph,
5. spätere Prüfung einer räumlichen beziehungsweise 3D-Darstellung.

Die technische und fachliche Staffelung ist in
[`15_Roadmap_Lesen_Recherche_Wissensgraph.md`](15_Roadmap_Lesen_Recherche_Wissensgraph.md)
festgehalten.

---

## 4. Ausbaublöcke nach V1

- Markdown-Preview
- Tabs
- Frontmatter-Editor
- Export
- Assets-Verwaltung
- Plugin-Modell
- Theme/Dark-Mode verfeinern

---

## 5. Ergebnis

Der Projektplan hält den Start bewusst klein, liefert aber früh nutzbare Ergebnisse und bleibt offen für spätere Erweiterungen.

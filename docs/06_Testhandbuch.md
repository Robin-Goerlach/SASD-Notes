# Testhandbuch – SASD Notes

Version: 0.1  
Datum: 2026-07-04  
Status: Arbeitsfassung

---

## 1. Ziel

Dieses Testhandbuch beschreibt die Teststrategie für **SASD Notes V1**.  
Ziel ist eine praxistaugliche Qualitätssicherung für eine lokale Windows-Desktop-Anwendung mit Fokus auf Dateisystem, Markdown-Verarbeitung, interne Verlinkung und Suche.

---

## 2. Testziele

Zu prüfen sind insbesondere:

- korrektes Öffnen und Scannen eines Vaults
- Laden, Bearbeiten und Speichern von Markdown-Dateien
- Erzeugen und Auflösen von Wiki-Links
- Berechnung von Backlinks
- Suche in Dateinamen und Inhalten
- Verhalten bei Fehlern im Dateisystem
- UI-Grundfunktion der WinForms-Oberfläche

---

## 3. Testarten

### 3.1 Unit-Tests

Geeignet für:

- `WikiLinkParser`
- `LinkResolver`
- `SearchService`
- `OutlineService`
- `FileNameSanitizer`
- `FrontmatterParser`

### 3.2 Integrationsnahe Tests

Geeignet für:

- `VaultService` mit temporären Testverzeichnissen
- Laden und Speichern realer `.md`-Dateien
- Umbenennen und Neuerzeugung von Dateien
- Reaktion auf Dateiänderungen

### 3.3 Manuelle UI-Tests

Geeignet für:

- Menüeinträge
- TreeView-Navigation
- Editorverhalten
- Suchdialog / Suchpanel
- Kontextmenüs
- Statusanzeige
- Dialoge für Ordnerauswahl und Fehlermeldungen

---

## 4. Testumgebung

- Windows 10/11
- .NET 8 SDK / Runtime
- Visual Studio 2022
- Testverzeichnisse im lokalen Dateisystem
- reproduzierbare Beispiel-Vaults mit:
  - einfachen Notizen
  - Unterordnern
  - Links
  - Tags
  - Frontmatter
  - fehlenden Linkzielen

---

## 5. Beispiel-Testdaten

Empfohlener Test-Vault:

```text
TestVault/
├─ 00_Inbox/
│  └─ Idee.md
├─ 01_Projekte/
│  ├─ Projektplan.md
│  ├─ Meeting-Notizen.md
│  └─ Aufgabenliste.md
├─ 02_Wissen/
│  └─ Markdown.md
└─ 03_Archiv/
```

Beispielinhalte:

- `Projektplan.md` verlinkt `[[Meeting-Notizen]]`
- `Meeting-Notizen.md` verlinkt `[[Projektplan]]`
- `Aufgabenliste.md` enthält Suchbegriffe
- `Markdown.md` enthält Überschriften und Formatierung

---

## 6. Testfälle

### 6.1 Vault öffnen

- Gültigen Ordner öffnen → TreeView wird aufgebaut
- Leeren Ordner öffnen → Anwendung bleibt stabil
- Nicht lesbaren Ordner öffnen → verständliche Fehlermeldung

### 6.2 Notiz laden

- Vorhandene `.md`-Datei öffnen → Inhalt sichtbar
- Datei mit Umlauten öffnen → korrekt dargestellt
- Große Datei öffnen → keine Blockade / kontrolliertes Verhalten

### 6.3 Notiz speichern

- Neue Änderungen speichern → Datei wird aktualisiert
- Datei schreibgeschützt → Fehlermeldung
- Datei extern geändert → Konflikthinweis oder definierte Reaktion

### 6.4 Wiki-Links

- `[[Notiz]]` wird erkannt
- `[[Ordner/Notiz]]` wird erkannt
- `[[Notiz|Alias]]` wird erkannt
- fehlendes Linkziel wird markiert oder als nicht auflösbar gemeldet

### 6.5 Backlinks

- Rückverweise werden korrekt gezählt
- Notizen ohne Rückverweise zeigen leere Liste
- Umbenennung aktualisiert spätere Berechnung korrekt

### 6.6 Suche

- Treffer im Dateinamen
- Treffer im Inhalt
- keine Treffer
- Groß-/Kleinschreibung ignorieren
- Suchausschnitt zeigt Kontext

### 6.7 Ordnerverwaltung

- neuer Themenordner wird erstellt
- Wechsel in anderen Vault funktioniert
- Recent Folders werden gespeichert und erneut angezeigt

---

## 7. Abnahmekriterien für V1

V1 gilt als fachlich abnahmefähig, wenn mindestens:

- ein Vault geöffnet werden kann,
- Markdown-Dateien erstellt, bearbeitet und gespeichert werden können,
- Wiki-Links erkannt und für Navigation genutzt werden können,
- Backlinks berechnet werden,
- Suche über Dateien und Inhalte funktioniert,
- verschiedene Themenordner verwaltbar sind,
- die Anwendung bei typischen Dateifehlern stabil bleibt.

---

## 8. Automatisierungsziele

Empfohlen für frühe Automatisierung:

- Parser-Tests
- Suchtests
- Pfadvalidierung
- Dateinamen-Sanitizing
- Link-Auflösung

UI-Tests können in V1 überwiegend manuell bleiben.

---

## 9. Testergebnisdokumentation

Zu jedem Testlauf sollen mindestens erfasst werden:

- getestete Version / Commit
- Datum
- Tester
- Testumgebung
- Ergebnis
- Auffälligkeiten / offene Punkte

---

## 10. Ergebnis

Die Teststrategie von SASD Notes V1 konzentriert sich auf **verlässliche Dateiverarbeitung, korrekte Verlinkung, robuste Suche und stabile Bedienbarkeit**.  
Das Handbuch ist bewusst praxisnah gehalten, damit es sofort in einem kleinen Projektkontext eingesetzt werden kann.

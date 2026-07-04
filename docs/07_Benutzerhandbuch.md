# Benutzerhandbuch – SASD Notes V1

Version: 0.1  
Datum: 2026-07-04  
Status: Entwurf

---

## 1. Zweck

Dieses Benutzerhandbuch beschreibt die Bedienung von **SASD Notes V1** aus Anwendersicht.

---

## 2. Grundidee

SASD Notes verwaltet lokale Markdown-Dateien in einem frei wählbaren Arbeitsordner.  
Notizen können wie normale Textdateien bearbeitet, in thematischen Ordnern abgelegt, intern verlinkt und durchsucht werden.

---

## 3. Arbeitsordner / Vault öffnen

1. Menü **Datei → Ordner/Vault öffnen**
2. Gewünschten Root-Ordner auswählen
3. Die Anwendung scannt alle Markdown-Dateien
4. Links im Navigationsbereich erscheinen Ordner und Notizen

### Hinweise

- Der ausgewählte Ordner ist der aktive Arbeitsbereich.
- Unterordner werden als Themenstruktur dargestellt.
- Zuletzt verwendete Ordner erscheinen später im Menü **Recent Folders**.

---

## 4. Neue Notiz erstellen

1. Im TreeView Zielordner wählen
2. Menü **Datei → Neue Notiz**
3. Titel eingeben
4. Datei wird als `.md` im gewählten Ordner angelegt

---

## 5. Notiz bearbeiten

Die Notiz wird im zentralen Editor geöffnet.  
Der Editor arbeitet in V1 textbasiert mit Markdown.

Beispiele:

- `# Überschrift`
- `## Unterüberschrift`
- `**fett**`
- `_kursiv_`
- `` `code` ``

Toolbar- oder Menüfunktionen können ausgewählten Text mit Formatierungen umschließen.

---

## 6. Interne Verlinkung

SASD Notes unterstützt Wiki-Links.

Beispiele:

- `[[Projektplan]]`
- `[[Projekte/Projektplan]]`
- `[[Projektplan|Mein Plan]]`

Durch Aktivierung eines Links kann zur Zielnotiz navigiert werden.  
Wenn das Ziel fehlt, kann später eine neue Notiz daraus erzeugt werden.

---

## 7. Suche

Die Suche kann Dateinamen und Inhalte durchsuchen.

Typische Anwendungsfälle:

- bestimmte Notiz finden
- Aufgaben oder Begriffe wiederfinden
- Dokumente zu einem Projektthema suchen

Suchtreffer zeigen Titel, Pfad und einen kurzen Kontextausschnitt.

---

## 8. Backlinks

Der Backlinks-Bereich zeigt, welche Notizen auf die aktuelle Notiz verweisen.  
Das ist hilfreich für Wissensnetze, Projektdokumentation und Zusammenhänge zwischen Themen.

---

## 9. Themenordner

Für verschiedene Themenkomplexe können eigene Verzeichnisstrukturen verwendet werden.  
Beispiele:

- `00_Inbox`
- `01_Projekte`
- `02_Wissen`
- `03_Tagebuch`
- `04_Archiv`

Zusätzlich kann mit einem anderen Root-Ordner ein anderer Themenbestand geöffnet werden.

---

## 10. Speichern

Änderungen können manuell gespeichert werden.

Empfohlene Bedienweise:

- regelmäßig speichern
- vor größeren Umbenennungen sichern
- Markdown-Dateien zusätzlich per Git oder Backup schützen

---

## 11. Typische Arbeitsweise

1. Themenordner öffnen
2. neue Notiz anlegen
3. Inhalte schreiben und formatieren
4. mit `[[...]]` verlinken
5. über Suche und Backlinks navigieren
6. strukturierte Sammlung nach und nach ausbauen

---

## 12. Fehlersituationen

Beispiele:

- Datei ist schreibgeschützt
- Datei wurde extern verändert
- Linkziel existiert nicht
- Dateiname enthält ungültige Zeichen

Die Anwendung soll in diesen Fällen verständliche Hinweise geben und keine stillen Datenverluste erzeugen.

---

## 13. Ergebnis

SASD Notes ist als einfaches, lokales Arbeitswerkzeug gedacht:  
**offene Markdown-Dateien, klare Ordnerstruktur, interne Verlinkung und schneller Zugriff auf Wissen.**

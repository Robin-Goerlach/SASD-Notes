# Optionales Startziel

Anforderung vom 24.10.2026: SASD Notes kann beim Programmstart optional einen
Ordner oder eine Markdown-Datei entgegennehmen.

## Aufruf und Vorrang

- Es wird höchstens ein Pfadargument unterstützt, ohne zusätzliche Schalter.
- Ohne Argument gilt das bisherige Verhalten: den zuletzt verwendeten,
  noch vorhandenen Vault öffnen, andernfalls ohne Vault starten.
- Ein expliziter Pfad hat Vorrang vor dem gemerkten Vault.
- Relative Pfade werden relativ zum Arbeitsverzeichnis des aufrufenden Prozesses
  aufgelöst. Das Prozess-Arbeitsverzeichnis wird nicht geändert.
- Die Argumentaufteilung einschließlich Anführungszeichen übernimmt die
  Laufzeitumgebung. Pfade werden danach nicht erneut aufgeteilt oder getrimmt.

## Ordner und Dateien

- Ein vorhandener Ordner wird direkt als Vault geöffnet, einschließlich seiner
  Markdown-Unterordner. Eine beliebige Notiz wird dabei nicht automatisch ausgewählt.
- Ein vorhandener Ordner bleibt auch bei einer Endung wie .md ein Ordner.
- Bei einer vorhandenen .md-Datei wird ihr unmittelbar übergeordneter Ordner
  zum Vault. Ein höherer Projekt- oder Vault-Ordner wird nicht erraten.
- Nach dem Laden wird die gewünschte Notiz im Editor und Dateibaum ausgewählt.
- Die Dateiendung wird ohne Unterscheidung von Groß-/Kleinschreibung geprüft.
- Erfolgreich geöffnete Start-Vaults aktualisieren die bestehenden Recent Folders.
- Der Start verändert oder erzeugt keine Notizdateien.

## Fehler

Mehrere Argumente, leere oder ungültige Pfade, nicht vorhandene Ziele und
andere Dateitypen werden mit einer Fehlermeldung abgewiesen. Die Diagnose
nennt bei mehreren Argumenten deren Anzahl und bei einem bereits
normalisierbaren Ziel den tatsächlich geprüften vollständigen Pfad. Bei solchen
vor dem Fensterstart erkannten Fehlern endet die Anwendung mit Exitcode 1,
ohne stattdessen den gemerkten Vault zu öffnen oder Einstellungen zu laden.

Ändert sich das Dateisystem nach der Zielprüfung, greift die normale
Fehlerbehandlung beim Vault-Laden. Eine inzwischen fehlende Startnotiz
wird gemeldet; es wird keine Ersatzdatei erzeugt oder andere Notiz ausgewählt.

Bedienung und Schließen sind während der asynchronen Startinitialisierung
und des Vault-Ladens gesperrt und nach Abschluss bzw. behandeltem Fehler
wieder freigegeben.

## Architektur und Prüfung

- Application normalisiert Argumente ohne UI und ohne Dateizugriff und enthält
  das typisierte Startziel.
- Infrastructure prüft Ordner und Dateien. Der Adapter wird ausschließlich im
  Composition Root benutzt; Application und MainForm kennen keinen konkreten
  Dateisystemadapter für die Startauswertung.
- Program reicht das Ziel an MainForm weiter. MainForm verwendet den bestehenden
  Workspace-Öffnungsweg und dessen Fehlerbehandlung.
- Kernprüfungen decken Argumentanzahl, relative Pfade, Leerzeichen, Unicode,
  Dateiendungen, fehlende Ziele und unveränderte Dateiinhalte ab.
- Windows-Fenstertests prüfen Standard-, Ordner- und Dateistart einschließlich
  Vorrang vor Recent Folders, Auswahl im Baum, Editorinhalt und Dirty-State.
  Ein verzögerter Einstellungen-Start prüft auch die Schließsperre.

Mehrere Dateien, automatische Dateitypregistrierung und ein gesonderter
Vault-Schalter sind nicht Teil dieser Anforderung.

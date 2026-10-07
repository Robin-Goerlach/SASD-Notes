# Security Baseline – SASD Notes

Version: 0.1  
Datum: 2026-07-04  
Status: Arbeitsfassung

---

## 1. Ziel

Diese Security Baseline beschreibt die Mindestanforderungen für die erste lokale Desktop-Version von **SASD Notes**.  
Obwohl SASD Notes primär als lokale Anwendung ohne Cloud-Anbindung geplant ist, müssen Risiken aus Dateizugriff, Datenverlust, unkontrollierten Pfaden, Erweiterungspunkten und späteren Plugins bereits früh berücksichtigt werden.

---

## 2. Schutzbedarf

### 2.1 Schutzgüter

- Markdown-Notizen
- Metadaten / Properties
- lokale Anhänge / Assets
- Konfigurationsdateien
- Liste zuletzt verwendeter Vaults
- spätere Erweiterungsartefakte

### 2.2 Bedrohungen

- unbeabsichtigtes Überschreiben von Notizen
- Datenverlust durch fehlerhafte Speicherlogik
- Pfadmanipulation bei Dateioperationen
- Laden problematischer oder unerwarteter Dateien
- Ausführung unsicherer Erweiterungen in späteren Versionen
- Offenlegung sensibler Pfade oder Inhalte in Logs

---

## 3. Sicherheitsprinzipien

1. **Lokale Dateien sind schützenswert.**
2. **Nur ausdrücklich unterstützte Dateitypen verarbeiten.**
3. **Pfadgrenzen des geöffneten Vaults respektieren.**
4. **Keine ungeprüfte Codeausführung.**
5. **Keine unnötige Telemetrie.**
6. **Fehler protokollieren, aber keine Inhalte aus Notizen in Logs kippen.**

---

## 4. Mindestregeln für V1

### 4.1 Dateioperationen

- Es dürfen in V1 standardmäßig nur Markdown-Dateien (`.md`) bearbeitet werden.
- Dateioperationen müssen immer gegen den aktuell geöffneten Vault validiert werden.
- Relative Pfade sind zu normalisieren.
- Pfad-Traversal (`..`) darf nicht zu Schreibzugriffen außerhalb des Vaults führen.
- Vor dem Überschreiben einer Datei ist zu prüfen, ob die Datei extern geändert wurde.

### 4.2 Konfigurationsdaten

- Anwendungseinstellungen werden lokal gespeichert.
- In Konfigurationsdateien dürfen keine geheimen Zugangsdaten vorgesehen werden.
- Zuletzt verwendete Ordner dürfen gespeichert werden, müssen aber klar dokumentiert sein.

### 4.3 Logging

- Logs dürfen keine kompletten Notizinhalte enthalten.
- Logs dürfen sensible lokale Pfade nur minimiert oder maskiert ausgeben, wenn möglich.
- Fehlerlogs sind optional, aber strukturiert zu halten.

### 4.4 Externe Inhalte

- Bilder, Anhänge oder externe Referenzen dürfen in V1 nicht automatisch ausgeführt werden.
- Externe URLs sollen nur auf Benutzeraktion geöffnet werden.
- Markdown-Inhalte dürfen nicht automatisch als Skripte interpretiert werden.

---

## 5. Plugin-Vorbereitung

Da ein Plugin-System später vorgesehen ist, gelten bereits in V1 vorbereitende Regeln:

- Erweiterungspunkte sollen explizit definiert werden.
- UI, Fachlogik und Infrastruktur dürfen nicht über globale Zustände unkontrolliert verknüpft werden.
- Spätere Plugin-APIs müssen Berechtigungs- und Vertrauensfragen berücksichtigen.
- Ein späteres Laden von DLLs darf nur bewusst und nachvollziehbar erfolgen.

---

## 6. Dateisystem-Sicherheit

- Der geöffnete Vault ist als Sicherheitsgrenze zu betrachten.
- Schreibtätigkeiten außerhalb des Vaults sind nur bei expliziten Exportfunktionen zulässig.
- Umbenennen, Löschen und Erstellen müssen jeweils validierte Zielpfade verwenden.
- Dateinamen sind gegen ungültige Zeichen zu prüfen.

---

## 7. Backup- und Wiederherstellungsgedanke

Die Anwendung soll in V1 zwar kein vollständiges Backup-System enthalten müssen, aber:

- keine intransparente Datenhaltung verwenden,
- Speichern nachvollziehbar durchführen,
- spätere Sicherungskonzepte nicht verbauen,
- optional spätere Snapshot- oder Exportfunktionen vorbereiten.

---

## 8. Sichere Standardwerte

Empfohlene sichere Defaults:

- Auto-Save standardmäßig aus oder klar konfigurierbar
- externe Links nur per Benutzeraktion öffnen
- keine automatische Ausführung externer Tools
- keine Netzwerkkommunikation ohne dokumentierte Funktion
- lokale Textdateien als Primärformat

---

## 9. Prüf- und Abnahmeaspekte

Vor Freigabe von V1 sollten mindestens geprüft werden:

- Schreibschutz- und Lesefehler
- Pfadvalidierung
- Verhalten bei extern geänderter Datei
- Verhalten bei doppeltem Dateinamen
- Verhalten bei ungültigem Zeichen im Titel
- Logging ohne Offenlegung kompletter Inhalte
- Öffnen problematischer großer Dateien

---

## 10. Ergebnis

Die Security Baseline für SASD Notes V1 fokussiert auf **robuste lokale Dateiverarbeitung, transparente Speicherung und konservative Erweiterbarkeit**.  
Damit passt sie zum Ziel eines kontrollierbaren, langfristig wartbaren Markdown-Werkzeugs.

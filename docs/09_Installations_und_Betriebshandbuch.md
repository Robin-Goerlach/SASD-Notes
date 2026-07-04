# Installations- und Betriebshandbuch – SASD Notes

Version: 0.1  
Datum: 2026-07-04  
Status: Entwurf

---

## 1. Zweck

Dieses Handbuch beschreibt die lokale Entwicklung, den Build und den Betrieb von **SASD Notes**.

---

## 2. Zielplattform

- Windows 10 oder Windows 11
- .NET 8 SDK für Entwicklung
- .NET 8 Runtime oder Self-Contained Deployment für Zielsysteme
- Visual Studio 2022 empfohlen

---

## 3. Repository einrichten

1. Repository klonen
2. Lösung in Visual Studio 2022 öffnen
3. NuGet-Pakete wiederherstellen
4. Projekt bauen
5. WinForms-Startprojekt ausführen

---

## 4. Build-Strategie

Empfohlene Build-Arten:

- Debug für lokale Entwicklung
- Release für interne Teststände
- optional Self-Contained Publish für Zielsysteme

Beispielideen:

- `dotnet restore`
- `dotnet build`
- `dotnet test`
- `dotnet publish`

---

## 5. Lokale Datenhaltung

SASD Notes speichert Inhalte primär als Markdown-Dateien im gewählten Vault.  
Anwendungseinstellungen können lokal in JSON-Dateien abgelegt werden.

Typische lokale Daten:

- letzter geöffneter Vault
- Liste zuletzt verwendeter Ordner
- Fenstereinstellungen
- Editoroptionen

---

## 6. Betriebsaspekte

### 6.1 Empfohlene Praxis

- Vaults unter Versionskontrolle oder Backup halten
- große Umbenennungsaktionen mit Sicherung begleiten
- Test-Vault und produktive Notizen trennen

### 6.2 Logging

- nur technische Diagnosedaten
- keine vollständigen Notizinhalte ins Log
- Logging optional aktivierbar

### 6.3 Updates

In V1 ist keine Auto-Update-Infrastruktur erforderlich.  
Release-Updates erfolgen kontrolliert über neue Builds oder veröffentlichte Pakete.

---

## 7. Fehlersuche

Typische Ursachen:

- ungültiger Vault-Pfad
- fehlende Schreibrechte
- externe Änderung einer Datei
- ungültige Zeichen in Dateinamen
- unerwartete Editorzustände

Prüfschritte:

1. Pfad kontrollieren
2. Dateirechte prüfen
3. Test mit kleinem Beispiel-Vault
4. Logs sichten
5. Reproduzierbaren Testfall notieren

---

## 8. Empfohlene Repository-Dateien

- `.gitignore` für Visual Studio / bin / obj
- `global.json` für .NET 8
- `README.md`
- `CHANGELOG.md`

---

## 9. Ergebnis

SASD Notes soll bewusst als **einfach betreibbare lokale Windows-Anwendung** starten.  
Das Betriebshandbuch hält den organisatorischen und technischen Aufwand dafür klein und nachvollziehbar.

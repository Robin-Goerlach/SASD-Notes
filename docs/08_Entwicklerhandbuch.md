# Entwicklerhandbuch – SASD Notes

Version: 0.1  
Datum: 2026-07-04  
Status: Entwurf

---

## 1. Ziel

Dieses Entwicklerhandbuch beschreibt Konventionen, Projektstruktur und Arbeitsweise für die technische Umsetzung von **SASD Notes**.

---

## 2. Technologie

- Sprache: C#
- Plattform: .NET 8
- UI: Windows Forms
- IDE: Visual Studio 2022
- Tests: paketfreier Konsolen-Test-Harness (`Sasd.Notes.Tests`); ein späterer
  Wechsel auf ein Test-SDK ist möglich, aber für die aktuelle Build-Baseline
  nicht erforderlich.
- Repository: Git / GitHub

---

## 3. Projektstruktur

```text
src/
├─ Sasd.Notes.App.WinForms
├─ Sasd.Notes.Application
├─ Sasd.Notes.Domain
└─ Sasd.Notes.Infrastructure

tests/
└─ Sasd.Notes.Tests
```

### Grundregeln

- Fachlogik gehört nicht in Forms-Code.
- Infrastruktur darf nicht in die UI „hochbluten“.
- Parser und Suchlogik müssen ohne UI testbar sein.
- Klassen klein und gut lesbar halten.

---

## 4. Kommentierstil

Gemäß gewünschtem SASD-Stil:

- `///` XML-Kommentare an Klassen, Properties, Methoden
- `//` Kommentare innerhalb der Methoden, wenn sie das Verständnis verbessern
- keine bedeutungslosen Kommentare
- lieber ruhige, fachlich saubere Beschreibungen

---

## 5. Coding Guidelines

- sprechende Namen
- kein unnötig kompliziertes Pattern-Gewitter
- defensive Dateisystem-Validierung
- Exceptions nicht verschlucken
- `async` dort nutzen, wo I/O stattfindet
- Konfiguration möglichst simpel halten

---

## 6. Branching und Commits

Empfehlung:

- `main` stabil halten
- Feature-Branches pro Themenblock
- klare Commit-Messages, z. B.:
  - `feat(vault): add markdown vault scanning`
  - `feat(search): add basic file and content search`
  - `fix(parser): handle alias wiki links correctly`
  - `docs: add test handbook and security baseline`

---

## 7. Bevorzugte Entwicklungsreihenfolge

1. Domain-Modelle
2. Parser / Resolver / Suche
3. Dateisystemzugriff
4. Application Services
5. WinForms-Oberfläche
6. manuelle und automatisierte Tests
7. Verfeinerung der UI

---

## 8. Dokumentationspflege

Zu pflegen sind mindestens:

- README
- Lastenheft
- Pflichtenheft
- Technisches Design
- Security Baseline
- Testhandbuch
- Benutzerhandbuch
- Entwicklerhandbuch
- Changelog

---

## 9. Erweiterbarkeit

Ein Plugin-System ist in V1 noch nicht enthalten.  
Der Code soll aber so gebaut werden, dass spätere Erweiterungspunkte sinnvoll möglich bleiben:

- Schnittstellen statt harter Kopplung
- Services statt globaler Hilfsklassen
- klar definierte Eingangs- und Ausgangsobjekte
- Konfigurations- und Menüpunkte später erweiterbar

---

## 10. Build- und Testschleife

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project .\tests\Sasd.Notes.Tests\Sasd.Notes.Tests.csproj
```

`dotnet test` stellt in der aktuellen paketfreien Struktur den Build des
Testprojekts sicher. Die fachlichen Prüfungen werden anschließend explizit mit
`dotnet run` ausgeführt.

## 11. Ergebnis

Dieses Entwicklerhandbuch dient als Arbeitsgrundlage für ein kleines, sauber dokumentiertes SASD-Projekt mit Fokus auf Lesbarkeit, Wartbarkeit und nachvollziehbare Entwicklung.

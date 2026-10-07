# AGENTS.md — SASD Notes

## Zweck

SASD Notes ist eine lokale Markdown-Anwendung für Windows. Markdown-Dateien
bleiben die primäre Datenquelle und müssen ohne SASD Notes in anderen Editoren
weiterverwendbar sein.

## Technischer Rahmen

- C# und .NET 8
- Windows Forms auf `net8.0-windows`
- Visual Studio 2022
- moderne SDK-Projektdateien mit `Microsoft.NET.Sdk`
- keine ungefragte Umstellung auf .NET 9 oder .NET 10

## Schichten

- `Sasd.Notes.Domain` enthält fachliche Modelle und darf keine UI-Abhängigkeit
  besitzen.
- `Sasd.Notes.Application` enthält Use Cases und testbare Anwendungslogik.
- `Sasd.Notes.Infrastructure` enthält Dateisystem- und Persistenzadapter.
- `Sasd.Notes.App.WinForms` enthält Formulare, Controls und dünne UI-Handler.
- `Sasd.Notes.Tests` enthält paketfreie deterministische Kernprüfungen.

Die Markdown-Dateien im Vault sind maßgeblich. Eine spätere Datenbank darf nur
abgeleitete Daten wie Cache oder Suchindex speichern.

## Arbeitsregeln

- Bestehenden Code zuerst analysieren und gezielt verbessern.
- Verständliche Namen, XML-Dokumentation für öffentliche API und sinnvolle
  `//`-Kommentare bei Verarbeitungsschritten verwenden.
- Dateipfade mit `System.IO.Path` behandeln und jeden benutzerbestimmten Pfad
  gegen den geöffneten Vault prüfen.
- Keine Notizen ungefragt löschen oder außerhalb des Vaults überschreiben.
- Neue Parser-, Link-, Such-, Pfad- und Formatterlogik mit deterministischen
  Tests abdecken.
- Bei Compilerfehlern zuerst den ersten echten Fehler beheben und danach neu
  prüfen.

## Verifikation

Im Repository-Root ausführen:

```text
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project tests/Sasd.Notes.Tests/Sasd.Notes.Tests.csproj
```

Der paketfreie Test-Harness muss zusätzlich mit `dotnet run` ausgeführt werden,
weil `dotnet test` ohne Test-SDK nur den Testprojekt-Build verifizieren kann.


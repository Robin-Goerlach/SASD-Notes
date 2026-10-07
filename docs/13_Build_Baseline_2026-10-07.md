# Build-Baseline – 2026-10-07

## Ausgangslage

Der übergebene Stand enthielt einen vollständigen .NET-8-WinForms-Quellstand.
Der bekannte Buildblocker lag in `EditorFormattingService.cs`: mehrere
Zeilenumbrüche waren als echte Zeilenumbrüche innerhalb von String- und
Zeichenliteralen gespeichert. Dadurch entstand eine Compilerfehlerkaskade.
Zusätzlich enthielt das Testprogramm eine mehrzeilige normale Stringkonstante.

## Korrekturen

- Formatter-Escapes repariert und die Zeilenformatierung für LF und CRLF
  stabilisiert.
- Formatter-Prüfungen für leere Auswahl, ungültige Grenzen, mehrere Zeilen und
  Unicode ergänzt.
- Mehrzeilige Testzeichenkette korrekt als `\n`-Text geschrieben.
- WinForms-Projekt auf `Microsoft.NET.Sdk` umgestellt.
- XML-Dokumentation für alle `MarkdownFormatKind`-Werte ergänzt.
- Vault-Prüfung aus der UI hinter `IVaultRepository` gekapselt.
- Speicher- und Erzeugungspfade gegen das Verlassen des geöffneten Vaults
  validiert.
- Dateinamen um explizite Pfadtrenner- und Steuerzeichenprüfung ergänzt.
- Beschädigte JSON-Einstellungen führen zu sicheren Standardwerten statt zu
  einem Abbruch des Programmstarts.
- `AGENTS.md` mit den Projekt- und Codex-Regeln ergänzt.

## Lokale Prüfung in dieser Arbeitsumgebung

Der Quellstand wurde statisch geprüft:

- alle Projektdateien und `global.json` sind gültiges XML bzw. JSON,
- keine normalen C#-Stringliterale enthalten mehr einen unbeabsichtigten
  Zeilenumbruch,
- Klammer- und Blockstruktur aller C#-Dateien ist ausgeglichen,
- Projektverweise und `net8.0`/`net8.0-windows`-Ziele sind konsistent,
- die veraltete `Microsoft.NET.Sdk.WindowsDesktop`-Angabe ist entfernt.

Ein echter .NET-Build konnte in dieser Linux-Arbeitsumgebung nicht ausgeführt
werden, weil kein `dotnet`-SDK installiert ist. Deshalb sind `dotnet clean`,
`dotnet restore`, `dotnet build`, `dotnet test` und der Test-Harness mit
`dotnet run` hier nicht als erfolgreich bestätigt.

## Prüfung auf Windows

Im Repository-Root ausführen:

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project .\tests\Sasd.Notes.Tests\Sasd.Notes.Tests.csproj
```

Der paketfreie Test-Harness wird mit `dotnet run` ausgeführt. `dotnet test`
stellt in dieser Struktur den Build des Testprojekts sicher, führt aber ohne
Test-SDK keine einzelnen Testmethoden aus.

## Nachprüfung unter Windows

Der erste Windows-Lauf bestätigte `dotnet clean`, `dotnet restore` und den
paketfreien Test-Harness. Der Solution-Build meldete anschließend einen
Namespace-Konflikt bei `Application.Run` sowie zwei `CS8669`-Warnungen in den
Designer-Dateien. Diese drei Stellen wurden danach korrigiert und sind im
aktualisierten ZIP enthalten.

Der zweite Windows-Lauf mit diesem Stand kompilierte die komplette Solution
erfolgreich mit 0 Fehlern. Dabei wurden noch 55 Nullable-Warnungen aus den
WinForms-Designerfeldern sichtbar. Die Designerfelder sind nun mit `null!`
gekennzeichnet, weil sie garantiert durch `InitializeComponent` erzeugt werden.
Damit sollte der nächste Lauf die Build-Baseline ohne Fehler und Warnungen
bestätigen.

## Bewusst offen gebliebene V1-Härtung

Die vorhandene V1 enthält weiterhin keine Erkennung konkurrierender externer
Dateiänderungen und keinen atomaren Speicheraustausch. Diese Themen gehören in
die geplante V1-Härtung, nachdem die Build-Baseline unter Windows bestätigt
wurde.

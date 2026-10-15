# Changelog

## Unreleased — Build-Baseline-Korrektur

- zerstörte Zeilenumbruch- und Zeichenliterale im Markdown-Formatter repariert
- paketfreien Test-Harness um Formatter-, Unicode-, CRLF- und Backlink-Prüfungen erweitert
- fehlende XML-Dokumentation für `MarkdownFormatKind` ergänzt
- WinForms-Projekt auf das moderne `Microsoft.NET.Sdk` umgestellt
- Vault-Pfade beim Speichern und Erzeugen neuer Notizen gegen Path Traversal abgesichert
- .NET-8-SDK-Band in `global.json` reproduzierbarer festgelegt
- Speichern von Markdown-Notizen gegen externe Änderungen abgesichert
- Notizen und lokale Einstellungen werden zunächst atomar über temporäre Dateien geschrieben
- Repository-Tests für erfolgreiches Speichern und erkannte externe Änderungen ergänzt
- Nicht auflösbare Wiki-Links werden im Info-Bereich angezeigt
- Warnung bei extern geänderten oder gelöschten Notizen ergänzt

## 2026-07-04

- added complete .NET 8 WinForms solution for SASD Notes
- added domain, application, infrastructure and app projects
- added package-free verification harness project
- added sample vault for quick testing
- kept the project documentation and UI concept screenshot

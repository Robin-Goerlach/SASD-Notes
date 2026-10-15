# Changelog

## Unreleased — Build-Baseline-Korrektur

- Sichere Erstellung und Anzeige leerer Vault-Unterordner im WinForms-Baum ergänzt.
- Beim Erzeugen einer neuen Notiz wird der Dirty-State vor dem Wechsel abgefragt.
- Markdown-Toolbar um Durchstreichen, Zitat und nummerierte Listen ergänzt.
- Windows-reservierte Datei- und Ordnernamen sowie problematische Endzeichen werden sicher normalisiert.
- Kontrolliertes Neuladen extern geänderter Notizen mit Dirty-State-Abfrage ergänzt.
- Windows-CI um Release-Publish und eine V1-Release-Checkliste erweitert.

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
- Such- und Backlink-Treffer zeigen jetzt auch ihre relativen Pfade an
- Windows-GitHub-Action für Restore, Build und Tests ergänzt
- Auch neu angelegte Notizen werden atomar geschrieben

## 2026-07-04

- added complete .NET 8 WinForms solution for SASD Notes
- added domain, application, infrastructure and app projects
- added package-free verification harness project
- added sample vault for quick testing
- kept the project documentation and UI concept screenshot

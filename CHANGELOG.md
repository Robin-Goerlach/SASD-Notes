# Changelog

## Unreleased — Build-Baseline-Korrektur

- Speichern verwendet einen unabhängigen Textschnappschuss; neuere Editoränderungen bleiben dirty. Doppelte laufende Speicherbefehle werden abgewiesen, Fehler und Abbruch erlauben Wiederholung. Verzögerte Application-Tests und Speicherspezifikation ergänzt.

- Vault-Wechsel übernimmt Notizen, Ordnerbaum und Recent Folders erst nach erfolgreichem Laden und Speichern der vorbereiteten Einstellungen. Fehler- und Abbruchtests sichern den Erhalt des bisherigen Dirty-State ab.

- Neuerzeugung von Notizen verwendet exklusives Veröffentlichen ohne Überschreiben; konkurrierende Namenskollisionen werden begrenzt wiederholt. Tests für gleichzeitige Erstellung, belegte Verzeichnisnamen und Abbruch ergänzt.

- Fensterschließen wartet auf asynchrone Speicherbestätigung; doppelte Schließversuche werden gesperrt. Windows-Regressionstest mit verzögertem Abschluss und Abbruch ergänzt.

- Markdown-Zeilenformatierung erhält angrenzende CRLF-Zeilenumbrüche und respektiert exklusive Auswahlenden; LF-/CRLF-Regressionstests ergänzt.

- Reservierte Windows-Namen mit Erweiterung korrekt bereinigt (`CON.md` → `CON_.md`); Namensregeln plattformunabhängig getestet.

- Sichere Erstellung und Anzeige leerer Vault-Unterordner im WinForms-Baum ergänzt.
- Beim Erzeugen einer neuen Notiz wird der Dirty-State vor dem Wechsel abgefragt.
- Markdown-Toolbar um Durchstreichen, Zitat und nummerierte Listen ergänzt.
- Windows-reservierte Datei- und Ordnernamen sowie problematische Endzeichen werden sicher normalisiert.
- Kontrolliertes Neuladen extern geänderter Notizen mit Dirty-State-Abfrage ergänzt.
- Windows-CI um Release-Publish und eine V1-Release-Checkliste erweitert.
- Der veröffentlichte WinForms-Stand wird zusätzlich als ZIP-CI-Artefakt bereitgestellt.
- README um den Release-Candidate-Status und die V1-Checkliste ergänzt.

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

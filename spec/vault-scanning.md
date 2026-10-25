# Vault-Scan

Ein Vault ist ein vorhandenes Wurzelverzeichnis. SASD Notes ermittelt darunter
rekursiv die normalen Markdown-Notizen und die sichtbare Ordnerstruktur.

## Notizdateien

- Eine Notiz ist eine Datei mit der Endung `.md`.
- Die Endung wird ordinal ohne Unterscheidung der Groß-/Kleinschreibung
  verglichen. Damit gelten beispielsweise `.md`, `.MD` und `.mD` gleich.
- Andere Dateitypen werden nicht als Notizen geladen oder verändert.
- Der Dateisystem-Scan erzeugt keine fehlenden Dateien und schreibt keine
  vorhandenen Inhalte.
- Geladene Notizen werden deterministisch nach ihrem logischen relativen
  Vault-Pfad sortiert: zuerst ordinal ohne Groß-/Kleinschreibung, bei Gleichstand
  ordinal. Die vom Dateisystem gelieferte Enumerationsreihenfolge ist nicht
  maßgeblich.

## Ordner

Unterordner werden rekursiv ermittelt und relativ zur Vault-Wurzel dargestellt.
Auch leere Ordner gehören zur Navigationsstruktur. Logische Pfade verwenden in
plattformneutralen Vergleichen den Schrägstrich `/`.

## Fehler und Abbruch

Ein nicht vorhandener oder nicht zugänglicher Vault darf nicht als leerer Vault
ausgegeben werden. Ein bereits angeforderter Abbruch wird vor der rekursiven
Dateientdeckung geprüft. Während der Entdeckung und zwischen dem Einlesen
einzelner Markdown-Dateien wird der Abbruch erneut berücksichtigt. Eine
Behandlung unzugänglicher Einzeldateien als partieller erfolgreicher Scan ist
für V1 nicht vereinbart.

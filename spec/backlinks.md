# Backlink-Liste in V1

Die Backlink-Liste enthält die Quelldateien, die auf die aktuelle Notiz verweisen.
Pro Quelldatei wird genau ein Navigationseintrag angezeigt, auch wenn dieselbe
Datei das Ziel mehrfach oder mit verschiedenen Aliastexten verlinkt.

- Die Auflösung erfolgt mit demselben Resolver wie die normale Linknavigation.
- Nur tatsächlich aufgelöste Zielverweise zählen. Fehlende oder mehrdeutige
  Ziele sowie vom Parser ausgeschlossene Codebeispiele erzeugen keine Einträge.
- Als Kontext dient der erste gültige Ziellink der Quelle in Parserreihenfolge.
- Die Identität der Quelle wird anhand ihres vollständigen Pfads bestimmt,
  ohne Unterscheidung von Groß-/Kleinschreibung wie in der Windows-Navigation.
- Gleichnamige Quelldateien in verschiedenen Ordnern bleiben getrennte Einträge.
- Selbstverweise der Zielnotiz werden nicht angezeigt.
- Die Ausgabe wird nach relativem Quellpfad sortiert.
- Jede Abfrage berechnet die Liste neu aus dem übergebenen Arbeitsstand.
  Entfernte Verweise dürfen daher nicht als alte Einträge zurückbleiben.

Eine spätere Ansicht einzelner Linkvorkommen oder eine Zählung je Quelle
ist nicht Bestandteil dieser Änderung. Der Dienst liest und schreibt keine Dateien.

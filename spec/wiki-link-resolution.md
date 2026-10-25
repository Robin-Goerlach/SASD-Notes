# Eindeutige Auflösung von Wiki-Links

Wiki-Links werden ausschließlich auf bereits bekannte Markdown-Notizen im
aktuellen Vault aufgelöst. Navigation, nicht auflösbare Links und Backlinks
verwenden denselben Resolver.

## Regeln für V1

1. Führende und abschließende Leerzeichen am Ziel werden entfernt.
2. Rückwärtsschrägstriche werden als Pfadtrenner wie Schrägstriche behandelt.
3. Eine abschließende Dateiendung `.md` ist optional. Pfadvergleich und
   Endungsvergleich erfolgen ohne Unterscheidung der Groß-/Kleinschreibung.
4. Ein eindeutiger exakter relativer Pfad hat Vorrang. Das gilt auch für eine
   Notiz direkt im Vault-Wurzelverzeichnis.
5. Enthält das Ziel einen Ordnerpfad, ist dieser verbindlich. Ein nicht
   vorhandenes `Projekte/Plan` wird nicht auf `Archiv/Plan.md` umgelenkt.
6. Nur Ziele ohne Ordner dürfen über den Dateinamen in Unterordnern gesucht
   werden. Gibt es dort mehrere passende Notizen und keinen eindeutigen
   exakten Pfad, bleibt der Kurzlink unaufgelöst.
7. Gibt es bereits mehrere exakte Treffer nach der Normalisierung, bleibt
   das Ziel ebenfalls unaufgelöst.
8. Leere oder nicht vorhandene Ziele bleiben unaufgelöst.

Der Alias in `[[Projekte/Plan.md|Projektplan]]` beeinflusst die Auflösung nicht.
Die Reihenfolge der geladenen Notizen darf die Zielauswahl nicht ändern.

## Beispiele

Bei `Archiv/Plan.md` und `Projekte/Plan.md`:

| Ziel | Ergebnis |
| --- | --- |
| `Projekte/Plan` | `Projekte/Plan.md` |
| `Projekte/Plan.md` | `Projekte/Plan.md` |
| `Fehlt/Plan` | unaufgelöst |
| `Plan` | unaufgelöst, da mehrdeutig |

Kommt zusätzlich `Plan.md` im Wurzelverzeichnis hinzu, zeigt `[[Plan]]`
auf diese Datei als exakten relativen Pfad.

## Grenzen

V1 liefert für fehlende und mehrdeutige Ziele jeweils kein aufgelöstes Modell.
Ein gesonderter Auswahldialog für Mehrdeutigkeiten ist noch nicht implementiert.
Quellordner-relative Pfade mit `.` oder `..`, Abschnittsanker und
Blockreferenzen sind kein Bestandteil dieser Auflösungsregeln.

Der Resolver liest keine Dateien und verwendet keine UI-Steuerelemente.

## Übersicht ausgehender Links

Die Übersicht erhält jedes vom gemeinsamen Parser erkannte Linkvorkommen in
Dokumentreihenfolge. Ziel, optionaler Alias und ursprünglicher Rohtext bleiben
erhalten. Der gemeinsame Resolver ergänzt nur bei eindeutiger Auflösung den
relativen Zielpfad. Fehlende und mehrdeutige Ziele gelten beide als nicht
aufgelöst und dürfen nicht willkürlich navigiert werden. Die Oberfläche darf
diese fachliche Entscheidung nicht durch eine eigene Namenssuche ersetzen.

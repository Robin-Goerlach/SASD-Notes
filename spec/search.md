# Einfache Suche in V1

Die Suche prüft Titel, relativen Pfad und Inhalt der geladenen Notizen mit
ordinalem Vergleich ohne Unterscheidung von Groß-/Kleinschreibung.
Rand-Leerraum des Suchbegriffs wird entfernt; leere Anfragen liefern keine Treffer.
Der Suchbegriff wird als zusammenhängender Literaltext gesucht.

Die einfache Bewertung addiert 100 Punkte für einen Titel-, 40 für einen
Pfad- und 50 für einen Inhaltstreffer. Sortiert wird nach absteigendem Score,
dann nach Titel und relativem Pfad. Gleichnamige Notizen bleiben getrennt.

Die Vorschau verwendet den Kontext des ersten Inhaltstreffers. Fehlt ein
Inhaltstreffer, wird der Dokumentanfang gezeigt. Ausgelassener Text am Anfang
bzw. Ende wird durch Auslassungspunkte gekennzeichnet; Zeilenumbrüche werden
für die Anzeige durch Leerzeichen ersetzt.

Ausschnittgrenzen dürfen gültige UTF-16-Surrogatpaare nicht trennen. Die
ursprüngliche Fundposition wird dadurch nicht verändert. Dies gewährleistet
keine vollständige Segmentierung kombinierter Zeichen oder Emoji-Sequenzen.
Die Suche verändert weder Notiztext noch Dateisystem und benötigt keinen Index.

Jeder Inhaltstreffer speichert die ursprüngliche UTF-16-Fundposition und die
Länge des tatsächlich gesuchten, bereits getrimmten Begriffs. Reine Titel-
oder Pfadtreffer haben Position -1 und Länge 0. Die Oberfläche verwendet
diese gespeicherte Länge für die Auswahl, nicht den späteren Inhalt des
Suchfelds. Zusätzlich wird der exakt gefundene Originaltext gespeichert.

Vor der Navigation wird dieser Text an der ursprünglichen Position bestätigt.
Nach zwischenzeitlichen Bearbeitungen wird das nächstgelegene Vorkommen mit
ordinalem Vergleich ohne Unterscheidung der Groß-/Kleinschreibung gesucht. Bei
gleicher Entfernung gewinnt die frühere Position. Ist der Treffer verschwunden
oder sind die Trefferdaten inkonsistent, öffnet die Oberfläche weiterhin die
Notiz, markiert aber keinen falschen Bereich und fordert zu einer neuen Suche auf.

Die anschließende Zuordnung zum Editortext berücksichtigt unterschiedliche
Zeilenenden (siehe editor-text-positions.md). Trefferdaten und Lokalisierung
verwenden UTF-16-Positionen.

## Trefferklassifikation

Jeder Treffer enthält eine kombinierbare Klassifikation der tatsächlich
gefundenen Bereiche: Titel, relativer Pfad und Inhalt. Ein Treffer kann mehrere
Bereiche gleichzeitig ausweisen. Die Klassifikation entsteht gemeinsam mit der
Bewertung in der Suchlogik; Oberflächen leiten sie nicht nachträglich aus dem
Score ab. Dadurch bleibt die additive Bewertung unverändert und die Darstellung
kann die Fundstellen dennoch eindeutig benennen.

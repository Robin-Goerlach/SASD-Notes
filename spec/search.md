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
Suchfelds. Bei inzwischen kürzerem Editortext wird die Länge auf den
verfügbaren Rest begrenzt. Nachträgliche Inhaltsänderungen können Treffer
weiterhin veralten lassen und erfordern eine erneute Suche.

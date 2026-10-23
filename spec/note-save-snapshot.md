# Speichern eines festen Notizstands

Ein Speicherbefehl erfasst den übergebenen Text einschließlich seiner Wiki-Links
in einem separaten Schnappschuss. Das Repository schreibt diesen Stand.

- Weitere Änderungen am Arbeitsmodell verändern den laufenden Schreibauftrag nicht.
- Nach Erfolg wird der gespeicherte Änderungszeitstempel ins Arbeitsmodell übernommen.
- Nur wenn der aktuelle Text dem gespeicherten Text entspricht, wird die Notiz als
  gespeichert markiert. Neuere, abweichende Änderungen bleiben ungespeichert.
- Bei Schreibfehlern oder beobachtetem Abbruch bleiben der angeforderte Text und
  der Dirty-State erhalten; der letzte erfolgreiche Zeitstempel bleibt bestehen.
- Bereits vor Beginn abgebrochene Aufträge verändern das Arbeitsmodell nicht.
- Ein zweiter laufender Speicherbefehl für denselben Arbeitsgegenstand wird
  abgewiesen, bevor er Text oder Repository verändert. Nach Abschluss, Fehler
  oder Abbruch ist erneutes Speichern möglich.
- Die Oberfläche darf nach einem Speicherbefehl mit weiterhin ungespeicherten
  Änderungen einen darauf wartenden Schließ- oder Wechselbefehl nicht freigeben.

Die Anwendung verwendet ihre Workspace-Operationen auf dem UI-Kontext.
Dies ist keine allgemeine Absicherung gegen parallele Zugriffe mehrerer Threads,
mehrerer Modelle derselben Datei oder externer Prozesse. Externe Änderungen
werden weiterhin anhand des Repository-Zeitstempels geprüft.

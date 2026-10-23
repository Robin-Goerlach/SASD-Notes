# Gezieltes Neuladen einer Notiz

„Aktuelle Notiz neu laden“ liest genau die angeforderte Markdown-Datei.
Ein erneuter vollständiger Vault-Scan ist dafür nicht erforderlich.

- Das Repository prüft den vollständig angegebenen Dateipfad lexikalisch gegen
  das Vault-Wurzelverzeichnis und akzeptiert nur Markdown-Dateien.
- Einzelzugriff und Vault-Scan verwenden dieselben Regeln für Text, Titel,
  relative Pfade, Wiki-Links und Änderungszeitstempel.
- Die gewünschte Notiz wird erst nach erfolgreichem Lesen ersetzt. Andere
  geladene Notizen einschließlich ihrer lokalen Änderungen bleiben erhalten.
- Bei fehlender oder nicht lesbarer Datei bleibt das bisherige Arbeitsmodell
  mit seiner Verwerfen-Basis erhalten.
- Während einer laufenden Speicherung derselben Arbeitsnotiz wird Neuladen
  abgewiesen. Ein bereits abgelöstes Modell darf nicht erneut geladen werden.
- Ändern sich Workspace, Modellzuordnung, Inhalt, Dirty-State oder gespeicherter
  Zeitstempel während des Lesens, wird das Ergebnis nicht übernommen.
- Ein nach dem Lesen beobachteter Abbruch verhindert ebenfalls die Übernahme.
- Die Oberfläche öffnet das Ergebnis nur dann im Editor, wenn dort weiterhin
  die angeforderte Arbeitsnotiz ausgewählt ist.
- Lokale Änderungen vor Ladebeginn dürfen nur nach ausdrücklicher Bestätigung
  durch die Oberfläche verworfen werden.

Dies ist keine allgemeine Synchronisation gegen externe Dateischreiber.
Die Pfadprüfung löst symbolische Links oder Windows-Junctions nicht auf.

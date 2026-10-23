# Lokale Änderungen verwerfen

Die Entscheidung „Nicht speichern“ vor einem Notizwechsel oder dem Schließen
verwirft den lokalen Arbeitsstand tatsächlich. Beim erneuten Öffnen derselben
geladenen Notiz dürfen die verworfenen Änderungen nicht zurückkehren.

- Vor der ersten Bearbeitung merkt sich der Workspace den geladenen Text.
  Weitere Bearbeitungsschritte überschreiben diese Basis nicht.
- Erfolgreiches Speichern ersetzt die Basis durch den gespeicherten Textstand.
  Dies gilt auch, wenn während des Schreibens bereits weitergetippt wurde.
- Schreibfehler und Abbruch ändern die letzte erfolgreiche Basis nicht.
- Verwerfen stellt Text und daraus geparste Wiki-Links wieder her und setzt den
  Dirty-State zurück. Der Dateizeitstempel bleibt unverändert.
- Während eines laufenden Speicherauftrags derselben Arbeitsnotiz wird Verwerfen
  abgewiesen. Es darf nicht gegen einen noch offenen Schreibauftrag konkurrieren.
- Bei fehlender bekannter Basis wird eine geänderte Notiz unverändert belassen.
- Nach erfolgreichem Neuladen beginnt das neue Modell mit seinem neuen Ladestand.
  Nach erfolgreichem Vault-Wechsel werden die bisherigen Bearbeitungsbasen freigegeben.
- Die Oberfläche zeigt den wiederhergestellten Text sofort an, da ein nachfolgender
  Dialog abgebrochen werden oder das Öffnen eines anderen Vaults fehlschlagen kann.

Verwerfen liest oder schreibt keine Datei. Es stellt den zuletzt bekannten
geladenen bzw. gespeicherten Text wieder her. Externe Änderungen erfordern
weiterhin den ausdrücklichen Befehl zum Neuladen.

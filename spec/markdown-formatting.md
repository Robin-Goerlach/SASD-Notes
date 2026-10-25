# Markdown-Formatierungshilfen

SASD Notes verändert mit den Formatierungsbefehlen normalen Markdown-Text. Die
Befehle arbeiten auf Text und Auswahlpositionen und benötigen kein UI-Control.

## Auswahlgrenzen

- Auswahlpositionen werden auf den gültigen UTF-16-Bereich des Dokuments
  begrenzt.
- Eine nichtleere Auswahl wird an ihren Grenzen erweitert, wenn sie andernfalls
  ein UTF-16-Surrogatpaar teilen würde. Ein Cursor innerhalb eines solchen
  Paares wird vor das vollständige Zeichen verschoben.
- Umschließende Befehle markieren nach der Transformation weiterhin den
  eigentlichen Inhalt ohne eingefügte Marker.
- Eine leere Auswahl erzeugt einen verständlichen Platzhalter und markiert ihn.
- Zeilenbefehle erweitern die Auswahl auf die tatsächlich betroffenen
  vollständigen Zeilen.
- Ein exklusives Auswahlende am Anfang der nächsten Zeile bezieht diese nächste
  Zeile nicht ein.

## Wiederholte Anwendung

Ein Formatierungsbefehl entfernt in V1 keine vorhandene Markdown-Formatierung.
Wird derselbe Befehl auf einen bereits exakt passend formatierten Inhalt
angewendet, bleibt der Text unverändert:

- Umschließende Befehle erkennen sowohl eine reine Inhaltsauswahl zwischen den
  Markern als auch eine Auswahl einschließlich der Marker.
- Gleichartige Marker werden nur dann als passend angesehen, wenn sie eindeutig
  begrenzt sind. Dadurch gilt beispielsweise Fettschrift nicht versehentlich
  als bereits vorhandene Kursivschrift.
- Zeilenpräfixe werden pro Zeile nicht dupliziert.
- Bereits dezimal nummerierte Zeilen erhalten kein zweites Nummerierungspräfix.

Diese idempotente Anwendung schützt vor versehentlichen Doppelmarkierungen. Ein
späterer Umschaltbefehl zum gezielten Entfernen von Formatierungen benötigt eine
eigene Spezifikation.

## Zeilentrenner

Zeilenbasierte Befehle verändern ausschließlich den sichtbaren Zeileninhalt.
Jeder vorhandene Zeilentrenner bleibt an seiner Position und in seiner Form
erhalten:

- Windows-CRLF (`\r\n`)
- Unix-LF (`\n`)
- einzelnes CR (`\r`)

Dies gilt auch für Dokumente, die mehrere Formen mischen. Die Formatierung darf
ein solches Dokument nicht nebenbei auf einen einheitlichen Zeilenstil
umschreiben. Ein abschließender Zeilentrenner bleibt ebenfalls unverändert.

## Unterstützte V1-Befehle

- Überschrift Ebene 1 bis 3
- Fett
- Kursiv
- Durchgestrichen
- Inline-Code
- Zitat
- Aufzählung
- nummerierte Liste
- Checkliste
- Wiki-Link

Leere Zeilen erhalten bei zeilenbasierten Befehlen keinen Marker. Nummerierte
Listen zählen nur nichtleere Zeilen.

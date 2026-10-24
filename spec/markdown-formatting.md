# Markdown-Formatierungshilfen

SASD Notes verändert mit den Formatierungsbefehlen normalen Markdown-Text. Die
Befehle arbeiten auf Text und Auswahlpositionen und benötigen kein UI-Control.

## Auswahlgrenzen

- Auswahlpositionen werden auf den gültigen UTF-16-Bereich des Dokuments
  begrenzt.
- Umschließende Befehle markieren nach der Transformation weiterhin den
  eigentlichen Inhalt ohne eingefügte Marker.
- Eine leere Auswahl erzeugt einen verständlichen Platzhalter und markiert ihn.
- Zeilenbefehle erweitern die Auswahl auf die tatsächlich betroffenen
  vollständigen Zeilen.
- Ein exklusives Auswahlende am Anfang der nächsten Zeile bezieht diese nächste
  Zeile nicht ein.

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

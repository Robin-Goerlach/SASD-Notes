# Codeblöcke bei Link- und Outline-Erkennung

## Verhalten

Wiki-Link-Parser und Gliederungsparser verwenden dieselbe Erkennung eingefasster
Codeblöcke. Codebeispiele erzeugen keine navigierbaren Wiki-Links, Backlinks,
Warnungen über fehlende Ziele oder Gliederungspunkte.

Auch bei einer vollständigen Linkauswahl verwendet die Navigation nur Treffer
aus dem Dokumentkontext. Ein isoliertes Fragment aus einem Codeblock darf die
Prüfung nicht umgehen. Auswahlbeginn und Linktext müssen zu einem gültigen
Treffer im aktuellen Text passen.

Der Text selbst wird dabei weder verändert noch gekürzt. Die Startpositionen
der verbleibenden Treffer beziehen sich weiterhin auf den Originaltext in
UTF-16-Zeichenpositionen, wie sie die .NET-Editoranbindung verwendet.

## Unterstützte Fences

- Ein Block beginnt mit mindestens drei aufeinanderfolgenden Backticks oder
  Tilden, mit höchstens drei vorangestellten Leerzeichen.
- Nach der öffnenden Fence darf eine Infoangabe stehen. Bei Backtick-Fences
  darf diese keine weiteren Backticks enthalten.
- Der Abschluss verwendet dieselbe Markerart und mindestens die Länge der
  öffnenden Fence. Danach sind ausschließlich Leerzeichen und Tabs erlaubt.
- Kürzere Marker, eine andere Markerart oder nachgestellter Text schließen
  einen laufenden Block nicht.
- Öffnende und schließende Markerzeilen gehören ebenfalls zum Codebereich.
- Ein nicht geschlossener Block reicht bis zum Dokumentende. Die nächste
  Auswertung wird vollständig aus dem aktuellen Text neu berechnet.
- LF und CRLF werden ohne Normalisierung der Zeichenpositionen verarbeitet.

Die Fence-Regeln orientieren sich an
[CommonMark 0.31.2, Abschnitt 4.5](https://spec.commonmark.org/0.31.2/#fenced-code-blocks).

## Zeilengrenzen

Wiki-Links dürfen sich nicht über LF- oder CRLF-Zeilengrenzen erstrecken und
benötigen ein nicht leeres Ziel. Eine ATX-Überschrift wird in V1 am Zeilenanfang
durch ein bis sechs Hash-Zeichen und mindestens ein Leerzeichen oder Tab
eingeleitet. Der Titel muss auf derselben Zeile stehen und darf nicht leer sein.
Insbesondere darf ein einzelnes Hash-Zeichen keine Folgezeile als Titel aufnehmen.

## Grenzen

Diese Erkennung ist kein vollständiger Markdown-Syntaxbaum. Inline-Code,
nur durch Einrückung markierte Codeblöcke, HTML-Kommentare und verschachtelte
Container wie Listen oder Zitate werden hier nicht als weitere Codekontexte
ausgewertet. Setext-Überschriften und die vollständige ATX-Syntax sind ebenfalls
nicht Teil dieser Änderung.

## Tests

Der paketfreie Kern-Harness prüft Parser, Originalpositionen und Backlinks
direkt sowie Navigation, unaufgelöste Links und Outline auf dem aktuellen
Arbeitsinhalt eines Workspace.

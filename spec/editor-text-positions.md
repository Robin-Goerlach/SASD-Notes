# Textpositionen zwischen Markdown und Editor

Suchtreffer, Outline und Wiki-Link-Parser verwenden UTF-16-Positionen im
Notiztext. Die Editoranbindung darf diese Positionen nicht ungeprüft übernehmen,
wenn das Control Zeilenenden anders darstellt.

Der Application-Dienst EditorTextPositionMapper überträgt Bereiche zwischen
zwei Texten. CRLF, LF und CR gelten dabei als jeweils ein logisches Zeilenende.
Alle übrigen UTF-16-Einheiten müssen im gesamten Text exakt übereinstimmen.

- Suche und Outline bilden Originalpositionen auf den Editortext ab.
- Wiki-Link-Navigation bildet Cursor und Auswahl zurück auf den Notiztext ab.
- Start und exklusives Ende werden getrennt übertragen; Bereichslängen können
  sich bei abweichenden Zeilenenden ändern.
- Ungültige Bereiche, Unterschiede im Inhalt und Grenzen mitten in einem
  CRLF-Paar werden abgewiesen. Es wird keine Position durch Abschneiden erraten.
- Leere Bereiche am Dokumentende sowie leere Dokumente sind zulässig.
- Die Zuordnung verändert weder Texte noch Dateien.

Dies ist keine allgemeine Wiederauffindung veralteter Suchtreffer nach
Inhaltsänderungen. Solche Änderungen erfordern weiterhin eine erneute Suche.
Kernprüfungen decken beide Richtungen, Unicode und Fehlergrenzen ab; ein
Windows-Test prüft zusätzlich Auswahl und Rückrichtung an einer echten RichTextBox.

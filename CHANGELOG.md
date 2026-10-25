# Changelog

## Unreleased — Build-Baseline-Korrektur

- Die Rechercheansicht rendert Markdown jetzt über einen UI-unabhängigen Vertrag mit Markdig und zeigt das Ergebnis sicher eingeschränkt in WebView2. Rohes HTML, Skripte, Hostobjekte und automatische Navigation sind deaktiviert; CSP, bestätigte externe Links, Größenbegrenzung und Quelltext-Fallback schützen die lokale Arbeitsweise. Renderer-Tests und ADR ergänzt.

- Erste eigenständige Rechercheansicht ergänzt, ohne das bestehende Editorlayout zu verändern. Das separate Fenster durchsucht den geladenen Arbeitsstand, zeigt Notizen schreibgeschützt, besitzt einen getesteten Vorwärts-/Rückwärtsverlauf und öffnet Notizen kontrolliert über die Dirty-State-Prüfung im Editor. Der spätere formatierte Renderer bleibt architektonisch getrennt.

- Die verbindliche Fortsetzung für browserähnliches Markdown-Lesen, Recherchekomfort und Wissensgraph wurde als eigene Roadmap dokumentiert. Die laufende V1-Stabilisierung bleibt zuerst; danach folgen Leseansicht, Navigation, Suchdarstellung, testbares Graphmodell, lokaler 2D-Graph und erst später eine optionale 3D-Darstellung.

- Der rekursive Ordnerscan übernimmt jetzt den Abbruchtoken des Vault-Wechsels, prüft ihn vor und während der Verzeichnisentdeckung und liefert eine plattformunabhängig deterministische Pfadreihenfolge. Repository-, Workspace- und Regressionstests sowie die Vault-Spezifikation wurden entsprechend erweitert.

- Wiederholtes Anwenden derselben Markdown-Formatierung erzeugt keine doppelten Umschließungen, Zeilenpräfixe oder Nummerierungen mehr. Vollständig oder nur inhaltlich ausgewählte Marker bleiben korrekt ausgewählt; Fett- und Kursivmarker werden eindeutig unterschieden. Verhalten, Regressionstests und Spezifikation wurden ergänzt.

- Abbruchanforderungen werden beim Vault-Laden jetzt vor und während der rekursiven Dateientdeckung sowie zwischen den einzelnen Lesevorgängen berücksichtigt. Die deterministische Pfadsortierung bleibt erhalten; ein vorab abgebrochener Scan ist mit echten Testdateien abgesichert.

- Der Vault-Scan erkennt Markdown-Endungen jetzt ausdrücklich ohne Unterscheidung der Groß-/Kleinschreibung und lädt Notizen deterministisch nach relativem Pfad. Dadurch sind `.md`, `.MD` und gemischte Schreibweisen unabhängig vom Dateisystem gleich behandelt; fremde Dateitypen, Unterordner und Reihenfolge sind mit echten Dateien getestet.

- Markdown-Formatierungen schützen UTF-16-Surrogatpaare an ungewöhnlichen Auswahlgrenzen. Nichtleere Auswahlen werden auf vollständige Zeichen erweitert; ein Cursor innerhalb eines Emojis wird davor positioniert. High-/Low-Surrogate-, Cursor- und Ergebnisauswahlfälle sind getestet und spezifiziert.

- Zeilenbasierte Markdown-Formatierungen erhalten jetzt jeden vorhandenen CRLF-, LF- und CR-Zeilentrenner einzeln. Gemischte Dokumente werden beim Anwenden von Überschriften, Zitaten, Listen und Checklisten nicht mehr stillschweigend vereinheitlicht; Aufzählungs- und Nummerierungsfälle sind getestet und spezifiziert.

- Startfehler nennen jetzt die Anzahl übergebener Argumente beziehungsweise den tatsächlich geprüften vollständigen Pfad. Fehlende Ziele, falsche Dateitypen und unzulässige relative Resolver-Pfade sind durch Diagnoseprüfungen abgesichert.

- Schutz vor externen Änderungen um einen SHA-256-Fingerabdruck des geladenen beziehungsweise gespeicherten Markdown-Texts ergänzt. Änderungen mit erhaltenem oder zurückgesetztem Zeitstempel werden erkannt und vor dem Speichern abgewiesen. Fingerabdruck-Übernahme, Neuerzeugung, Unicode und gleich lange Fremdänderungen sind getestet und dokumentiert.

- Navigation aus veralteten Suchergebnissen abgesichert. Der tatsächliche Inhaltstreffer wird gespeichert, vor der Auswahl bestätigt und nach Bearbeitungen am nächstgelegenen Vorkommen neu lokalisiert. Entfernte oder inkonsistente Treffer werden nicht falsch markiert. Unicode-, Mehrfachtreffer- und Verschiebungstests sowie Suchspezifikation ergänzt.

- Recent-Folder-Pfade syntaktisch normalisiert, dedupliziert und auf zehn Einträge begrenzt. Relative oder ungültige Altwerte werden ignoriert, nicht erreichbare absolute Vaults bleiben erhalten. Laufwerkswurzeln und Anzeigenamen sowie unabhängige Rückgabeschnappschüsse sind durch Tests abgesichert.

- JSON-Einstellungen gegen null-Einträge und leere Recent-Folder-Pfade abgesichert. Einstellungszugriff isoliert testbar gemacht; Lesen erzeugt keine Verzeichnisse. Abbruch wird vor Seiteneffekten und Dateiersetzung geprüft. Datei-, Unicode- und Abbruchtests ergänzt.

- Outline erkennt ATX-Überschriften mit bis zu drei führenden Leerzeichen und entfernt optionale Abschlussrauten aus dem Anzeigetitel. Originalpositionen bleiben erhalten; Unicode-, LF-/CRLF- und Codeblock-Regressionstests sowie Spezifikation ergänzt.

- Periodische Dateiprüfung auf die geöffnete Notiz begrenzt und gegen E/A- sowie Zugriffsfehler abgesichert. Vorübergehend nicht prüfbare Dateien werden ausdrücklich gemeldet und erneut geprüft; Entwurf und Dirty-State bleiben erhalten. Workspace- und echte Windows-Timertests ergänzt.

- Gemeinsame Positionszuordnung zwischen Notiz- und Editortext ergänzt. Suche, Outline und Wiki-Link-Navigation berücksichtigen unterschiedliche CRLF-/LF-Zeilenenden. Ungültige Bereiche werden abgewiesen. Kern- und RichTextBox-Regressionstests sowie Verhaltensspezifikation ergänzt.

- Suchtreffer speichern ihre tatsächliche UTF-16-Trefferlänge. Die Editor-Markierung verwendet diese Länge statt des veränderlichen Suchfelds; Rand-Leerzeichen verlängern die Auswahl nicht mehr. Tests für Titel-, Unicode- und LF-/CRLF-Treffer ergänzt.

- Suchvorschauen trennen UTF-16-Surrogatpaare an Ausschnittgrenzen nicht mehr und kennzeichnen auch ausgelassene Enden. Gleich bewertete, gleichnamige Treffer werden zusätzlich nach Pfad sortiert. Unicode-, LF-/CRLF-, Leertext- und Sortiertests sowie Suchspezifikation ergänzt.

- Backlinks zeigen jede Quelldatei nur einmal, auch bei mehrfachen Links oder Aliasvarianten. Gleichnamige Quellen in verschiedenen Ordnern bleiben getrennt; der erste gültige Link liefert den Kontext. Regressionstests und Backlink-Spezifikation ergänzt.

- Dirty-State wird anhand des tatsächlichen Textunterschieds ermittelt. Unveränderte Editorereignisse und die Rückkehr zum letzten Lade-/Speicherstand erzeugen keine unnötige Speicherabfrage. Regressionstests für Unicode, Zeilenumbrüche, leere Texte und asynchrone Speicherabschlüsse ergänzt.

- Optionaler Start mit einem Ordner oder einer Markdown-Datei ergänzt. Relative Pfade, Leerzeichen und Unicode werden unterstützt; explizite Ziele haben Vorrang vor dem gemerkten Vault. Der Dateistart wählt die Notiz im Editor und Baum. Ungültige Argumente werden vor dem Fensterstart abgewiesen. Kern- und Windows-Starttests sowie Anforderung FA-21 und Bedienungsdokumentation ergänzt.
- Auch die asynchrone Einstellungen-Initialisierung beim Programmstart schützt jetzt vor vorzeitigem Schließen des Fensters.

- Einzeilige Inline-Code-Spans werden bei Wiki-Links, Navigation und Backlinks ausgelassen. Gleich lange Backtick-Gruppen, Escapes, offene Marker und überlappende Linksyntax sind getestet. Originalpositionen bleiben erhalten; Mehrzeilen-Spans bleiben ausdrücklich außerhalb dieser V1-Teilmenge.

- Gemeinsame Erkennung eingefasster Markdown-Codeblöcke für Wiki-Links und Outline ergänzt. Beispiele erzeugen keine Backlinks oder fehlenden Linkziele mehr. Zeilengrenzen der Parser korrigiert; Unicode-, LF-/CRLF-, Marker- und Workspace-Regressionstests hinzugefügt.

- Vault-Übergänge gegen überlappende Öffnungs-, Bearbeitungs- und Schreibbefehle abgesichert. Laufende Schreibaufträge verhindern den Wechsel; Lade- und Einstellungsphase bleiben geschützt. WinForms sperrt Bedienung und Schließen während des Ladens. Verzögerte Application- und Windows-Fenstertests ergänzt.

- Wiki-Link-Auflösung behandelt die optionale .md-Endung auf beiden Seiten gleich. Explizite Ordnerpfade werden nicht mehr auf gleichnamige Dateien umgelenkt; mehrdeutige Kurzlinks bleiben unaufgelöst. Reihenfolge-, Unicode-, Pfad- und Backlink-Regressionstests sowie gemeinsame Spezifikation ergänzt.

- Gezielter Repository-Einzelzugriff für das Neuladen ergänzt. Gesperrte Nachbardateien beeinflussen den Vorgang nicht mehr; verspätete Ergebnisse werden nach Bearbeitung, Vault-Wechsel oder Abbruch verworfen. Pfad-, Datei- und Application-Regressionstests ergänzt.

- „Nicht speichern“ verwirft lokale Änderungen nun auch im geladenen Notizmodell. Der Workspace erhält die letzte Lade-/Speicherbasis, stellt Links wieder her und verhindert Verwerfen während laufender Speicherung. Regressionstests und Verhaltensspezifikation ergänzt.

- Speichern verwendet einen unabhängigen Textschnappschuss; neuere Editoränderungen bleiben dirty. Doppelte laufende Speicherbefehle werden abgewiesen, Fehler und Abbruch erlauben Wiederholung. Verzögerte Application-Tests und Speicherspezifikation ergänzt.

- Vault-Wechsel übernimmt Notizen, Ordnerbaum und Recent Folders erst nach erfolgreichem Laden und Speichern der vorbereiteten Einstellungen. Fehler- und Abbruchtests sichern den Erhalt des bisherigen Dirty-State ab.

- Neuerzeugung von Notizen verwendet exklusives Veröffentlichen ohne Überschreiben; konkurrierende Namenskollisionen werden begrenzt wiederholt. Tests für gleichzeitige Erstellung, belegte Verzeichnisnamen und Abbruch ergänzt.

- Fensterschließen wartet auf asynchrone Speicherbestätigung; doppelte Schließversuche werden gesperrt. Windows-Regressionstest mit verzögertem Abschluss und Abbruch ergänzt.

- Markdown-Zeilenformatierung erhält angrenzende CRLF-Zeilenumbrüche und respektiert exklusive Auswahlenden; LF-/CRLF-Regressionstests ergänzt.

- Reservierte Windows-Namen mit Erweiterung korrekt bereinigt (`CON.md` → `CON_.md`); Namensregeln plattformunabhängig getestet.

- Sichere Erstellung und Anzeige leerer Vault-Unterordner im WinForms-Baum ergänzt.
- Beim Erzeugen einer neuen Notiz wird der Dirty-State vor dem Wechsel abgefragt.
- Markdown-Toolbar um Durchstreichen, Zitat und nummerierte Listen ergänzt.
- Windows-reservierte Datei- und Ordnernamen sowie problematische Endzeichen werden sicher normalisiert.
- Kontrolliertes Neuladen extern geänderter Notizen mit Dirty-State-Abfrage ergänzt.
- Windows-CI um Release-Publish und eine V1-Release-Checkliste erweitert.
- Der veröffentlichte WinForms-Stand wird zusätzlich als ZIP-CI-Artefakt bereitgestellt.
- README um den Release-Candidate-Status und die V1-Checkliste ergänzt.

- zerstörte Zeilenumbruch- und Zeichenliterale im Markdown-Formatter repariert
- paketfreien Test-Harness um Formatter-, Unicode-, CRLF- und Backlink-Prüfungen erweitert
- fehlende XML-Dokumentation für `MarkdownFormatKind` ergänzt
- WinForms-Projekt auf das moderne `Microsoft.NET.Sdk` umgestellt
- Vault-Pfade beim Speichern und Erzeugen neuer Notizen gegen Path Traversal abgesichert
- .NET-8-SDK-Band in `global.json` reproduzierbarer festgelegt
- Speichern von Markdown-Notizen gegen externe Änderungen abgesichert
- Notizen und lokale Einstellungen werden zunächst atomar über temporäre Dateien geschrieben
- Repository-Tests für erfolgreiches Speichern und erkannte externe Änderungen ergänzt
- Nicht auflösbare Wiki-Links werden im Info-Bereich angezeigt
- Warnung bei extern geänderten oder gelöschten Notizen ergänzt
- Such- und Backlink-Treffer zeigen jetzt auch ihre relativen Pfade an
- Windows-GitHub-Action für Restore, Build und Tests ergänzt
- Auch neu angelegte Notizen werden atomar geschrieben

## 2026-07-04

- added complete .NET 8 WinForms solution for SASD Notes
- added domain, application, infrastructure and app projects
- added package-free verification harness project
- added sample vault for quick testing
- kept the project documentation and UI concept screenshot

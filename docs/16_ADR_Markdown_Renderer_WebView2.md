# ADR 001 – Markdown-Leseansicht mit Markdig und WebView2

Datum: 2026-10-25  
Status: Angenommen

## Kontext

Die bestehende WinForms-Oberfläche eignet sich gut zum Bearbeiten von
Markdown-Quelltext. Die neue Rechercheansicht benötigt zusätzlich eine
formatierte, browserähnliche und weiterhin lokale Leseansicht.

Ein eigener vollständiger Markdown-Parser und HTML-Renderer würde bereits
gelöste Syntaxprobleme erneut implementieren. Ein Browser-Control darf
andererseits weder die Domain noch die Application-Schicht durchdringen und
keine unkontrollierte Netz- oder Skriptausführung ermöglichen.

## Entscheidung

- Die Application-Schicht definiert mit `IMarkdownHtmlRenderer` ausschließlich
  den benötigten Renderer-Vertrag.
- Die Infrastructure-Schicht implementiert ihn mit Markdig.
- Das WinForms-Projekt verwendet WebView2 zur Darstellung des fertigen
  HTML-Dokuments.
- Die Domain bleibt frei von HTML-, Browser-, Markdig- und
  WebView2-Abhängigkeiten.
- Markdig wird zunächst in Version `1.5.0` fest referenziert.
- Das WebView2-SDK wird zunächst in der stabilen Version `1.0.4258.31`
  fest referenziert.
- Paketupdates erfolgen bewusst in eigenen Änderungen mit Build- und
  Regressionstest.

## Sicherheitsregeln

- Rohes HTML aus Markdown wird im Renderer deaktiviert.
- Jedes Dokument erhält eine restriktive Content-Security-Policy.
- JavaScript aus Dokumenten, Hostobjekte und Webnachrichten sind in WebView2
  deaktiviert.
- Für die Suchhervorhebung führt die Anwendung ausschließlich ein festes,
  intern erzeugtes Hostskript über `ExecuteScriptAsync` aus. Der Suchtext wird
  davor als JSON-Zeichenfolge kodiert und ausschließlich über `textContent`
  in den DOM übernommen.
- Automatische Navigation außerhalb des erzeugten `about:blank`-Dokuments wird
  abgebrochen.
- HTTP-, HTTPS- und Mail-Links werden nur nach einem Benutzerdialog an das
  Betriebssystem übergeben.
- Netzwerkbilder, Frames, Objekte und externe Schriftarten werden nicht geladen.
- Bei fehlender WebView2-Runtime bleibt die schreibgeschützte
  Markdown-Quelltextansicht verfügbar.
- Dokumente oberhalb der sicheren `NavigateToString`-Größe verwenden ebenfalls
  die Quelltextansicht.

## Folgen

Die Rechercheansicht erhält eine hochwertige Markdown-Darstellung, ohne die
Editoroberfläche umzubauen. Markdig und WebView2 sind zusätzliche externe
Abhängigkeiten, bleiben jedoch auf Infrastructure beziehungsweise WinForms
begrenzt. Das repositoryweite `Directory.Build.targets` entfernt bei reinen
WinForms-Projekten die vom WebView2-Paket zusätzlich eingetragene, unbenutzte
WPF-Referenz. Dadurch bleibt der Release-Publish frei vom bekannten
`WindowsBase`-Versionskonflikt, ohne WPF in die Anwendung aufzunehmen.

Der UI-unabhängige `ResearchMarkdownService` verwendet den vorhandenen
`WikiLinkParser` und `LinkResolver`. Nur eindeutig aufgelöste Wiki-Links
werden vor dem Rendern in Markdown-Links mit dem internen Schema
`sasd-note:` umgewandelt. Fehlende und mehrdeutige Ziele bleiben sichtbar in
ihrer ursprünglichen Wiki-Link-Syntax.

WebView2 bricht die Browsernavigation für dieses interne Schema ab und öffnet
ausschließlich eine bereits im Workspace geladene Notiz. Der Markdig-Renderer
kennt dadurch weder Vaults noch Linkauflösung, und die UI führt keine zweite
fachliche Auflösungsregel ein.


## Suchtreffer-Hervorhebung

Inhaltstreffer werden nicht in den Markdown-Quelltext hineinformatiert. Der
bereits vorhandene `SearchHitLocator` lokalisiert einen Treffer erneut im
aktuellen Arbeitsstand. Ein kleiner WinForms-Adapter markiert danach den ersten
exakt passenden sichtbaren Textknoten mit einem semantischen `mark`-Element
und scrollt ihn in die Mitte der Leseansicht.

Liegt ein Treffer ausschließlich in Markdown-Steuerzeichen oder über mehreren
HTML-Textknoten, bleibt das Dokument unverändert und die Statusleiste erklärt,
dass keine Markierung möglich war. Der Quelltext-Fallback markiert weiterhin
den exakt lokalisierten UTF-16-Bereich direkt im `RichTextBox`-Control.

## Gliederungsnavigation

Die Rechercheansicht bezieht ihre Überschriften über
`NotesWorkspaceService.GetOutline` und damit aus demselben `IOutlineParser` wie
die bestehende Editoransicht. Die WinForms-Schicht implementiert keine eigene
Markdown-Erkennung.

Für die formatierte Ansicht ordnet ein festes Hostskript Ebene, sichtbaren Titel
und Wiederholungsnummer den gerenderten `h1`- bis `h6`-Elementen zu. Das
verhindert eine Indexverschiebung durch zusätzliche Überschriftsformen des
Renderers. Der Titel wird als JSON-Zeichenfolge kodiert, ausschließlich über
`textContent` verglichen und niemals als HTML eingesetzt. Das Skript führt nur
Markierung und `scrollIntoView` aus. Während
`NavigateToString` noch läuft, bleibt die Gliederung gesperrt, damit kein Sprung
versehentlich das vorherige Dokument anspricht. Der Quelltext-Fallback verwendet
direkt den vom Parser gelieferten Originalindex.

## Backlink-Navigation

Die Rechercheansicht ruft Rückverweise ausschließlich über
`NotesWorkspaceService.GetBacklinks` ab. Damit bleiben Arbeitsstand-Schnappschuss,
`BacklinkService` und der gemeinsame `ILinkResolver` maßgeblich. Die WinForms-
Schicht zeigt `BacklinkInfo`-Einträge an und navigiert anhand des gelieferten
relativen Quellpfads über denselben `ResearchNavigationService`, der auch
Suchtreffer und Wiki-Links in den Vorwärts-/Rückwärtsverlauf übernimmt.

## Ausgehende und nicht aufgelöste Links

`OutgoingLinkService` erzeugt aus den bereits geparsten Wiki-Links eine
UI-unabhängige Liste in Dokumentreihenfolge. Für jedes Vorkommen bleiben
Zielangabe, Alias und Rohtext erhalten; der gemeinsame `ILinkResolver` ergänzt
nur bei eindeutiger Auflösung den relativen Zielpfad. Fehlende und mehrdeutige
Ziele bleiben damit sichtbar, ohne eine falsche Navigation anzubieten.

Die WinForms-Schicht formatiert diese Ergebnisse, navigiert ausschließlich zu
bereits geladenen relativen Zielpfaden und meldet offene Ziele in der
Statusleiste. Sie parst oder löst keine Wiki-Link-Syntax selbst auf.

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
- JavaScript, Hostobjekte und Webnachrichten sind in WebView2 deaktiviert.
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

Wiki-Links erscheinen im ersten Renderer-Schritt als Text. Ihre anklickbare
Darstellung wird anschließend auf Basis des vorhandenen `WikiLinkParser` und
`LinkResolver` ergänzt; der Renderer darf keine konkurrierende
Linkauflösungslogik einführen.

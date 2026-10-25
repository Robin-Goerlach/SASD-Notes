# Roadmap – Lesen, Recherchieren und Wissensgraph

Version: 0.1  
Datum: 2026-10-25  
Status: Geplante Fortsetzung nach der stabilen V1-Basis

---

## 1. Ziel

SASD Notes soll nicht nur Markdown-Dateien zuverlässig bearbeiten, sondern
vorhandenes Wissen ebenso gut lesbar, durchsuchbar und navigierbar machen.
Dieser Funktionsblock ist verbindlich vorgesehen. Die laufende Stabilisierung
der V1-Basis wird dafür nicht abgebrochen.

Die Reihenfolge lautet:

1. V1-Basis weiter stabilisieren und manuell abnehmen,
2. Leseansicht und dokumentbezogene Navigation,
3. Rechercheoberfläche und Suchkomfort,
4. UI-unabhängiges Wissensgraph-Modell,
5. lokaler zweidimensionaler Graph,
6. optionale räumliche beziehungsweise dreidimensionale Darstellung.

---

## 2. Browserähnliche Leseansicht

Die Leseansicht stellt Markdown formatiert dar und bleibt vollständig in den
lokalen Vault eingebunden. Browserähnlich bezeichnet dabei die Bedienung, nicht
den Zugriff auf das Internet.

Geplante Mindestfunktionen:

- eine eigenständige Rechercheansicht, welche die vorhandene Editoroberfläche
  unverändert lässt,
- formatierte Markdown-Darstellung über einen austauschbaren Renderer-Vertrag,
- sichere WebView-Darstellung mit stets verfügbarer Quelltext-Rückfallebene,
- Suche, Trefferliste und schreibgeschützte Dokumentanzeige in einem separaten
  Fenster,
- kontrollierter Wechsel einer gelesenen Notiz in den Editor,
- Umschalten zwischen Bearbeiten und Lesen,
- optional eine geteilte Editor- und Vorschauansicht,
- Vorwärts- und Zurücknavigation innerhalb des Vaults,
- anklickbare Wiki-Links für eindeutig aufgelöste Ziele (umgesetzt),
- Sprünge zu Überschriften über die Gliederung (umgesetzt),
- sichtbare und automatisch zentrierte Inhaltstreffer im gelesenen Dokument (umgesetzt),
- Backlinks als Navigationshilfe; die Outline-Navigation ist umgesetzt,
- verständlicher Ersatzhinweis, falls der Renderer nicht verfügbar ist.

Die vorhandenen Regeln von `WikiLinkParser` und `LinkResolver` bleiben
maßgeblich. Die Vorschau darf keine zweite, abweichende Linkauflösung
implementieren.

---

## 3. Rechercheoberfläche

Die bestehende Volltextsuche bleibt zunächst bewusst einfach. Ihre Darstellung
und Navigation werden vor einem komplexen Suchindex verbessert.

Geplante Schritte:

- Suchtreffer nach Titel, Pfad und Inhalt erkennbar unterscheiden,
- Trefferbegriffe und Kontextausschnitte hervorheben,
- Such- und Navigationsverlauf bereitstellen,
- Treffer ohne Verlust der aktuellen Arbeitsposition öffnen,
- Backlinks, ausgehende Links und nicht aufgelöste Links gemeinsam nutzbar
  machen,
- später Filter nach Ordnern, Tags oder weiteren Metadaten ergänzen.

Ein Suchindex ist erst sinnvoll, wenn reale Vault-Größen die einfache Suche
messbar begrenzen.

---

## 4. Wissensgraph

Vor einer grafischen Darstellung entsteht in der Application-Schicht ein
testbares Modell aus Knoten und Kanten:

- eine vorhandene Notiz ist ein Knoten,
- ein aufgelöster Wiki-Link ist eine gerichtete Kante,
- nicht aufgelöste Links werden ausdrücklich gekennzeichnet,
- mehrdeutige Links werden nicht stillschweigend einer Notiz zugeordnet,
- Knotenschlüssel verwenden die bereits festgelegte Pfad- und
  Linkauflösungslogik.

Der erste sichtbare Graph soll lokal um die aktive Notiz arbeiten. Eine
begrenzte Tiefe und Filter verhindern, dass große Vaults sofort zu einem
unlesbaren Netz werden. Erst danach folgt ein globaler Graph.

Eine dreidimensionale Ansicht bleibt als bewusste spätere Ausbaustufe erhalten.
Sie soll denselben fachlichen Graphen verwenden und keine eigene
Linkinterpretation einführen.

---

## 5. Architekturgrenzen

### Domain

Die Domain enthält weiterhin keine HTML-, Browser-, JavaScript- oder
WinForms-Abhängigkeiten.

### Application

Die Application-Schicht koordiniert:

- Lese- und Navigationsanwendungsfälle,
- Navigationsverlauf,
- Such- und Trefferlogik,
- Aufbau des Wissensgraph-Modells,
- abstrakte Schnittstellen für einen Markdown-Renderer.

### Infrastructure

Eine spätere Renderer-Implementierung darf eine etablierte Markdown-Bibliothek
verwenden. Die konkrete Bibliothek wird vor der Einführung in einer
nachvollziehbaren Architekturentscheidung festgehalten.

### WinForms

Die Oberfläche hostet die Darstellung, verarbeitet Benutzeraktionen und leitet
Wiki-Link-Navigation an die Application-Schicht weiter. Ein möglicher
WebView-Adapter bleibt vollständig auf die UI-Schicht begrenzt.

---

## 6. Sicherheitsregeln für die Vorschau

- Markdown-Dateien bleiben die einzige maßgebliche Inhaltsquelle.
- Eingebettetes HTML und Skripte werden standardmäßig nicht ausgeführt.
- Die Vorschau führt keine automatische Netzwerkkommunikation aus.
- Lokale Datei- und Ressourcenpfade dürfen die Vault-Grenze nicht verlassen.
- Externe URLs werden nur nach einer ausdrücklichen Benutzeraktion geöffnet.
- Inhalte einer Notiz werden nicht als ausführbarer Programmcode behandelt.
- Ein Darstellungsfehler darf den Markdown-Quelltext weder verändern noch
  überschreiben.

---

## 7. Abnahmekriterien für den ersten Lesemodus

Der erste Funktionsstand ist abnahmefähig, wenn:

- eine geöffnete Notiz zuverlässig als formatierter Markdown-Inhalt erscheint,
- Bearbeiten und Lesen ohne Inhaltsverlust gewechselt werden können,
- Wiki-Links dieselben Ziele wie im Editor öffnen,
- Zurück- und Vorwärtsnavigation reproduzierbar funktionieren,
- die Darstellung ohne Internetverbindung nutzbar bleibt,
- schädliches HTML oder Skriptinhalt nicht ausgeführt wird,
- Renderer- und Navigationslogik ohne gestartete UI getestet werden können,
- Windows-Build, Kernprüfungen und WinForms-Prüfungen erfolgreich bleiben.

---

## 8. Bewusste Zurückstellung

Folgende Punkte werden nicht zusammen mit dem ersten Lesemodus umgesetzt:

- vollständige Browserfunktionalität,
- freie Navigation zu beliebigen Webseiten,
- ein komplexer Suchindex,
- globale Graphanimation für sehr große Vaults,
- dreidimensionale Darstellung,
- Plugin-Schnittstellen für Renderer oder Graphen.

Damit bleibt der nächste Ausbau überschaubar, ohne die langfristige
Wissens- und Rechercheausrichtung zu verlieren.

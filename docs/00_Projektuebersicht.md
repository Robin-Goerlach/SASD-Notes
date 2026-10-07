# SASD Notes – Projektübersicht

Version: 0.2  
Status: Repository- und Architekturgrundlage

## Zweck

**SASD Notes** ist eine lokale Markdown-basierte Wissens- und Notizanwendung mit internen Wiki-Links, strukturierten Themenordnern, Suche, Backlinks und editororientierten Arbeitsabläufen.

Die erste und weiterhin priorisierte Implementierung basiert auf **C# / .NET 8 / Windows Forms**.

Das Repository wird jedoch von Anfang an so strukturiert, dass später zusätzliche Implementierungen — beispielsweise in **C++**, **Java** oder **Swift** — parallel entwickelt werden können, ohne dass dafür die Produktdokumentation oder das Markdown-Datenformat dupliziert werden müssen.

## Leitidee

SASD Notes ist **ein Produkt mit einem gemeinsamen Daten- und Verhaltensvertrag**, aber möglicherweise mehreren idiomatischen Implementierungen.

Gemeinsam bleiben insbesondere:

- Markdown-Dateien als primäre Datenquelle
- Vault- und Ordnersemantik
- Wiki-Link-Syntax
- Backlink-Verhalten
- Such- und Outline-Grundregeln
- Interoperabilitätsanforderungen

Nicht vereinheitlicht werden müssen dagegen sprach- oder plattformspezifische Details wie WinForms, CMake, JavaFX oder SwiftUI/AppKit.

## Repository-Struktur

```text
docs/          Produkt- und Implementierungsdokumentation
spec/          sprachneutrale Verhaltens- und Kompatibilitätsspezifikationen
src/dotnet/    aktuelle Referenzimplementierung (.NET 8 / WinForms)
src/cpp/       reserviert für eine spätere C++-Implementierung
src/java/      reserviert für eine spätere Java-Implementierung
src/swift/     reserviert für eine spätere Swift-Implementierung
tests/dotnet/  automatisierte Tests der .NET-Implementierung
```

Weitere Sprachverzeichnisse werden erst angelegt, wenn eine konkrete Implementierung begonnen wird.

## Aktueller Fokus

Die Mehrsprachenstruktur bedeutet ausdrücklich **nicht**, dass V1 gleichzeitig in mehreren Sprachen entwickelt werden soll.

Der aktuelle Fokus bleibt:

1. .NET-8-/WinForms-Build-Baseline stabilisieren
2. Vault- und Ordnerverwaltung
3. Markdown-Editor
4. Formatierungshilfen
5. Wiki-Links und Backlinks
6. Suche
7. V1-Hardening

## Codex-Unterstützung

Die Datei `/AGENTS.md` definiert repositoryweite Arbeits- und Sicherheitsregeln für autonome Coding Agents.

Sprachspezifische Ergänzungen liegen innerhalb des jeweiligen Implementierungsbaums, zum Beispiel `/src/dotnet/AGENTS.md`.

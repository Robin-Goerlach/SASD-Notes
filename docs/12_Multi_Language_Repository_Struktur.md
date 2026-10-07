# SASD Notes – Multi-Language-Repository-Struktur

## 1. Ziel

Dieses Dokument legt fest, wie SASD Notes künftig mehrere technische Implementierungen im selben Repository aufnehmen kann.

Die Struktur soll vier Ziele gleichzeitig erfüllen:

1. Die aktuelle .NET-8-/WinForms-Entwicklung darf nicht ausgebremst werden.
2. Spätere C++-, Java-, Swift- oder andere Versionen sollen einen klaren Platz erhalten.
3. Produktanforderungen und Markdown-Kompatibilität dürfen nicht pro Sprache auseinanderlaufen.
4. Jede Sprache soll trotzdem idiomatisch entwickelt werden können.

## 2. Entscheidung

Der Quellcode wird nach Implementierungssprache unter `src/<language>/` gegliedert.

Dies folgt der bereits in anderen SASD-Repositories verwendeten Richtung, insbesondere den sprachbezogenen Unterordnern von SASD Math Toolkit und SASD Graphics Toolkit.

Zielstruktur:

```text
SASD-Notes/
├─ AGENTS.md
├─ README.md
├─ CONTRIBUTING.md
├─ docs/
├─ spec/
├─ src/
│  ├─ dotnet/
│  ├─ cpp/
│  ├─ java/
│  └─ swift/
└─ tests/
   ├─ dotnet/
   ├─ cpp/
   ├─ java/
   └─ swift/
```

Testverzeichnisse für noch nicht gestartete Implementierungen müssen nicht künstlich mit Platzhaltercode gefüllt werden.

## 3. Warum nicht ein Repository pro Sprache?

Getrennte Repositories würden zwar Buildsysteme voneinander isolieren, hätten aber früh einen hohen Synchronisationsaufwand.

Für SASD Notes sollen insbesondere folgende Dinge gemeinsam bleiben:

- Lastenheft und Produktvision
- Markdown-Dateiformat
- Wiki-Link-Semantik
- Vault-Verhalten
- Backlinks
- Suchgrundregeln
- Beispiel-Vaults und spätere Conformance-Tests

Ein gemeinsames Repository macht Abweichungen früh sichtbar.

Sollte eine Implementierung später organisatorisch so groß werden, dass ein eigenes Repository sinnvoll ist, kann dies erneut bewertet werden. Die jetzige Struktur erzwingt diese spätere Entscheidung nicht.

## 4. Warum `src/dotnet` statt `src/csharp`?

Die Implementierung wird nach ihrem technischen Ökosystem benannt, nicht nur nach der Sprache.

`dotnet` umfasst beispielsweise:

- C#
- .NET SDK
- Windows Forms
- .NET-Projekt- und Paketstruktur

Dies entspricht zudem der Struktur des SASD Math Toolkit und SASD Graphics Toolkit.

## 5. Gemeinsame Spezifikationen

Die neue Ebene `spec/` enthält sprachneutrale Verträge.

Beispiele für spätere Dokumente:

```text
spec/
├─ README.md
├─ vault-paths.md
├─ wiki-links.md
├─ backlinks.md
├─ search.md
├─ markdown-formatting.md
└─ fixtures/
```

Die Spezifikation beschreibt **beobachtbares Verhalten**, nicht Implementierungsdetails.

Beispiel:

> `[[Folder/Note]]` bezeichnet eine Note relativ zum Vault und verwendet in der logischen Darstellung einen normalen Schrägstrich.

Nicht in die gemeinsame Spezifikation gehört beispielsweise:

> C# verwendet hierfür `Path.GetRelativePath`.

## 6. .NET-Referenzimplementierung

Die bestehende lokale Solution soll beim nächsten Synchronisieren in folgende Struktur überführt werden:

```text
src/dotnet/
├─ Sasd.Notes.sln
├─ Sasd.Notes.Domain/
├─ Sasd.Notes.Application/
├─ Sasd.Notes.Infrastructure/
└─ Sasd.Notes.App.WinForms/

tests/dotnet/
└─ Sasd.Notes.Tests/
```

Das bisherige Clean-/Layered-Architecture-Modell bleibt damit vollständig erhalten.

Die Änderung ist primär eine zusätzliche äußere Gruppierung nach Implementierungssprache.

## 7. C++-Implementierung

Eine spätere C++-Version erhält ihr eigenes Buildsystem und ihre eigene interne Architektur.

Denkbare Struktur:

```text
src/cpp/
├─ CMakeLists.txt
├─ include/
├─ src/
└─ app/
```

Die konkrete UI-Technologie wird erst entschieden, wenn diese Implementierung tatsächlich begonnen wird.

Es wird nicht vorzeitig versucht, WinForms-Klassen oder .NET-Schichten künstlich 1:1 in C++ nachzubauen.

## 8. Java-Implementierung

Eine spätere Java-Version darf ein idiomatisches Gradle- oder Maven-Projekt verwenden.

Beispiel:

```text
src/java/
├─ build.gradle.kts
└─ src/
   ├─ main/
   └─ test/
```

Die genaue Struktur wird erst mit Start des Java-Ports festgelegt.

## 9. Swift-Implementierung

Die Swift-Version wird voraussichtlich eine native macOS-Anwendung.

Sie darf deshalb SwiftPM/Xcode sowie SwiftUI oder AppKit idiomatisch verwenden.

Die gemeinsame Anforderung ist nicht eine identische UI-Architektur, sondern dieselbe Markdown- und Vault-Kompatibilität.

## 10. Agentenregeln

`/AGENTS.md` enthält die repositoryweiten Regeln.

Jede aktive Implementierung kann eine eigene untergeordnete Datei besitzen:

```text
src/dotnet/AGENTS.md
src/cpp/AGENTS.md
src/java/AGENTS.md
src/swift/AGENTS.md
```

Die untergeordneten Regeln dürfen Build-, Style- und Architekturdetails ergänzen.

Sie dürfen die gemeinsamen Daten- und Sicherheitsregeln nicht aufheben.

## 11. Buildsysteme

Es wird bewusst kein künstliches gemeinsames Meta-Buildsystem für alle Sprachen eingeführt.

Stattdessen besitzt jede Implementierung ihre nativen Werkzeuge:

- .NET: `dotnet build`, `dotnet test`
- C++: später CMake/CTest
- Java: später Gradle oder Maven
- Swift: später SwiftPM/Xcode

Ein repositoryweites Build-Skript kann später hinzukommen, sobald mindestens zwei aktive Implementierungen existieren und ein echter Nutzen besteht.

## 12. Migration des derzeit lokalen .NET-Codes

Der auf dem Entwicklungsrechner vorhandene .NET-Code wurde bisher unter:

```text
src/Sasd.Notes.*
tests/Sasd.Notes.Tests
```

entwickelt.

Beim nächsten Abgleich mit dieser Repository-Struktur sollte er verschoben werden nach:

```text
src/dotnet/Sasd.Notes.*
tests/dotnet/Sasd.Notes.Tests
```

Die Projekt- und Solution-Referenzen müssen anschließend überprüft werden.

Danach sind mindestens auszuführen:

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet test
```

Der Umbau gilt erst als abgeschlossen, wenn die .NET-Solution wieder fehlerfrei referenziert und gebaut werden kann.

## 13. Vermeidung von Over-Engineering

Die Multi-Language-Struktur ist eine **Repository-Organisation**, kein Auftrag zur sofortigen Mehrfachimplementierung.

Für V1 gilt weiterhin:

> Erst die .NET-8-/WinForms-Version korrekt und belastbar entwickeln.

C++, Java und Swift bleiben reservierte Erweiterungspfade, bis ein konkreter Port begonnen wird.

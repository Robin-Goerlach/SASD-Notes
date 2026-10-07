# AGENTS.md — SASD Notes

<<<<<<< HEAD
## Zweck

SASD Notes ist eine lokale Markdown-Anwendung für Windows. Markdown-Dateien
bleiben die primäre Datenquelle und müssen ohne SASD Notes in anderen Editoren
weiterverwendbar sein.

## Technischer Rahmen

- C# und .NET 8
- Windows Forms auf `net8.0-windows`
- Visual Studio 2022
- moderne SDK-Projektdateien mit `Microsoft.NET.Sdk`
- keine ungefragte Umstellung auf .NET 9 oder .NET 10

## Schichten

- `Sasd.Notes.Domain` enthält fachliche Modelle und darf keine UI-Abhängigkeit
  besitzen.
- `Sasd.Notes.Application` enthält Use Cases und testbare Anwendungslogik.
- `Sasd.Notes.Infrastructure` enthält Dateisystem- und Persistenzadapter.
- `Sasd.Notes.App.WinForms` enthält Formulare, Controls und dünne UI-Handler.
- `Sasd.Notes.Tests` enthält paketfreie deterministische Kernprüfungen.

Die Markdown-Dateien im Vault sind maßgeblich. Eine spätere Datenbank darf nur
abgeleitete Daten wie Cache oder Suchindex speichern.

## Arbeitsregeln

- Bestehenden Code zuerst analysieren und gezielt verbessern.
- Verständliche Namen, XML-Dokumentation für öffentliche API und sinnvolle
  `//`-Kommentare bei Verarbeitungsschritten verwenden.
- Dateipfade mit `System.IO.Path` behandeln und jeden benutzerbestimmten Pfad
  gegen den geöffneten Vault prüfen.
- Keine Notizen ungefragt löschen oder außerhalb des Vaults überschreiben.
- Neue Parser-, Link-, Such-, Pfad- und Formatterlogik mit deterministischen
  Tests abdecken.
- Bei Compilerfehlern zuerst den ersten echten Fehler beheben und danach neu
  prüfen.

## Verifikation

Im Repository-Root ausführen:

```text
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project tests/Sasd.Notes.Tests/Sasd.Notes.Tests.csproj
```

Der paketfreie Test-Harness muss zusätzlich mit `dotnet run` ausgeführt werden,
weil `dotnet test` ohne Test-SDK nur den Testprojekt-Build verifizieren kann.

=======
## Purpose

SASD Notes is a local-first Markdown knowledge and note-taking application. This repository is intentionally structured so that the same product can be implemented in multiple programming languages while sharing one product specification, file-format contract, and documentation set.

This file defines repository-wide rules for autonomous coding agents such as OpenAI Codex.

Codex should work independently on routine implementation, refactoring, testing, documentation, and build fixes when the task is clear and these rules are respected. Ask only when a decision would be destructive, irreversible, security-sensitive, licensing-related, or would materially redefine the product.

## Repository model

The repository is organized by implementation language:

```text
src/
├─ dotnet/
├─ cpp/
├─ java/
└─ swift/

tests/
├─ dotnet/
├─ cpp/
├─ java/
└─ swift/

spec/
docs/
```

Not every language directory must contain a complete implementation at all times. The current reference implementation is the .NET 8 / Windows Forms version under `src/dotnet/`.

Language-specific instructions belong in a nested `AGENTS.md`, for example `src/dotnet/AGENTS.md`. A nested file may add or refine rules for its subtree but must not contradict the product and safety rules in this root file.

## Shared product contract

All implementations should converge on the same user-visible semantics for:

- Markdown files as the source of truth
- vault / working-folder behavior
- nested topic folders
- wiki links such as `[[Note]]`, `[[Note|Alias]]`, and `[[Folder/Note]]`
- backlinks
- search behavior
- Markdown formatting helpers
- document outline
- recent folders / recent vaults
- non-destructive file handling

When semantics become stable enough to be shared across languages, document them under `spec/` rather than copying rules independently into each implementation.

## Core principles

### Markdown files are authoritative

User notes are normal `.md` files. Do not make a database the authoritative store for note contents.

Databases may later be used for caches, indexes, or derived metadata only.

### Local-first

The application must remain useful offline. Do not add telemetry, cloud accounts, remote APIs, or online dependencies unless explicitly requested.

### Preserve user data

Never silently delete or overwrite user notes. Validate paths, keep writes inside the selected vault, handle access errors, and avoid path traversal.

### Correctness before optimization

Prefer clear, testable, conservative implementations. Performance work may follow once behavior is correct and measured.

### Avoid premature platform unification

Different language implementations may use native UI frameworks and idiomatic project layouts. Do not force a common lowest-level architecture across C#, C++, Java, and Swift merely for visual symmetry.

What must remain shared is the product behavior and on-disk compatibility, not identical source code.

## Documentation

Repository-wide product and architecture documents live in `docs/`.

Cross-language behavioral contracts live in `spec/`.

Implementation-specific design notes should clearly name their implementation, for example:

- .NET 8 / WinForms
- C++ desktop implementation
- Java desktop implementation
- Swift/macOS implementation

Do not silently rewrite one implementation-specific document as if it applied to all platforms.

## Testing

Each implementation owns its language-specific automated tests under `tests/<language>/`.

New parser, link-resolution, search, path-handling, and formatting behavior should be covered by deterministic tests whenever practical.

Where multiple implementations exist, equivalent conformance cases should be derived from the shared contracts in `spec/`.

## Autonomous work policy

Codex may, without asking:

- inspect repository files and documentation
- edit source and test files
- add tests
- improve documentation
- refactor within the requested scope
- run safe build and test commands
- run `git status`, `git diff`, `git log`, and `git show`
- work on a feature branch
- make focused incremental commits when permitted

Codex must not, without explicit authorization:

- force-push
- rewrite shared history
- delete large groups of user files
- change the project license
- publish releases or packages
- add telemetry or cloud services
- perform destructive data migrations
- install system-wide software
- replace the currently selected runtime/toolchain for an existing implementation

## Git workflow

Prefer feature branches and small reviewable commits.

Before considering code work complete, review the final diff and run the language-specific verification commands documented by that implementation.

Never claim a build or test run succeeded unless it was actually executed.

## Architecture review

Before finalizing changes, check:

- Is shared product behavior being duplicated instead of documented in `spec/`?
- Is implementation-specific code leaking into repository-wide contracts?
- Is user data still plain Markdown and portable?
- Are file operations non-destructive?
- Can core behavior be tested without starting the UI?
- Is the change more complicated than the current milestone requires?

## Current roadmap priority

The .NET implementation is currently the active reference implementation. Its near-term sequence remains:

1. clean build baseline
2. vault / folder handling
3. editor and save workflow
4. Markdown formatting helpers
5. wiki links and backlinks
6. search
7. usability and hardening

Future C++, Java, Swift, or other implementations should begin only from shared documented behavior and should not block progress of the current .NET version.

## Definition of done

A task is complete when the requested behavior is implemented, relevant tests and documentation are updated, architecture boundaries remain understandable, and the appropriate build/test commands succeed or any inability to run them is stated explicitly.

The guiding principle is:

> One product contract, multiple idiomatic implementations, portable Markdown data.
>>>>>>> origin/main

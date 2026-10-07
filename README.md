# SASD Notes

SASD Notes is a local-first note-taking and knowledge-base application built around **normal Markdown files**, folder-based organization, wiki-style links, search, backlinks, and an editor-oriented workflow.

The project is designed as **one product with multiple possible native implementations**. The current reference implementation is **C# / .NET 8 / Windows Forms** for Windows. Future implementations may be developed in **C++**, **Java**, **Swift**, or other languages while sharing the same Markdown interoperability rules and product specifications.

A future plugin system remains part of the long-term direction, but it is **not part of V1**.

## Concept screenshot

The current UI concept represents the .NET / WinForms reference implementation:

![SASD Notes WinForms concept screenshot](docs/images/sasd-notes-winforms-concept-dark.png)

## Product goals

- manage local Markdown files in a chosen vault / folder
- create and edit notes in an editor-oriented workflow
- support internal links such as `[[Project Plan]]`
- organize knowledge across nested topic folders
- switch between recently used topic folders / vaults
- provide search, backlinks, and document outline navigation
- keep user notes portable and usable outside SASD Notes
- keep implementation architecture readable, testable, and maintainable

## Planned V1 scope

The first V1 is the **.NET 8 / Windows Forms implementation**.

Planned capabilities include:

- open a vault / root folder
- show folders and `.md` files in a tree
- create, open, edit, and save notes
- basic Markdown formatting helpers
- wiki-link parsing and navigation
- backlinks
- simple search across file names, titles, and content
- recent folders / recent vaults
- document outline
- status information in the main window

## Multi-language repository layout

```text
SASD-Notes/
├─ AGENTS.md
├─ README.md
├─ CONTRIBUTING.md
├─ docs/                       product and implementation documentation
├─ spec/                       shared language-neutral behavior contracts
├─ src/
│  ├─ dotnet/                  current C# / .NET 8 / WinForms reference implementation
│  ├─ cpp/                     future C++ implementation
│  ├─ java/                    future Java implementation
│  └─ swift/                   future Swift implementation
└─ tests/
   └─ dotnet/                  current .NET tests
```

Additional language directories should be added only when an implementation is actually started.

The repository deliberately shares **behavior and file-format contracts**, not necessarily source code. Each language implementation may use an idiomatic architecture and native UI framework.

## Shared specifications

Cross-language behavior belongs under [`spec/`](spec/README.md).

The shared contract includes concepts such as:

- Markdown files as the source of truth
- vault path and folder semantics
- wiki-link syntax and resolution
- backlinks
- search semantics
- outline extraction
- interoperability and conformance fixtures

## Current implementation

The active implementation is:

- C#
- .NET 8
- Windows Forms
- Visual Studio 2022
- Windows

See [`src/dotnet/`](src/dotnet/README.md).

Codex and other coding agents should follow the repository-level [`AGENTS.md`](AGENTS.md) and, for the .NET subtree, [`src/dotnet/AGENTS.md`](src/dotnet/AGENTS.md).

## Documentation

- [Project overview](docs/00_Projektuebersicht.md)
- [Obsidian feature reference catalog](docs/01_Obsidian_Funktionskatalog_Referenz.md)
- [Lastenheft](docs/02_Lastenheft_SASD_Notes.md)
- [Pflichtenheft WinForms](docs/03_Pflichtenheft_SASD_Notes_WinForms.md)
- [Technical design WinForms](docs/04_Technisches_Design_WinForms.md)
- [Security baseline](docs/05_Security_Baseline.md)
- [Test handbook](docs/06_Testhandbuch.md)
- [User manual](docs/07_Benutzerhandbuch.md)
- [Developer handbook](docs/08_Entwicklerhandbuch.md)
- [Installation and operations handbook](docs/09_Installations_und_Betriebshandbuch.md)
- [MVP / V1 project plan](docs/10_Projektplan_MVP_V1.md)
- [Backlog V1 / V2](docs/11_Backlog_V1_V2.md)
- [Multi-language repository architecture](docs/12_Multi_Language_Repository_Struktur.md)

## Development status

The GitHub repository currently contains the planning baseline and the new multi-language layout. The .NET implementation remains the first development priority.

The purpose of the language split is **not** to slow down V1 by developing four applications at once. It creates a clean place for future ports while preserving one shared product definition.

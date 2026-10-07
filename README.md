# SASD Notes

SASD Notes is a local-first Windows desktop application for Markdown-based
knowledge bases. Notes remain ordinary `.md` files so that they stay portable
and usable with other Markdown editors.

The application is designed around plain Markdown files, folder-based topic
organization, wiki-style links, search, backlinks, document outlines, and an
editor-focused workflow.

![SASD Notes WinForms concept](docs/images/sasd-notes-winforms-concept-dark.png)

## Current implementation status

SASD Notes is one product with a shared, language-neutral behavior contract.
The current reference implementation is C# / .NET 8 / Windows Forms for
Windows. Future implementations may be developed in C++, Java, Swift, or
other languages while preserving Markdown interoperability and the shared
product specifications.

A future plugin system remains part of the long-term direction, but it is not
part of MVP or V1.

The repository contains a working V1 code-base structure with clean
architectural layers:

- `Sasd.Notes.Domain`
- `Sasd.Notes.Application`
- `Sasd.Notes.Infrastructure`
- `Sasd.Notes.App.WinForms`
- `Sasd.Notes.Tests`

The current .NET project files remain in the established `src/Sasd.Notes.*`
directories. The language-specific directories described below provide the
repository structure for future implementations and their documentation.

## Implemented V1 features

- Open a local vault folder
- Scan and load Markdown files recursively
- Show folders and notes in a TreeView
- Open and edit Markdown notes
- Save notes back to disk
- Create new notes in the selected folder
- Recent Folders menu
- Markdown editor toolbar
- Wiki-link parser (`[[Note]]`, `[[Folder/Note]]`, `[[Note|Alias]]`)
- Open selected wiki-links from the editor
- Search in note titles, paths and content
- Backlinks panel
- Outline panel from Markdown headings
- Info panel and status bar metrics
- Dirty-state handling with save prompt

## Build requirements

- Windows
- Visual Studio 2022
- .NET 8 SDK

## Build in Visual Studio

1. Open `Sasd.Notes.sln`
2. Restore and build the solution
3. Start `Sasd.Notes.App.WinForms`

## Build on the command line

```powershell
dotnet clean
dotnet restore
dotnet build Sasd.Notes.sln
dotnet test Sasd.Notes.sln
dotnet run --project .\tests\Sasd.Notes.Tests\Sasd.Notes.Tests.csproj
dotnet run --project .\src\Sasd.Notes.App.WinForms\Sasd.Notes.App.WinForms.csproj
```

The WinForms build and application run are Windows-specific. The repository
should be edited and built from one consistent Windows working copy; do not
alternate Git clients from PowerShell and WSL against the same working tree.

## Notes about the tests project

To keep the solution **package-free and easy to build**, `Sasd.Notes.Tests` is currently implemented as a small console-based verification harness instead of using external test packages. This keeps compilation simple and still provides executable checks for the core logic.

`dotnet test` verifies that the solution and test project build. The executable
test harness itself is run with the additional `dotnet run --project` command
shown above.

## Quick demo

A small `sample-vault/` is included so you can start the application and immediately open a realistic local knowledge base.

## Product goals

- Manage local Markdown files in a selected vault or root folder.
- Create, open, edit, and save notes in an editor-oriented workflow.
- Support internal links between notes.
- Organize knowledge across nested topic folders.
- Switch between recently used vaults or topic folders.
- Provide search, backlinks, and outline navigation.
- Keep user notes portable and usable outside SASD Notes.
- Keep the implementation readable, testable, and maintainable.

## Multi-language repository layout

```text
SASD-Notes/
├─ AGENTS.md
├─ README.md
├─ CONTRIBUTING.md
├─ docs/                       product and implementation documentation
├─ spec/                       shared language-neutral behavior contracts
├─ src/
│  ├─ dotnet/                  .NET implementation guidance and future home
│  ├─ cpp/                     future C++ implementation
│  ├─ java/                    future Java implementation
│  └─ swift/                   future Swift implementation
└─ tests/
   └─ dotnet/                  .NET test guidance and future test layout
```

Additional language directories should be added only when an implementation
is actually started. The repository shares behavior and file-format contracts,
not necessarily source code. Each implementation may use an idiomatic
architecture and native UI framework.

## Shared specifications

Cross-language behavior belongs under [`spec/`](spec/README.md). The shared
contract includes concepts such as:

- Markdown files as the source of truth.
- Vault path and folder semantics.
- Wiki-link syntax and resolution.
- Backlinks.
- Search semantics.
- Outline extraction.
- Interoperability and conformance fixtures.

## Documentation

- [Project overview](docs/00_Projektuebersicht.md)
- [Obsidian feature reference catalog](docs/01_Obsidian_Funktionskatalog_Referenz.md)
- [Requirements specification](docs/02_Lastenheft_SASD_Notes.md)
- [WinForms technical specification](docs/03_Pflichtenheft_SASD_Notes_WinForms.md)
- [Technical design](docs/04_Technisches_Design_WinForms.md)
- [Security baseline](docs/05_Security_Baseline.md)
- [Test handbook](docs/06_Testhandbuch.md)
- [User handbook](docs/07_Benutzerhandbuch.md)
- [Developer handbook](docs/08_Entwicklerhandbuch.md)
- [Installation and operations handbook](docs/09_Installations_und_Betriebshandbuch.md)
- [MVP / V1 project plan](docs/10_Projektplan_MVP_V1.md)
- [V1 / V2 backlog](docs/11_Backlog_V1_V2.md)
- [Multi-language repository structure](docs/12_Multi_Language_Repository_Struktur.md)

The repository-level [`AGENTS.md`](AGENTS.md) contains the general development
rules. The .NET-specific guidance is available in
[`src/dotnet/AGENTS.md`](src/dotnet/AGENTS.md).

## Development status

The .NET 8 WinForms reference implementation is the first development
priority. The multi-language layout prepares future ports without requiring
four applications to be developed in parallel or slowing down V1.

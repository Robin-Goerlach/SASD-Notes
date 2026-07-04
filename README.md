# SASD Notes

SASD Notes is a planned Windows desktop note-taking application for **local Markdown knowledge bases**.  
The project is designed around open text files, internal wiki-style links, folder-based organization, search, backlinks and an editor-oriented workflow.

The first implementation target is **Windows Forms on .NET 8**.  
A future plugin system is intentionally kept in mind, but it is **not part of V1**.

## Concept Screenshot

![SASD Notes WinForms concept screenshot](docs/images/sasd-notes-winforms-concept-dark.png)

## Project goals

- manage local Markdown files in a chosen vault/folder
- create and edit notes in an editor-like workflow
- support internal links such as `[[Project Plan]]`
- organize knowledge across multiple topic folders
- provide search, backlinks and outline-oriented navigation
- stay readable, testable and extensible

## Planned V1 scope

- open a vault / root folder
- show folders and `.md` files in a tree
- create, open, edit and save notes
- basic Markdown formatting helpers
- wiki-link parsing and navigation
- backlinks
- simple search across file names and content
- recent folders / recent vaults
- status information in the main window

## Repository structure

```text
docs/   documentation set
src/    planned source projects
tests/  planned automated tests
```

## Documentation

- [Project overview](docs/00_Projektuebersicht.md)
- [Obsidian feature reference catalog](docs/01_Obsidian_Funktionskatalog_Referenz.md)
- [Lastenheft](docs/02_Lastenheft_SASD_Notes.md)
- [Pflichtenheft WinForms](docs/03_Pflichtenheft_SASD_Notes_WinForms.md)
- [Technical design](docs/04_Technisches_Design_WinForms.md)
- [Security baseline](docs/05_Security_Baseline.md)
- [Test handbook](docs/06_Testhandbuch.md)
- [User manual](docs/07_Benutzerhandbuch.md)
- [Developer handbook](docs/08_Entwicklerhandbuch.md)
- [Installation and operations handbook](docs/09_Installations_und_Betriebshandbuch.md)
- [MVP / V1 project plan](docs/10_Projektplan_MVP_V1.md)
- [Backlog V1 / V2](docs/11_Backlog_V1_V2.md)

## Development note

This repository starter intentionally focuses on **documentation, structure and implementation guidance first**.  
It is meant to serve as a clean GitHub starting point before the real WinForms codebase is added.

# SASD Notes shared specifications

The `spec/` directory is the language-neutral contract between SASD Notes implementations.

It exists to prevent the C#, C++, Java, Swift, and future implementations from gradually developing incompatible behavior.

## What belongs here

Stable, testable behavior such as:

- supported Markdown and wiki-link syntax
- vault path rules
- link normalization and resolution
- backlink semantics
- note naming rules
- search semantics
- outline extraction rules
- interoperability expectations
- example vaults and conformance fixtures

## What does not belong here

Implementation details such as:

- WinForms controls
- CMake organization
- Java package names
- SwiftUI/AppKit choices
- dependency-injection frameworks
- language-specific persistence APIs

## Initial shared contract

Until dedicated specification files are added, all implementations must at minimum agree on these rules:

1. Notes remain normal UTF-8 Markdown files.
2. Folder hierarchy is meaningful and must be preserved.
3. Wiki links support `[[Note]]`, `[[Note|Alias]]`, and folder-qualified targets such as `[[Folder/Note]]`.
4. A link implementation must distinguish resolved and unresolved targets.
5. Backlinks are derived from links in other notes rather than stored as authoritative note data.
6. Indexes and caches are disposable derived data.
7. Implementations must not require proprietary metadata in order to read ordinary notes.

As cross-language development begins, split these rules into focused specification documents and derive equivalent test fixtures from them.

# AGENTS.md — SASD Notes

## Purpose

SASD Notes is a local-first Markdown knowledge and note-taking application for
Windows. User notes remain ordinary Markdown files and must remain usable in
other editors without SASD Notes.

This file defines repository-wide rules for autonomous coding agents such as
OpenAI Codex. Agents may work independently on routine implementation,
refactoring, testing, documentation, and build fixes when the task is clear
and these rules are respected.

## Technical baseline

The active reference implementation uses:

- C# and .NET 8
- Windows Forms with target framework `net8.0-windows`
- Visual Studio 2022
- SDK-style projects using `Microsoft.NET.Sdk`

Do not switch the existing implementation to .NET 9 or .NET 10 without an
explicit project decision.

The Windows implementation is built and tested from a consistent Windows
working copy. Do not alternate Git clients from PowerShell and WSL against the
same working tree.

## Repository model

The repository is prepared for one product with multiple possible native
implementations:

```text
src/
├─ dotnet/                  .NET guidance and future language-specific home
├─ cpp/                     future C++ implementation
├─ java/                    future Java implementation
└─ swift/                   future Swift implementation

tests/
├─ dotnet/                  .NET test guidance and future test layout
├─ cpp/
├─ java/
└─ swift/

spec/                       shared language-neutral behavior contracts
docs/                       product and implementation documentation
```

The current .NET project files remain in the established `src/Sasd.Notes.*`
directories in this build baseline. A future move into a language-specific
subtree must be performed as a deliberate, separately verified change.

Language-specific instructions belong in a nested `AGENTS.md`, for example
`src/dotnet/AGENTS.md`. A nested file may refine rules for its subtree but must
not contradict the product, safety, or portability rules in this root file.

## Shared product contract

All implementations should converge on the same user-visible semantics for:

- Markdown files as the source of truth
- vault and working-folder behavior
- nested topic folders
- wiki-links such as `[[Note]]`, `[[Note|Alias]]`, and `[[Folder/Note]]`
- link resolution and backlinks
- search behavior
- Markdown formatting helpers
- document outline extraction
- recent folders and recent vaults
- non-destructive file handling

When behavior is stable enough to share across languages, document it under
`spec/` instead of copying the rule independently into every implementation.

## Architecture boundaries

- `Sasd.Notes.Domain` contains domain models and rules and must not reference
  Windows Forms or other UI frameworks.
- `Sasd.Notes.Application` contains use cases and testable application logic.
- `Sasd.Notes.Infrastructure` contains filesystem and persistence adapters.
- `Sasd.Notes.App.WinForms` contains forms, controls, and thin UI handlers.
- `Sasd.Notes.Tests` contains deterministic core-logic verification.

Keep infrastructure behind interfaces where application logic needs it. Core
behavior must remain testable without starting the UI.

## Core principles

### Markdown files are authoritative

User notes are normal `.md` files. Do not make a database the authoritative
store for note contents. A later database may contain only derived data such
as caches, indexes, or metadata.

### Local-first

The application must remain useful offline. Do not add telemetry, cloud
accounts, remote APIs, or online dependencies unless explicitly requested.

### Preserve user data

Never silently delete or overwrite user notes. Validate paths, keep writes
inside the selected vault, prevent path traversal, handle access errors, and
avoid destructive migrations.

### Correctness before optimization

Prefer clear, conservative, and testable implementations. Optimize only after
behavior is correct and the relevant performance problem has been measured.

### Avoid premature platform unification

Different language implementations may use native UI frameworks and idiomatic
project layouts. Do not force identical source architecture across C++, C#,
Java, and Swift merely for visual symmetry. Shared behavior and on-disk
compatibility matter more than identical source code.

## Coding and documentation rules

- Analyze existing code before replacing it; improve the current architecture
  where practical.
- Prefer descriptive names and straightforward implementations over clever
  one-liners or unnecessary abstraction.
- Add XML documentation to public classes, interfaces, enums, enum members,
  methods, properties, and important constructors.
- Add meaningful `//` comments around non-trivial processing steps, explaining
  intent and safety decisions rather than restating syntax.
- Use `System.IO.Path` for path handling and validate every user-controlled path
  against the selected vault.
- Never delete notes or overwrite files outside the vault without explicit
  user intent.
- Add deterministic tests for new parser, link, search, path, and formatter
  behavior whenever practical.
- When a compiler error occurs, fix the first real error before addressing
  follow-on diagnostics.

## Documentation

Repository-wide product and architecture documents live in `docs/`.
Cross-language behavioral contracts live in `spec/`.
Implementation-specific design notes must clearly identify their implementation,
for example `.NET 8 / WinForms`, C++ desktop, Java desktop, or Swift/macOS.
Do not silently rewrite an implementation-specific document as though it
applied to every platform.

## Testing

Each implementation owns its language-specific automated tests under
`tests/<language>/` when that layout is active. Equivalent conformance cases
for multiple implementations should be derived from the shared contracts in
`spec/`.

The current .NET test project is intentionally package-free. `dotnet test`
verifies that the solution and test project build; the executable core-logic
harness must additionally be run with `dotnet run`.

## Autonomous work policy

Agents may, without additional approval:

- inspect repository files and documentation
- edit source, test, and documentation files
- add focused tests
- refactor within the requested scope
- run safe build and test commands
- run `git status`, `git diff`, `git log`, and `git show`
- create focused commits when the task permits them

Agents must not, without explicit authorization:

- force-push or rewrite shared history
- delete large groups of user files
- change the project license
- publish releases or packages
- add telemetry or cloud services
- perform destructive data migrations
- install system-wide software
- replace the selected runtime/toolchain for an existing implementation

## Git workflow

Prefer feature branches and small, reviewable commits. Before considering code
work complete, review the final diff and run the implementation-specific
verification commands.

Never claim that a build or test run succeeded unless it was actually executed.
Do not commit merge-conflict markers. After every merge, search for
`<<<<<<<`, `=======`, and `>>>>>>>` before pushing.

## Verification

From the repository root on Windows, run:

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project .\tests\Sasd.Notes.Tests\Sasd.Notes.Tests.csproj
```

The package-free test harness must be run explicitly with `dotnet run` because
`dotnet test` without a test SDK only verifies the test-project build.

## Architecture review checklist

Before finalizing a larger change, check:

- Is shared behavior being duplicated instead of documented in `spec/`?
- Is implementation-specific code leaking into repository-wide contracts?
- Are Markdown files still the authoritative portable data source?
- Are file operations non-destructive and path-safe?
- Can core behavior be tested without starting the UI?
- Are Domain, Application, Infrastructure, and UI boundaries preserved?
- Is the implementation more complicated than the current milestone requires?

## Current roadmap priority

The .NET implementation is the active reference implementation. The near-term
sequence is:

1. clean build baseline
2. vault and folder handling
3. editor and save workflow
4. Markdown formatting helpers
5. wiki-links and backlinks
6. search
7. usability and hardening

Future C++, Java, Swift, or other implementations should begin from shared
documented behavior and must not block progress of the .NET version.

## Definition of done

A task is complete when the requested behavior is implemented, relevant tests
and documentation are updated, architecture boundaries remain understandable,
the final diff has been reviewed, and the appropriate build/test commands have
succeeded. If a required command cannot be run, state that explicitly.

The guiding principle is:

> One product contract, multiple idiomatic implementations, portable Markdown data.

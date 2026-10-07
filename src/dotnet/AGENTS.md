# AGENTS.md — .NET implementation

These instructions apply to the C#/.NET subtree and complement the repository-wide `/AGENTS.md`.

## Technical baseline

- C#
- .NET 8
- Windows Forms
- Visual Studio 2022
- target framework: `net8.0-windows`

Do not upgrade this implementation to .NET 9 or .NET 10 unless explicitly requested.

For the WinForms project use the modern SDK form:

```xml
<Project Sdk="Microsoft.NET.Sdk">
```

with:

```xml
<TargetFramework>net8.0-windows</TargetFramework>
<UseWindowsForms>true</UseWindowsForms>
```

## Architecture

Expected projects:

```text
Sasd.Notes.Domain
Sasd.Notes.Application
Sasd.Notes.Infrastructure
Sasd.Notes.App.WinForms
```

### Domain

Contains pure domain models and concepts.

Must not depend on WinForms or concrete filesystem infrastructure.

### Application

Contains use cases and core services such as:

- WikiLinkParser
- LinkResolver
- BacklinkService
- SearchService
- EditorFormattingService

Do not manipulate `RichTextBox`, `TreeView`, `Form`, or other WinForms controls here.

### Infrastructure

Contains filesystem and other technical implementations.

Keep vault scanning, file persistence, recent-folder persistence, and file watching out of Forms.

### App.WinForms

Contains UI composition and UI event handling.

Keep event handlers thin and delegate real work to application services.

## Documentation style

Publicly visible classes, interfaces, enums, enum values, methods, and important properties should have useful XML documentation.

Use explanatory `//` comments inside methods for meaningful processing steps. Avoid comments that only repeat obvious syntax.

Do not knowingly introduce new `CS1591` warnings.

## Build loop

From the .NET solution root, use:

```powershell
dotnet clean
dotnet restore
dotnet build
dotnet test
```

During iterative work, after restore has already succeeded:

```powershell
dotnet build
dotnet test
```

Do not claim success unless commands were actually executed.

When compiler errors cascade, fix the first genuine root error and rebuild before treating later parser errors as independent defects.

## Tests

Prioritize tests for:

- wiki-link parsing
- link resolution
- backlinks
- search
- Markdown formatting
- path normalization
- vault scanning
- CRLF and LF handling
- Unicode
- invalid boundaries and missing files

## File safety

Use `System.IO.Path` APIs rather than manual path concatenation.

Normalize logical vault-relative link paths consistently, preferably using `/`.

Validate that user-derived paths remain inside the selected vault before creating or writing files.

## Current historical note

An earlier local build reported malformed string/character literal errors in `EditorFormattingService.cs`, XML documentation warnings on `MarkdownFormatKind`, and `NETSDK1137` for `Microsoft.NET.Sdk.WindowsDesktop`.

Always inspect and reproduce the current state; do not assume these historical issues still exist.

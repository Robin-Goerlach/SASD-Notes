# Source implementations

SASD Notes supports parallel implementations in multiple programming languages.

## Layout

```text
src/
├─ dotnet/   C# / .NET implementation
├─ cpp/      future C++ implementation
├─ java/     future Java implementation
└─ swift/    future Swift implementation
```

The directories are organized by **implementation language**, following the same approach already used in other SASD repositories such as SASD Math Toolkit and SASD Graphics Toolkit.

## Important rule

These implementations are not expected to share source code mechanically.

They are expected to share:

- the SASD Notes product requirements
- Markdown file compatibility
- wiki-link semantics
- vault behavior
- search and backlink semantics where specified
- shared conformance expectations under `/spec`

Each implementation may use the architecture and UI framework that is idiomatic for its target platform.

The current reference implementation is **C# / .NET 8 / Windows Forms** under `src/dotnet/`.

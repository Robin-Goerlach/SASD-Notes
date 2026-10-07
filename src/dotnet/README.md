# SASD Notes — .NET implementation

This directory contains the current reference implementation of SASD Notes.

## Baseline

- C#
- .NET 8
- Windows Forms
- Visual Studio 2022
- Windows target

## Planned solution layout

```text
src/dotnet/
├─ Sasd.Notes.sln
├─ Sasd.Notes.Domain/
├─ Sasd.Notes.Application/
├─ Sasd.Notes.Infrastructure/
└─ Sasd.Notes.App.WinForms/
```

The corresponding tests belong under:

```text
tests/dotnet/
└─ Sasd.Notes.Tests/
```

The solution should preserve the existing layered architecture:

`Domain -> Application abstractions -> Infrastructure / WinForms composition`

The WinForms application is the first implementation and remains the development priority for V1.

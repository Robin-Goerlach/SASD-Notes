# Contributing

SASD Notes is organized as one product with multiple possible language-specific implementations.

## Working style

- keep Markdown files as the primary source of truth
- prefer readable code over clever code
- protect user data and avoid destructive file operations
- keep implementation-specific details inside the relevant language subtree
- document shared product behavior under `spec/`
- add tests for parser, search, link resolution, formatting, and file handling logic
- do not introduce cross-language abstractions merely for symmetry

## Repository areas

- `docs/` — product, architecture, user, test, and implementation documentation
- `spec/` — language-neutral behavior and interoperability contracts
- `src/<language>/` — source code for a specific implementation
- `tests/<language>/` — tests for that implementation

The current development priority is `src/dotnet/`.

## Documentation first for shared behavior

Before changing behavior that should be compatible across implementations, update or add the relevant contract in `spec/`.

Implementation-only details belong in that implementation's documentation and should not be presented as universal requirements.

## Language-specific guidance

Read the nearest `AGENTS.md` before editing code.

The repository root `AGENTS.md` contains shared rules. Nested files may define additional build, style, and architecture requirements.

## Commit style

Examples:

- `feat(dotnet-vault): add root folder loading`
- `feat(dotnet-editor): add markdown formatting helpers`
- `fix(dotnet-search): improve result snippet generation`
- `spec: define wiki-link resolution rules`
- `docs: update repository architecture`

Keep commits focused and reviewable.

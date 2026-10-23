# Vault contract: creating a Markdown note

Status: implemented in the .NET 8 reference implementation.
Scope: note creation inside an already selected vault; existing-note saving is a separate operation.

## Naming and publication

- A requested title is converted into a safe file name and receives a Markdown extension.
- Existing files and directories reserve their names. The implementation selects a numbered suffix, starting with ` 2`, if necessary.
- Checking that a name is free does not reserve it.
- Publishing a new note must use an exclusive operation that cannot replace an existing target.
- If another creator takes the candidate first, the implementation may retry with another available name. The .NET implementation allows up to 32 retries and then reports the file error.
- A successful result identifies the file actually created; title, path and initial heading refer to that final name.
- Existing contents must remain unchanged, including during concurrent note creation.
- Candidate naming is not required to be gap-free or predictable under concurrency.

## Cancellation and errors

- Cancellation observed before creation starts must not create the target folder or note.
- Cancellation is checked again before publication. Temporary files are removed when writing, publication or cancellation fails.
- Cancellation arriving after the exclusive publication has completed does not roll back a successfully created note.
- Non-collision errors remain visible to the caller.

## Limits

This contract covers exclusive creation. It does not claim to serialize all external editors, make existing-note saving conflict-free against every concurrent writer, or resolve symlink/reparse-point paths.

## Reference verification

The package-free .NET harness verifies concurrent creators, preserved existing contents, distinct result paths, directory-name collisions, cancellation and temporary-file cleanup.

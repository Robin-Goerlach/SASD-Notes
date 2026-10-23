# Vault contract: switching the active workspace

Scope: opening a vault, including refreshing the current vault.

## Prepare before replacing

- The current vault, loaded notes, note identities, dirty contents and folder tree remain active while the candidate is prepared.
- All candidate notes and folder paths must be loaded successfully before any active state is replaced.
- Recent-folder changes are prepared in an independent settings candidate, including independent recent-folder entries.
- The .NET implementation saves this candidate before adopting the new workspace.
- Read errors, settings-save errors and cancellation observed before adoption leave the active in-memory workspace and its recent-folder list unchanged.
- An unsuccessful first open leaves the workspace unopened and without loaded notes.
- A successful switch adopts the candidate notes, folders, vault and settings together, without an intervening asynchronous operation.
- After the settings repository reports successful persistence, adoption completes without an additional cancellation checkpoint.

## Responsibility boundaries

The UI obtains any required confirmation to save or discard local edits before requesting a switch. The Application layer preserves the old state if the requested switch fails; it does not overwrite note files as part of opening a vault.

Infrastructure remains responsible for reading notes and folders and persisting settings. The workspace does not claim to roll back a partially completed settings write from a failing third-party adapter. The standard JSON adapter writes through a temporary file.

## Exclusive transition

- At most one vault-switch request may be active at a time. Further requests are rejected without starting another scan.
- A switch cannot start while note saving, note creation or folder creation is still in progress.
- During a switch, new write operations, working-content updates, discarding and note reloads are rejected.
- A note reload that started earlier cannot adopt its result while a switch is in progress.
- Read-only views can continue to use the previous workspace until adoption.
- The transition remains protected while settings are being saved, not just during the note scan.
- Protection is released after success, failure or cancellation, so a later attempt remains possible.

The WinForms UI obtains save/discard confirmation first and disables interaction
while loading the vault. Closing is deferred by cancelling the close attempt
during that transition; the user may close normally after loading completes.
A duplicate opening request during confirmation or loading does not start another UI transition.

The .NET workspace is used on a single execution context (the WinForms UI context).
These guards coordinate overlapping asynchronous commands on that context;
they are not general synchronization for calls from multiple threads.
Direct mutation of publicly mutable note models bypasses workspace operations
and is outside this coordination contract.

## Verification

The package-free .NET harness checks failed initial opening, note-read failure, folder-read failure after note loading, settings-save failure, cancellation after scanning, preservation of dirty content and successful switching after errors. Additional controlled-delay tests cover overlapping switches, writes, settings persistence and cancellation. The Windows harness checks interaction and close protection during real form loading.

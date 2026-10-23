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

Concurrent vault-switch requests are not serialized by this contract; coordinating multiple simultaneous UI operations remains a separate concern.

## Verification

The package-free .NET harness checks failed initial opening, note-read failure, folder-read failure after note loading, settings-save failure, cancellation after scanning, preservation of dirty content and successful switching after errors.

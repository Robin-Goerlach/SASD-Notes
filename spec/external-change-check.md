# Periodic external-change checks

The Windows reference implementation checks the currently open note periodically.
It does not reload, save, discard, or edit a note as part of this check.

The application layer reports four distinct outcomes:

- `Unchanged`: the repository reported no external change.
- `Changed`: the repository reported a changed or missing file.
- `Unavailable`: the repository raised an I/O or unauthorized-access error.
- `NotChecked`: a vault transition or save is in progress, or the note model
  no longer belongs to the workspace.

A failed check must never be interpreted as successful confirmation that the
file is unchanged. Expected access errors must not escape the UI timer handler.
Unexpected programming errors remain visible for diagnosis.

The UI reports changed and unavailable states once per file and state. It retries
on subsequent ticks, reports recovery from an unavailable state, and leaves the
editor text, dirty flag, loaded timestamp and discard baseline untouched.
A busy or obsolete model causes no repository check. Unrelated notes are not
polled by this UI timer.

The existing repository timestamp comparison remains unchanged. This is not a
filesystem watcher, a content-hash comparison, or a guarantee against concurrent
writers. The repository can report missing or inaccessible paths as changed
when its underlying existence check cannot distinguish those cases. The explicit
unavailable outcome covers access exceptions actually reported by the adapter.
Save conflict detection and explicit reload remain separate operations.

Verification includes controlled repository failures and recovery, busy/foreign
models, preservation of local drafts, and the real Windows timer with a visible
form and message pump.

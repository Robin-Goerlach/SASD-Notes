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

The filesystem adapter records both the last-write timestamp and a SHA-256
fingerprint of the decoded Markdown text after load, creation, and successful
save. A timestamp difference reports a change immediately. When the timestamp
is unchanged, the current decoded text is fingerprinted and compared as well.
This detects same-length edits and changes whose timestamp was preserved or
reset. A model without a fingerprint retains timestamp-only compatibility.

The fingerprint is derived metadata held in memory; it is never written into the
Markdown file and is not a security signature. A byte-order-mark-only change is
ignored when the decoded text remains identical.

The comparison does not provide a writer lock or a transaction across processes.
A concurrent change after the final pre-save check can still race with the atomic
replacement. The repository can report missing or inaccessible paths as changed
when its underlying existence check cannot distinguish those cases. The explicit
unavailable outcome covers access exceptions actually reported by the adapter.
Save conflict detection and explicit reload remain separate operations.

Verification includes controlled repository failures and recovery, busy or
foreign models, preserved local drafts, same-length changes with equal timestamps,
fingerprint propagation, and the real Windows timer with a visible form and
message pump.

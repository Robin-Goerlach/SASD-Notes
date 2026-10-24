# Local settings persistence

Settings use JSON in the local user profile. Missing files and malformed JSON
yield defaults. Explicit null settings or a null recent-folder list also yield
safe defaults. Null recent entries and blank paths are ignored; otherwise valid
entries and the last-opened path remain unchanged. Missing vault directories
are not removed simply because they are currently unavailable.

Reading never rewrites the source file or creates directories. I/O and access
errors are propagated to the existing application error handling.

Saving writes a unique temporary file before replacing the settings file.
Cancellation is checked before directory creation and again before replacement.
Cancellation is cooperative; a request arriving after the final check can race
with replacement. This is not a multi-process transaction or writer lock.

The infrastructure repository accepts an absolute settings-file path for isolated
tests. The default constructor continues to select the local user profile.
User Markdown files are unaffected.

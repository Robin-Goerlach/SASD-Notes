# Recent folders

The application keeps at most ten recently opened vault paths. Paths are stored
as fully qualified, syntactically normalized paths. Optional trailing directory
separators and dot segments do not create separate entries for the same vault.
Comparison is case-insensitive for the Windows reference implementation.

Opening a vault moves its normalized path to the first position. Older duplicate
entries are removed and the most recent timestamp wins when legacy settings
already contain duplicates. The active settings are replaced only after the new
vault and the prepared settings have both been persisted successfully.

Persisted relative, blank, malformed, or unsupported paths are ignored. A valid
absolute path remains in the history even when its directory is temporarily
unavailable. Merely displaying recent folders therefore performs no filesystem
probe and does not silently delete history.

Filesystem and drive roots retain their required trailing separator. If a root
has no final path component, its complete normalized path is used as the display
name. Returned recent-folder entries are independent snapshots so callers cannot
mutate the loaded settings indirectly.

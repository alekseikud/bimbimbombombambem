# Submission Notes

## Library File Format

The application saves libraries as indented JSON files using the `.musica.json` extension. JSON was chosen because the stored data is structured but simple: song metadata, optional cover/audio bytes, and playlist song references by `Guid`. It is easy to inspect during grading and does not require external packages.

## Implemented Home Scope

Implemented: MVVM infrastructure, library creation/opening/saving with automatic persistence, startup navigation, song list view, playlist view, playlist create/rename/delete, adding/removing/reordering songs in playlists, song import with file data storage and basic metadata fallback/ID3v1 reading, song edit window, real-time search, and a basic now-playing bar with a `MediaPlayer` service.

Not fully implemented: rich audio tag parsing for all file formats and album-cover extraction from embedded tags. The import remains self-contained and does not depend on NuGet packages.

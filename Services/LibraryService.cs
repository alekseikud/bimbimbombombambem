using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WpfApp3.Models;

namespace WpfApp3.Services;

public class LibraryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string? CurrentPath { get; private set; }

    public MusicLibrary CreateNew(string path)
    {
        CurrentPath = path;
        var library = new MusicLibrary();
        Save(library);
        return library;
    }

    public MusicLibrary Load(string path)
    {
        CurrentPath = path;
        if (!File.Exists(path))
        {
            return new MusicLibrary();
        }

        var dto = JsonSerializer.Deserialize<LibraryDto>(File.ReadAllText(path), JsonOptions) ?? new LibraryDto();
        var library = new MusicLibrary();
        var songsById = new Dictionary<Guid, Song>();

        foreach (var songDto in dto.Songs)
        {
            var song = songDto.ToSong();
            library.Songs.Add(song);
            songsById[song.Id] = song;
        }

        foreach (var playlistDto in dto.Playlists)
        {
            var playlist = playlistDto.ToPlaylist();
            foreach (var songId in playlistDto.SongIds)
            {
                if (songsById.TryGetValue(songId, out var song))
                {
                    playlist.Songs.Add(song);
                }
            }

            playlist.RefreshSongCount();
            library.Playlists.Add(playlist);
        }

        return library;
    }

    public void Save(MusicLibrary library)
    {
        if (string.IsNullOrWhiteSpace(CurrentPath))
        {
            return;
        }

        var dto = LibraryDto.FromLibrary(library);
        File.WriteAllText(CurrentPath, JsonSerializer.Serialize(dto, JsonOptions));
    }

    private class LibraryDto
    {
        public List<SongDto> Songs { get; set; } = new();
        public List<PlaylistDto> Playlists { get; set; } = new();

        public static LibraryDto FromLibrary(MusicLibrary library)
        {
            return new LibraryDto
            {
                Songs = library.Songs.Select(SongDto.FromSong).ToList(),
                Playlists = library.Playlists.Select(PlaylistDto.FromPlaylist).ToList()
            };
        }
    }

    private class SongDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Genre { get; set; } = "";
        public int Year { get; set; }
        public int DurationSeconds { get; set; }
        public byte[]? CoverArt { get; set; }
        public byte[]? AudioData { get; set; }
        public string FileExtension { get; set; } = ".mp3";

        public Song ToSong()
        {
            return new Song
            {
                Id = Id,
                Title = Title,
                Artist = Artist,
                Album = Album,
                Genre = Genre,
                Year = Year,
                DurationSeconds = DurationSeconds,
                CoverArt = CoverArt,
                AudioData = AudioData,
                FileExtension = FileExtension
            };
        }

        public static SongDto FromSong(Song song)
        {
            return new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Genre = song.Genre,
                Year = song.Year,
                DurationSeconds = song.DurationSeconds,
                CoverArt = song.CoverArt,
                AudioData = song.AudioData,
                FileExtension = song.FileExtension
            };
        }
    }

    private class PlaylistDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public byte[]? CoverArt { get; set; }
        public List<Guid> SongIds { get; set; } = new();

        public Playlist ToPlaylist()
        {
            return new Playlist
            {
                Id = Id,
                Name = Name,
                CoverArt = CoverArt
            };
        }

        public static PlaylistDto FromPlaylist(Playlist playlist)
        {
            return new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                CoverArt = playlist.CoverArt,
                SongIds = playlist.Songs.Select(song => song.Id).ToList()
            };
        }
    }
}

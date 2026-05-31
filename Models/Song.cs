using System;
using System.IO;
using System.Text;
using WpfApp3.Infrastructure;

namespace WpfApp3.Models;

public class Song : BaseViewModel
{
    private string _title = "";
    private string _artist = "";
    private string _album = "";
    private string _genre = "";
    private int _year;
    private int _durationSeconds;
    private byte[]? _coverArt;

    public Guid Id { get; set; } = Guid.NewGuid();
    public byte[]? AudioData { get; set; }
    public string FileExtension { get; set; } = ".mp3";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    public string Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    public string Genre
    {
        get => _genre;
        set => SetProperty(ref _genre, value);
    }

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    public int DurationSeconds
    {
        get => _durationSeconds;
        set
        {
            if (SetProperty(ref _durationSeconds, value))
            {
                OnPropertyChanged(nameof(DurationDisplay));
            }
        }
    }

    public byte[]? CoverArt
    {
        get => _coverArt;
        set => SetProperty(ref _coverArt, value);
    }

    public string DurationDisplay
    {
        get
        {
            var duration = TimeSpan.FromSeconds(DurationSeconds);
            return $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";
        }
    }

    public static Song CreateMock(int index, string title, string artist, string album, int year, int durationSeconds)
    {
        return new Song
        {
            Title = title,
            Artist = artist,
            Album = album,
            Genre = "Indie",
            Year = year,
            DurationSeconds = durationSeconds,
            FileExtension = ".mp3"
        };
    }

    public static Song FromAudioFile(string path)
    {
        var song = new Song
        {
            Title = Path.GetFileNameWithoutExtension(path),
            Artist = "",
            Album = "",
            Genre = "",
            Year = 0,
            DurationSeconds = 0,
            AudioData = File.ReadAllBytes(path),
            FileExtension = Path.GetExtension(path)
        };

        ApplyId3v1Metadata(path, song);
        return song;
    }

    private static void ApplyId3v1Metadata(string path, Song song)
    {
        if (!string.Equals(Path.GetExtension(path), ".mp3", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileInfo = new FileInfo(path);
        if (fileInfo.Length < 128)
        {
            return;
        }

        using var stream = File.OpenRead(path);
        stream.Position = stream.Length - 128;
        var tag = new byte[128];
        _ = stream.Read(tag, 0, tag.Length);
        if (Encoding.ASCII.GetString(tag, 0, 3) != "TAG")
        {
            return;
        }

        var title = ReadId3Text(tag, 3, 30);
        var artist = ReadId3Text(tag, 33, 30);
        var album = ReadId3Text(tag, 63, 30);
        var yearText = ReadId3Text(tag, 93, 4);

        if (!string.IsNullOrWhiteSpace(title))
        {
            song.Title = title;
        }

        song.Artist = artist;
        song.Album = album;
        if (int.TryParse(yearText, out var year))
        {
            song.Year = year;
        }
    }

    private static string ReadId3Text(byte[] tag, int start, int count)
    {
        return Encoding.Latin1.GetString(tag, start, count).TrimEnd('\0', ' ');
    }
}

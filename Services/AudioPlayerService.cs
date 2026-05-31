using System;
using System.IO;
using System.Windows.Media;
using WpfApp3.Models;

namespace WpfApp3.Services;

public class AudioPlayerService
{
    private readonly MediaPlayer _mediaPlayer = new();
    private string? _tempFilePath;

    public event EventHandler? MediaEnded;
    public event EventHandler? MediaOpened;

    public AudioPlayerService()
    {
        _mediaPlayer.MediaEnded += (_, _) => MediaEnded?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.MediaOpened += (_, _) => MediaOpened?.Invoke(this, EventArgs.Empty);
    }

    public TimeSpan Position
    {
        get => _mediaPlayer.Position;
        set => _mediaPlayer.Position = value;
    }

    public TimeSpan Duration
    {
        get
        {
            return _mediaPlayer.NaturalDuration.HasTimeSpan
                ? _mediaPlayer.NaturalDuration.TimeSpan
                : TimeSpan.Zero;
        }
    }

    public double Volume
    {
        get => _mediaPlayer.Volume;
        set => _mediaPlayer.Volume = value;
    }

    public void Open(Song song)
    {
        if (song.AudioData is null || song.AudioData.Length == 0)
        {
            return;
        }

        CleanupTempFile();
        var extension = string.IsNullOrWhiteSpace(song.FileExtension) ? ".mp3" : song.FileExtension;
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"musica_{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(_tempFilePath, song.AudioData);
        _mediaPlayer.Open(new Uri(_tempFilePath));
    }

    public void Play()
    {
        _mediaPlayer.Play();
    }

    public void Pause()
    {
        _mediaPlayer.Pause();
    }

    private void CleanupTempFile()
    {
        if (_tempFilePath is null || !File.Exists(_tempFilePath))
        {
            return;
        }

        try
        {
            File.Delete(_tempFilePath);
        }
        catch (IOException)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp3.Infrastructure;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3.ViewModels;

public class PlayerViewModel : BaseViewModel
{
    private readonly AudioPlayerService _audioPlayerService;
    private readonly DispatcherTimer _timer;
    private readonly List<Song> _queue = new();
    private int _queueIndex = -1;
    private Song? _currentSong;
    private bool _isPlaying;
    private TimeSpan _position;
    private TimeSpan _duration;
    private double _volume = 0.7;

    public PlayerViewModel(AudioPlayerService audioPlayerService)
    {
        _audioPlayerService = audioPlayerService;
        _audioPlayerService.Volume = Volume;
        _audioPlayerService.MediaOpened += (_, _) =>
        {
            Duration = _audioPlayerService.Duration;
            if (CurrentSong is not null && CurrentSong.DurationSeconds == 0 && Duration > TimeSpan.Zero)
            {
                CurrentSong.DurationSeconds = (int)Duration.TotalSeconds;
            }
        };
        _audioPlayerService.MediaEnded += (_, _) => Next();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) =>
        {
            Position = _audioPlayerService.Position;
            Duration = _audioPlayerService.Duration;
        };
        _timer.Start();

        PlayPauseCommand = new RelayCommand(PlayPause, () => CurrentSong is not null);
        PreviousCommand = new RelayCommand(Previous, () => _queueIndex > 0);
        NextCommand = new RelayCommand(Next, () => _queueIndex >= 0 && _queueIndex < _queue.Count - 1);
        SeekCommand = new RelayCommand(parameter =>
        {
            if (parameter is double seconds)
            {
                Seek(TimeSpan.FromSeconds(seconds));
            }
        });
    }

    public ICommand PlayPauseCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand SeekCommand { get; }

    public Song? CurrentSong
    {
        get => _currentSong;
        private set => SetProperty(ref _currentSong, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            if (SetProperty(ref _isPlaying, value))
            {
                OnPropertyChanged(nameof(PlayPauseText));
            }
        }
    }

    public string PlayPauseText => IsPlaying ? "Pause" : "Play";

    public TimeSpan Position
    {
        get => _position;
        set
        {
            if (SetProperty(ref _position, value))
            {
                OnPropertyChanged(nameof(PositionSeconds));
                OnPropertyChanged(nameof(PositionDisplay));
            }
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(DurationSeconds));
                OnPropertyChanged(nameof(DurationDisplay));
            }
        }
    }

    public double PositionSeconds
    {
        get => Position.TotalSeconds;
        set => Seek(TimeSpan.FromSeconds(value));
    }

    public double DurationSeconds => Math.Max(1, Duration.TotalSeconds);

    public double Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value))
            {
                _audioPlayerService.Volume = value;
            }
        }
    }

    public string PositionDisplay => FormatTime(Position);
    public string DurationDisplay => FormatTime(Duration);

    public void PlaySingle(Song song)
    {
        PlayQueue(new[] { song }, 0);
    }

    public void PlayQueue(IEnumerable<Song> songs, int startIndex = 0)
    {
        _queue.Clear();
        _queue.AddRange(songs.Where(song => song.AudioData is not null && song.AudioData.Length > 0));
        if (_queue.Count == 0)
        {
            return;
        }

        _queueIndex = Math.Clamp(startIndex, 0, _queue.Count - 1);
        LoadCurrent();
    }

    private void PlayPause()
    {
        if (CurrentSong is null)
        {
            return;
        }

        if (IsPlaying)
        {
            _audioPlayerService.Pause();
            IsPlaying = false;
            return;
        }

        _audioPlayerService.Play();
        IsPlaying = true;
    }

    private void Previous()
    {
        if (_queueIndex <= 0)
        {
            return;
        }

        _queueIndex--;
        LoadCurrent();
    }

    private void Next()
    {
        if (_queueIndex < 0 || _queueIndex >= _queue.Count - 1)
        {
            IsPlaying = false;
            return;
        }

        _queueIndex++;
        LoadCurrent();
    }

    private void Seek(TimeSpan position)
    {
        _audioPlayerService.Position = position;
        Position = position;
    }

    private void LoadCurrent()
    {
        CurrentSong = _queue[_queueIndex];
        Position = TimeSpan.Zero;
        _audioPlayerService.Open(CurrentSong);
        _audioPlayerService.Play();
        IsPlaying = true;
        CommandManager.InvalidateRequerySuggested();
    }

    private static string FormatTime(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes}:{time.Seconds:00}";
    }
}

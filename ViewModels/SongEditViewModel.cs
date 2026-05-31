using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp3.Infrastructure;
using WpfApp3.Models;

namespace WpfApp3.ViewModels;

public class SongEditViewModel : BaseViewModel
{
    private string _title;
    private string _artist;
    private string _album;
    private string _genre;
    private int _year;
    private byte[]? _coverArt;

    public SongEditViewModel(Song song)
    {
        SourceSong = song;
        _title = song.Title;
        _artist = song.Artist;
        _album = song.Album;
        _genre = song.Genre;
        _year = song.Year;
        _coverArt = song.CoverArt;

        BrowseCoverCommand = new RelayCommand(BrowseCover);
        SaveCommand = new RelayCommand(Save, () => !string.IsNullOrWhiteSpace(Title));
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(this, false));
    }

    public event EventHandler<bool>? CloseRequested;

    public Song SourceSong { get; }
    public ICommand BrowseCoverCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
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

    public byte[]? CoverArt
    {
        get => _coverArt;
        set
        {
            if (SetProperty(ref _coverArt, value))
            {
                OnPropertyChanged(nameof(HasCover));
            }
        }
    }

    public bool HasCover => CoverArt is not null;

    private void BrowseCover()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.png;*.jpg;*.jpeg|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            CoverArt = File.ReadAllBytes(dialog.FileName);
        }
    }

    private void Save()
    {
        SourceSong.Title = Title.Trim();
        SourceSong.Artist = Artist.Trim();
        SourceSong.Album = Album.Trim();
        SourceSong.Genre = Genre.Trim();
        SourceSong.Year = Year;
        SourceSong.CoverArt = CoverArt;
        CloseRequested?.Invoke(this, true);
    }
}

using System;
using System.Collections.ObjectModel;
using WpfApp3.Infrastructure;

namespace WpfApp3.Models;

public class Playlist : BaseViewModel
{
    private string _name = "";
    private byte[]? _coverArt;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public byte[]? CoverArt
    {
        get => _coverArt;
        set => SetProperty(ref _coverArt, value);
    }

    public ObservableCollection<Song> Songs { get; } = new();

    public int SongCount => Songs.Count;

    public void RefreshSongCount()
    {
        OnPropertyChanged(nameof(SongCount));
    }
}

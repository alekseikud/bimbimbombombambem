using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WpfApp3.Infrastructure;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly LibraryService _libraryService = new();
    private MusicLibrary _library = new();
    private bool _isLibraryOpen;

    public MainViewModel()
    {
        NewLibraryCommand = new RelayCommand(NewLibrary);
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        SaveLibraryCommand = new RelayCommand(SaveLibrary, () => IsLibraryOpen);
        CreatePlaylistCommand = new RelayCommand(CreatePlaylist, () => IsLibraryOpen);
        Welcome = new WelcomeViewModel(NewLibraryCommand, OpenLibraryCommand);
    }

    public WelcomeViewModel Welcome { get; }
    public ICommand NewLibraryCommand { get; }
    public ICommand OpenLibraryCommand { get; }
    public ICommand SaveLibraryCommand { get; }
    public ICommand CreatePlaylistCommand { get; }

    public ObservableCollection<Song> Songs => _library.Songs;
    public ObservableCollection<Playlist> Playlists => _library.Playlists;

    public bool IsLibraryOpen
    {
        get => _isLibraryOpen;
        private set
        {
            if (SetProperty(ref _isLibraryOpen, value))
            {
                OnPropertyChanged(nameof(WelcomeVisibility));
                OnPropertyChanged(nameof(LibraryVisibility));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public Visibility WelcomeVisibility => IsLibraryOpen ? Visibility.Collapsed : Visibility.Visible;
    public Visibility LibraryVisibility => IsLibraryOpen ? Visibility.Visible : Visibility.Collapsed;

    private void NewLibrary()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Musica library (*.musica.json)|*.musica.json|JSON files (*.json)|*.json|All files|*.*",
            FileName = "MyLibrary.musica.json"
        };

        if (dialog.ShowDialog() == true)
        {
            var library = _libraryService.CreateNew(dialog.FileName);
            SeedLabSongs(library);
            SetLibrary(library);
            SaveLibrary();
        }
    }

    private void OpenLibrary()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Musica library (*.musica.json;*.json)|*.musica.json;*.json|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            SetLibrary(_libraryService.Open(dialog.FileName));
        }
    }

    private void CreatePlaylist()
    {
        var window = new PlaylistEditWindow { Owner = Application.Current.MainWindow };
        if (window.ShowDialog() == true)
        {
            var playlist = new Playlist
            {
                Name = window.PlaylistName,
                CoverPath = window.CoverPath
            };

            AttachPlaylist(playlist);
            Playlists.Add(playlist);
            SaveLibrary();
        }
    }

    private void SetLibrary(MusicLibrary library)
    {
        DetachLibrary();
        _library = library;
        AttachLibrary();
        IsLibraryOpen = true;
        OnPropertyChanged(nameof(Songs));
        OnPropertyChanged(nameof(Playlists));
    }

    private void AttachLibrary()
    {
        Songs.CollectionChanged += SongsChanged;
        Playlists.CollectionChanged += PlaylistsChanged;

        foreach (var song in Songs)
        {
            AttachSong(song);
        }

        foreach (var playlist in Playlists)
        {
            AttachPlaylist(playlist);
        }
    }

    private void DetachLibrary()
    {
        Songs.CollectionChanged -= SongsChanged;
        Playlists.CollectionChanged -= PlaylistsChanged;

        foreach (var song in Songs)
        {
            song.PropertyChanged -= ItemChanged;
        }

        foreach (var playlist in Playlists)
        {
            playlist.PropertyChanged -= ItemChanged;
        }
    }

    private void SongsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (Song song in e.NewItems)
            {
                AttachSong(song);
            }
        }

        SaveLibrary();
    }

    private void PlaylistsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (Playlist playlist in e.NewItems)
            {
                AttachPlaylist(playlist);
            }
        }

        SaveLibrary();
    }

    private void AttachSong(Song song)
    {
        song.PropertyChanged += ItemChanged;
    }

    private void AttachPlaylist(Playlist playlist)
    {
        playlist.PropertyChanged += ItemChanged;
    }

    private void ItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveLibrary();
    }

    private void SaveLibrary()
    {
        _libraryService.Save(_library);
    }

    private static void SeedLabSongs(MusicLibrary library)
    {
        if (library.Songs.Count > 0)
        {
            return;
        }

        library.Songs.Add(new Song
        {
            Id = 1,
            Title = "Northern Lights",
            Artist = "Ava Stone",
            Album = "Night Walks",
            Year = 2021,
            Duration = "3:47"
        });
        library.Songs.Add(new Song
        {
            Id = 2,
            Title = "Red Vinyl",
            Artist = "The Glass Birds",
            Album = "City Echoes",
            Year = 2020,
            Duration = "3:14"
        });
        library.Songs.Add(new Song
        {
            Id = 3,
            Title = "Morning Static",
            Artist = "Leon Hale",
            Album = "Signals",
            Year = 2022,
            Duration = "4:11"
        });
        library.Songs.Add(new Song
        {
            Id = 4,
            Title = "Soft Exit",
            Artist = "Mira Vale",
            Album = "Small Rooms",
            Year = 2019,
            Duration = "3:28"
        });
        library.Songs.Add(new Song
        {
            Id = 5,
            Title = "Blue Arcade",
            Artist = "Nova Lane",
            Album = "Pixel Heart",
            Year = 2023,
            Duration = "3:56"
        });
    }
}

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp3.Infrastructure;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly LibraryService _libraryService = new();
    private MusicLibrary _library = new();
    private object? _currentPage;
    private Playlist? _selectedPlaylist;
    private SongListViewModel? _songListViewModel;
    private PlaylistViewModel? _playlistViewModel;

    public MainViewModel()
    {
        Player = new PlayerViewModel(new AudioPlayerService());

        NewLibraryCommand = new RelayCommand(NewLibrary);
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        SaveLibraryCommand = new RelayCommand(SaveLibrary, () => HasOpenLibrary);
        ShowAllSongsCommand = new RelayCommand(ShowAllSongs, () => HasOpenLibrary);
        CreatePlaylistCommand = new RelayCommand(CreatePlaylist, () => HasOpenLibrary);
        RenameSelectedPlaylistCommand = new RelayCommand(RenameSelectedPlaylist, () => SelectedPlaylist is not null);
        DeleteSelectedPlaylistCommand = new RelayCommand(DeleteSelectedPlaylist, () => SelectedPlaylist is not null);
        ImportSongsCommand = new RelayCommand(ImportSongs, () => HasOpenLibrary);
        EditSelectedSongCommand = new RelayCommand(EditSelectedSong, () => CurrentSelectedSong is not null);
        AddSelectedSongToPlaylistCommand = new RelayCommand(AddSelectedSongToPlaylist, () =>
            _songListViewModel?.SelectedSong is not null && _songListViewModel.TargetPlaylist is not null);
        PlaySelectedSongCommand = new RelayCommand(PlaySelectedSong, () => CurrentSelectedSong is not null);
        PlayPlaylistCommand = new RelayCommand(PlayPlaylist, () => SelectedPlaylist?.Songs.Count > 0);
        RemoveSelectedSongCommand = new RelayCommand(RemoveSelectedSongFromPlaylist, () => _playlistViewModel?.SelectedSong is not null);
        MoveSelectedSongUpCommand = new RelayCommand(MoveSelectedSongUp, () => CanMoveSelectedSong(-1));
        MoveSelectedSongDownCommand = new RelayCommand(MoveSelectedSongDown, () => CanMoveSelectedSong(1));
        ClearSearchCommand = new RelayCommand(() => _songListViewModel?.ClearSearch());

        CurrentPage = new WelcomeViewModel(NewLibraryCommand, OpenLibraryCommand);
    }

    public MusicLibrary Library
    {
        get => _library;
        private set => SetProperty(ref _library, value);
    }

    public PlayerViewModel Player { get; }
    public ICommand NewLibraryCommand { get; }
    public ICommand OpenLibraryCommand { get; }
    public ICommand SaveLibraryCommand { get; }
    public ICommand ShowAllSongsCommand { get; }
    public ICommand CreatePlaylistCommand { get; }
    public ICommand RenameSelectedPlaylistCommand { get; }
    public ICommand DeleteSelectedPlaylistCommand { get; }
    public ICommand ImportSongsCommand { get; }
    public ICommand EditSelectedSongCommand { get; }
    public ICommand AddSelectedSongToPlaylistCommand { get; }
    public ICommand PlaySelectedSongCommand { get; }
    public ICommand PlayPlaylistCommand { get; }
    public ICommand RemoveSelectedSongCommand { get; }
    public ICommand MoveSelectedSongUpCommand { get; }
    public ICommand MoveSelectedSongDownCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (SetProperty(ref _selectedPlaylist, value) && value is not null)
            {
                ShowPlaylist(value);
            }
        }
    }

    public bool HasOpenLibrary => _libraryService.CurrentPath is not null;

    private Song? CurrentSelectedSong
    {
        get
        {
            if (ReferenceEquals(CurrentPage, _playlistViewModel))
            {
                return _playlistViewModel?.SelectedSong;
            }

            return _songListViewModel?.SelectedSong;
        }
    }

    private void NewLibrary()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Musica library (*.musica.json)|*.musica.json|JSON files (*.json)|*.json|All files|*.*",
            FileName = "MyLibrary.musica.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        Library = _libraryService.CreateNew(dialog.FileName);
        SeedLabSongs();
        PrepareOpenedLibrary();
        SaveLibrary();
    }

    private void OpenLibrary()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Musica library (*.musica.json;*.json)|*.musica.json;*.json|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        Library = _libraryService.Load(dialog.FileName);
        PrepareOpenedLibrary();
    }

    private void PrepareOpenedLibrary()
    {
        AttachAutoSave();
        BuildSongListViewModel();
        CurrentPage = _songListViewModel;
        OnPropertyChanged(nameof(HasOpenLibrary));
        CommandManager.InvalidateRequerySuggested();
    }

    private void BuildSongListViewModel()
    {
        _songListViewModel = new SongListViewModel(
            Library.Songs,
            Library.Playlists,
            ImportSongsCommand,
            ClearSearchCommand,
            EditSelectedSongCommand,
            AddSelectedSongToPlaylistCommand,
            PlaySelectedSongCommand);
    }

    private void ShowAllSongs()
    {
        SelectedPlaylist = null;
        BuildSongListViewModel();
        CurrentPage = _songListViewModel;
    }

    private void ShowPlaylist(Playlist playlist)
    {
        _playlistViewModel = new PlaylistViewModel(
            playlist,
            PlayPlaylistCommand,
            PlaySelectedSongCommand,
            EditSelectedSongCommand,
            RemoveSelectedSongCommand,
            MoveSelectedSongUpCommand,
            MoveSelectedSongDownCommand);
        CurrentPage = _playlistViewModel;
        CommandManager.InvalidateRequerySuggested();
    }

    private void CreatePlaylist()
    {
        var viewModel = new PlaylistEditViewModel();
        var window = new PlaylistEditWindow(viewModel) { Owner = Application.Current.MainWindow };

        if (window.ShowDialog() == true)
        {
            var playlist = new Playlist
            {
                Name = viewModel.Name.Trim(),
                CoverArt = viewModel.CoverArt
            };
            AttachPlaylist(playlist);
            Library.Playlists.Add(playlist);
            SelectedPlaylist = playlist;
            SaveLibrary();
        }
    }

    private void RenameSelectedPlaylist()
    {
        if (SelectedPlaylist is null)
        {
            return;
        }

        var viewModel = new PlaylistEditViewModel(SelectedPlaylist);
        var window = new PlaylistEditWindow(viewModel) { Owner = Application.Current.MainWindow };

        if (window.ShowDialog() == true)
        {
            SelectedPlaylist.Name = viewModel.Name.Trim();
            SelectedPlaylist.CoverArt = viewModel.CoverArt;
            SaveLibrary();
        }
    }

    private void DeleteSelectedPlaylist()
    {
        if (SelectedPlaylist is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Delete playlist \"{SelectedPlaylist.Name}\"?",
            "Delete playlist",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        Library.Playlists.Remove(SelectedPlaylist);
        SelectedPlaylist = null;
        ShowAllSongs();
        SaveLibrary();
    }

    private void ImportSongs()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Audio files|*.mp3;*.wav;*.wma;*.flac;*.aac|All files|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        foreach (var fileName in dialog.FileNames)
        {
            try
            {
                var song = Song.FromAudioFile(fileName);
                song.DurationSeconds = TryReadWavDuration(fileName);
                AttachSong(song);
                Library.Songs.Add(song);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Import failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        SaveLibrary();
    }

    private void EditSelectedSong()
    {
        var song = CurrentSelectedSong;
        if (song is null)
        {
            return;
        }

        var viewModel = new SongEditViewModel(song);
        var window = new SongEditWindow(viewModel) { Owner = Application.Current.MainWindow };
        if (window.ShowDialog() == true)
        {
            SaveLibrary();
        }
    }

    private void AddSelectedSongToPlaylist()
    {
        if (_songListViewModel?.SelectedSong is not Song song || _songListViewModel.TargetPlaylist is not Playlist playlist)
        {
            return;
        }

        if (!playlist.Songs.Contains(song))
        {
            playlist.Songs.Add(song);
            playlist.RefreshSongCount();
            SaveLibrary();
        }
    }

    private void PlaySelectedSong()
    {
        var song = CurrentSelectedSong;
        if (song is not null)
        {
            Player.PlaySingle(song);
        }
    }

    private void PlayPlaylist()
    {
        if (SelectedPlaylist is not null)
        {
            Player.PlayQueue(SelectedPlaylist.Songs);
        }
    }

    private void RemoveSelectedSongFromPlaylist()
    {
        if (SelectedPlaylist is null || _playlistViewModel?.SelectedSong is not Song song)
        {
            return;
        }

        SelectedPlaylist.Songs.Remove(song);
        SelectedPlaylist.RefreshSongCount();
        SaveLibrary();
    }

    private void MoveSelectedSongUp()
    {
        MoveSelectedSong(-1);
    }

    private void MoveSelectedSongDown()
    {
        MoveSelectedSong(1);
    }

    private bool CanMoveSelectedSong(int offset)
    {
        if (SelectedPlaylist is null || _playlistViewModel?.SelectedSong is not Song song)
        {
            return false;
        }

        var index = SelectedPlaylist.Songs.IndexOf(song);
        var targetIndex = index + offset;
        return index >= 0 && targetIndex >= 0 && targetIndex < SelectedPlaylist.Songs.Count;
    }

    private void MoveSelectedSong(int offset)
    {
        if (SelectedPlaylist is null || _playlistViewModel?.SelectedSong is not Song song)
        {
            return;
        }

        var index = SelectedPlaylist.Songs.IndexOf(song);
        var targetIndex = index + offset;
        if (index < 0 || targetIndex < 0 || targetIndex >= SelectedPlaylist.Songs.Count)
        {
            return;
        }

        SelectedPlaylist.Songs.Move(index, targetIndex);
        _playlistViewModel.SelectedSong = song;
        SaveLibrary();
    }

    private void SaveLibrary()
    {
        _libraryService.Save(Library);
    }

    private void AttachAutoSave()
    {
        Library.Songs.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Song song in e.NewItems)
                {
                    AttachSong(song);
                }
            }

            SaveLibrary();
        };

        Library.Playlists.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Playlist playlist in e.NewItems)
                {
                    AttachPlaylist(playlist);
                }
            }

            SaveLibrary();
        };

        foreach (var song in Library.Songs)
        {
            AttachSong(song);
        }

        foreach (var playlist in Library.Playlists)
        {
            AttachPlaylist(playlist);
        }
    }

    private void AttachSong(Song song)
    {
        song.PropertyChanged += AutoSaveOnPropertyChanged;
    }

    private void AttachPlaylist(Playlist playlist)
    {
        playlist.PropertyChanged += AutoSaveOnPropertyChanged;
        playlist.Songs.CollectionChanged += PlaylistSongsChanged;
    }

    private void PlaylistSongsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not null)
        {
            foreach (var playlist in Library.Playlists.Where(p => ReferenceEquals(p.Songs, sender)))
            {
                playlist.RefreshSongCount();
            }
        }

        SaveLibrary();
    }

    private void AutoSaveOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveLibrary();
    }

    private void SeedLabSongs()
    {
        if (Library.Songs.Count > 0)
        {
            return;
        }

        Library.Songs.Add(Song.CreateMock(1, "Northern Lights", "Ava Stone", "Night Walks", 2021, 227));
        Library.Songs.Add(Song.CreateMock(2, "Red Vinyl", "The Glass Birds", "City Echoes", 2020, 194));
        Library.Songs.Add(Song.CreateMock(3, "Morning Static", "Leon Hale", "Signals", 2022, 251));
        Library.Songs.Add(Song.CreateMock(4, "Soft Exit", "Mira Vale", "Small Rooms", 2019, 208));
        Library.Songs.Add(Song.CreateMock(5, "Blue Arcade", "Nova Lane", "Pixel Heart", 2023, 236));
    }

    private static int TryReadWavDuration(string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".wav", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        try
        {
            using var reader = new BinaryReader(File.OpenRead(fileName));
            reader.BaseStream.Position = 22;
            var channels = reader.ReadInt16();
            var sampleRate = reader.ReadInt32();
            reader.BaseStream.Position = 34;
            var bitsPerSample = reader.ReadInt16();
            reader.BaseStream.Position = 40;
            var dataLength = reader.ReadInt32();
            var bytesPerSecond = sampleRate * channels * bitsPerSample / 8;
            return bytesPerSecond == 0 ? 0 : dataLength / bytesPerSecond;
        }
        catch (IOException)
        {
            return 0;
        }
    }
}

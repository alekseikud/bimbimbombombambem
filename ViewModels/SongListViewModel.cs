using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp3.Infrastructure;
using WpfApp3.Models;

namespace WpfApp3.ViewModels;

public class SongListViewModel : BaseViewModel
{
    private readonly ICollectionView _songsView;
    private string _searchText = "";
    private Song? _selectedSong;
    private Playlist? _targetPlaylist;

    public SongListViewModel(
        ObservableCollection<Song> songs,
        ObservableCollection<Playlist> playlists,
        ICommand importSongsCommand,
        ICommand clearSearchCommand,
        ICommand editSelectedSongCommand,
        ICommand addSelectedSongToPlaylistCommand,
        ICommand playSelectedSongCommand)
    {
        Songs = songs;
        Playlists = playlists;
        ImportSongsCommand = importSongsCommand;
        ClearSearchCommand = clearSearchCommand;
        EditSelectedSongCommand = editSelectedSongCommand;
        AddSelectedSongToPlaylistCommand = addSelectedSongToPlaylistCommand;
        PlaySelectedSongCommand = playSelectedSongCommand;

        _songsView = CollectionViewSource.GetDefaultView(Songs);
        _songsView.Filter = FilterSong;
    }

    public ObservableCollection<Song> Songs { get; }
    public ObservableCollection<Playlist> Playlists { get; }
    public ICollectionView SongsView => _songsView;
    public ICommand ImportSongsCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand EditSelectedSongCommand { get; }
    public ICommand AddSelectedSongToPlaylistCommand { get; }
    public ICommand PlaySelectedSongCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _songsView.Refresh();
            }
        }
    }

    public Song? SelectedSong
    {
        get => _selectedSong;
        set
        {
            if (SetProperty(ref _selectedSong, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public Playlist? TargetPlaylist
    {
        get => _targetPlaylist;
        set
        {
            if (SetProperty(ref _targetPlaylist, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public void ClearSearch()
    {
        SearchText = "";
    }

    private bool FilterSong(object item)
    {
        if (item is not Song song || string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var query = SearchText.Trim().ToLowerInvariant();
        return song.Title.ToLowerInvariant().Contains(query)
            || song.Artist.ToLowerInvariant().Contains(query)
            || song.Album.ToLowerInvariant().Contains(query)
            || song.Genre.ToLowerInvariant().Contains(query);
    }
}

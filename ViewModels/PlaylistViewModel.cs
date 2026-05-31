using System.Windows.Input;
using WpfApp3.Infrastructure;
using WpfApp3.Models;

namespace WpfApp3.ViewModels;

public class PlaylistViewModel : BaseViewModel
{
    private Song? _selectedSong;

    public PlaylistViewModel(
        Playlist playlist,
        ICommand playPlaylistCommand,
        ICommand playSelectedSongCommand,
        ICommand editSelectedSongCommand,
        ICommand removeSelectedSongCommand,
        ICommand moveSelectedSongUpCommand,
        ICommand moveSelectedSongDownCommand)
    {
        Playlist = playlist;
        PlayPlaylistCommand = playPlaylistCommand;
        PlaySelectedSongCommand = playSelectedSongCommand;
        EditSelectedSongCommand = editSelectedSongCommand;
        RemoveSelectedSongCommand = removeSelectedSongCommand;
        MoveSelectedSongUpCommand = moveSelectedSongUpCommand;
        MoveSelectedSongDownCommand = moveSelectedSongDownCommand;
    }

    public Playlist Playlist { get; }
    public ICommand PlayPlaylistCommand { get; }
    public ICommand PlaySelectedSongCommand { get; }
    public ICommand EditSelectedSongCommand { get; }
    public ICommand RemoveSelectedSongCommand { get; }
    public ICommand MoveSelectedSongUpCommand { get; }
    public ICommand MoveSelectedSongDownCommand { get; }

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
}

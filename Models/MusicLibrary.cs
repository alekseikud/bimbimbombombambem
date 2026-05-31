using System.Collections.ObjectModel;

namespace WpfApp3.Models;

public class MusicLibrary
{
    public ObservableCollection<Song> Songs { get; set; } = new();
    public ObservableCollection<Playlist> Playlists { get; set; } = new();
}

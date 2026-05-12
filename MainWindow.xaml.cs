using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class Song
    {
        public string cover;
        public string title;
        public string artist;
        public string album;
        public string genre;
        public string duration;
        public Song(string _cover,string _title, string _artist, string _album, string _genre, string _duration) {
            cover= _cover;
            title= _title;
            artist= _artist;
                
            album= _album;
            genre= _genre;
            duration= _duration;
                

        }
    }
    public class Playlist
    {
        public string cover;
        public string name;
        public Playlist(string _cover, string _name) { 
            cover
                = _cover;
            name = _name;
        }
    }
    public partial class MainWindow : Window
    {
        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }
        public MainWindow()
        {
            Songs = new ObservableCollection<Song>
            {
                new Song(
                    "_cover",
                    "_title",
                     "_artist",
                    "_album",
                    "_genre",
                    "_duration"
                ),
                new Song(
                    "_cover",
                    "_title",
                     "_artist",
                    "_album",
                    "_genre",
                    "_duration"
                )
            };
            InitializeComponent();
        }
    }
}
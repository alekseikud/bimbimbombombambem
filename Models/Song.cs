using WpfApp3.Infrastructure;

namespace WpfApp3.Models;

public class Song : BaseViewModel
{
    private int _id;
    private string _title = "";
    private string _artist = "";
    private string _album = "";
    private int _year;
    private string _duration = "";

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
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

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    public string Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }
}

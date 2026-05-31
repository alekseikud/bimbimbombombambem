using WpfApp3.Infrastructure;

namespace WpfApp3.Models;

public class Playlist : BaseViewModel
{
    private string _name = "";
    private string? _coverPath;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string? CoverPath
    {
        get => _coverPath;
        set => SetProperty(ref _coverPath, value);
    }
}

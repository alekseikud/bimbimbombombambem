using System.Windows.Input;

namespace WpfApp3.ViewModels;

public class WelcomeViewModel
{
    public WelcomeViewModel(ICommand createLibraryCommand, ICommand openLibraryCommand)
    {
        CreateLibraryCommand = createLibraryCommand;
        OpenLibraryCommand = openLibraryCommand;
    }

    public ICommand CreateLibraryCommand { get; }
    public ICommand OpenLibraryCommand { get; }
}

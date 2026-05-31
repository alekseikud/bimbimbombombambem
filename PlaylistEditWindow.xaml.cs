using System.Windows;
using WpfApp3.ViewModels;

namespace WpfApp3;

public partial class PlaylistEditWindow : Window
{
    public PlaylistEditWindow(PlaylistEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (_, accepted) =>
        {
            DialogResult = accepted;
            Close();
        };
    }
}

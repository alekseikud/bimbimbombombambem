using System.Windows;
using WpfApp3.ViewModels;

namespace WpfApp3;

public partial class SongEditWindow : Window
{
    public SongEditWindow(SongEditViewModel viewModel)
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

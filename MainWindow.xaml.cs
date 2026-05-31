using System.Windows;
using WpfApp3.ViewModels;

namespace WpfApp3;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}

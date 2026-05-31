using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp3;

public partial class PlaylistEditWindow : Window
{
    public string PlaylistName { get; private set; } = "";
    public string? CoverPath { get; private set; }

    public PlaylistEditWindow()
    {
        InitializeComponent();
    }

    private void BrowseCover_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.png;*.jpg;*.jpeg|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            CoverPath = dialog.FileName;
            CoverImage.Source = new BitmapImage(new System.Uri(CoverPath));
            NoCoverText.Visibility = Visibility.Collapsed;
        }
    }

    private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(NameTextBox.Text);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        PlaylistName = NameTextBox.Text.Trim();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

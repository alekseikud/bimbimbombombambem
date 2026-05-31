using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp3.Infrastructure;
using WpfApp3.Models;

namespace WpfApp3.ViewModels;

public class PlaylistEditViewModel : BaseViewModel
{
    private string _name = "";
    private byte[]? _coverArt;

    public PlaylistEditViewModel(Playlist? playlist = null)
    {
        if (playlist is not null)
        {
            Name = playlist.Name;
            CoverArt = playlist.CoverArt;
        }

        BrowseCoverCommand = new RelayCommand(BrowseCover);
        SaveCommand = new RelayCommand(() => CloseRequested?.Invoke(this, true), () => !string.IsNullOrWhiteSpace(Name));
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(this, false));
    }

    public event EventHandler<bool>? CloseRequested;

    public ICommand BrowseCoverCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public byte[]? CoverArt
    {
        get => _coverArt;
        set
        {
            if (SetProperty(ref _coverArt, value))
            {
                OnPropertyChanged(nameof(HasCover));
            }
        }
    }

    public bool HasCover => CoverArt is not null;

    private void BrowseCover()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.png;*.jpg;*.jpeg|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            CoverArt = File.ReadAllBytes(dialog.FileName);
        }
    }
}

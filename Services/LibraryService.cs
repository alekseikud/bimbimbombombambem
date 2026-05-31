using System.IO;
using System.Text.Json;
using WpfApp3.Models;

namespace WpfApp3.Services;

public class LibraryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string? CurrentPath { get; private set; }

    public MusicLibrary CreateNew(string path)
    {
        CurrentPath = path;
        var library = new MusicLibrary();
        Save(library);
        return library;
    }

    public MusicLibrary Open(string path)
    {
        CurrentPath = path;
        if (!File.Exists(path))
        {
            return new MusicLibrary();
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MusicLibrary>(json, JsonOptions) ?? new MusicLibrary();
    }

    public void Save(MusicLibrary library)
    {
        if (string.IsNullOrWhiteSpace(CurrentPath))
        {
            return;
        }

        var json = JsonSerializer.Serialize(library, JsonOptions);
        File.WriteAllText(CurrentPath, json);
    }
}

using System;
using System.IO;

namespace My.Extensions.Localization.Json.Internal;

public class JsonFileWatcher : IDisposable
{
    private const string JsonExtension = "*.json";

    private bool _disposed;
    
    private readonly FileSystemWatcher _filesWatcher;

    public event FileSystemEventHandler Changed;

    public JsonFileWatcher(string rootDirectory)
    {
        _filesWatcher = new(rootDirectory)
        {
            Filter = JsonExtension,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _filesWatcher.Changed += (s, e) => Changed?.Invoke(s, e);
    }

    ~JsonFileWatcher()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(true);
    }

    public virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _filesWatcher.Dispose();
        }

        _disposed = true;
    }
}

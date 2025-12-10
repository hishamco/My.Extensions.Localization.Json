using System;
using System.IO;

namespace My.Extensions.Localization.Json.Internal;

/// <summary>
/// Provides a mechanism for monitoring changes to JSON files within a specified directory.
/// </summary>
public class JsonFileWatcher : IDisposable
{
    private const string JsonExtension = "*.json";

    private bool _disposed;
    
    private readonly FileSystemWatcher _filesWatcher;

    /// <summary>
    /// Occurs when a file or directory in the specified path is changed.
    /// </summary>
    public event FileSystemEventHandler Changed;

    /// <summary>
    /// Initializes a new instance of the JsonFileWatcher class to monitor changes to JSON files in the specified
    /// directory.
    /// </summary>
    /// <param name="rootDirectory">The path to the directory to monitor for changes to JSON files. Must be a valid directory path.</param>
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

    /// <summary>
    /// Finalizes the JsonFileWatcher instance and releases unmanaged resources before the object is reclaimed by
    /// garbage collection.
    /// </summary>
    ~JsonFileWatcher()
    {
        Dispose(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(true);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

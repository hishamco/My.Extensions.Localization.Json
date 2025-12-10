using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace My.Extensions.Localization.Json.Internal;

public class JsonResourceManager
{
    private readonly List<JsonFileWatcher> _jsonFileWatchers = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _resourcesCache = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _loadedFilesCache = new();

    public JsonResourceManager(string resourcesPath, string resourceName = null)
        : this(new[] { resourcesPath }, fallBackToParentUICultures: true, resourceName)
    {
    }

    public JsonResourceManager(string[] resourcesPaths, bool fallBackToParentUICultures, string resourceName = null)
    {
        ResourcesPaths = resourcesPaths ?? Array.Empty<string>();
        ResourceName = resourceName;
        FallBackToParentUICultures = fallBackToParentUICultures;
        
        foreach (var path in ResourcesPaths)
        {
            SetupFileWatcher(path);
        }
    }

    public JsonResourceManager(string[] resourcesPaths, string resourceName = null)
        : this(resourcesPaths, fallBackToParentUICultures: true, resourceName)
    {
    }

    public string ResourceName { get; }

    public string[] ResourcesPaths { get; }

    public string ResourcesFilePath { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to fall back to parent UI cultures
    /// when a localized string is not found for the current culture.
    /// </summary>
    public bool FallBackToParentUICultures { get; }

    public virtual ConcurrentDictionary<string, string> GetResourceSet(CultureInfo culture, bool tryParents)
    {
        TryLoadResourceSet(culture);

        var key = $"{ResourceName}.{culture.Name}";
        if (!_resourcesCache.ContainsKey(key))
        {
            return null;
        }

        if (tryParents)
        {
            var allResources = new ConcurrentDictionary<string, string>();
            do
            {
                if (_resourcesCache.TryGetValue(key, out var resources))
                {
                    foreach (var entry in resources)
                    {
                        allResources.TryAdd(entry.Key, entry.Value);
                    }
                }

                culture = culture.Parent;
            } while (culture != CultureInfo.InvariantCulture);

            return allResources;
        }
        else
        {
            _resourcesCache.TryGetValue(key, out var resources);

            return resources;
        }
    }

    public virtual string GetString(string name)
    {
        var culture = CultureInfo.CurrentUICulture;
        GetResourceSet(culture, tryParents: FallBackToParentUICultures);

        if (_resourcesCache.IsEmpty)
        {
            return null;
        }

        do
        {
            var key = $"{ResourceName}.{culture.Name}";
            if (_resourcesCache.TryGetValue(key, out var resources))
            {
                if (resources.TryGetValue(name, out var value))
                {
                    return value.ToString();
                }
            }

            if (!FallBackToParentUICultures)
            {
                break;
            }

            culture = culture.Parent;
        } while (culture != culture.Parent);

        return null;
    }

    public virtual string GetString(string name, CultureInfo culture)
    {
        GetResourceSet(culture, tryParents: FallBackToParentUICultures);

        if (_resourcesCache.IsEmpty)
        {
            return null;
        }

        var currentCulture = culture;
        do
        {
            var key = $"{ResourceName}.{currentCulture.Name}";
            if (_resourcesCache.TryGetValue(key, out var resources))
            {
                if (resources.TryGetValue(name, out var value))
                {
                    return value.ToString();
                }
            }

            if (!FallBackToParentUICultures)
            {
                break;
            }

            currentCulture = currentCulture.Parent;
        } while (currentCulture != currentCulture.Parent);

        return null;
    }

    private void TryLoadResourceSet(CultureInfo culture)
    {
        // Load from all resources paths (merging resources, first path takes precedence)
        foreach (var path in ResourcesPaths)
        {
            TryLoadResourceSetFromPath(path, culture);
        }
    }

    private void TryLoadResourceSetFromPath(string basePath, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(ResourceName))
        {
            var file = Path.Combine(basePath, $"{culture.Name}.json");

            TryAddResources(file, culture);
        }
        else
        {
            var resourceFiles = Enumerable.Empty<string>();
            var rootCulture = culture.Name[..2];
            if (ResourceName.Contains('.'))
            {
                if (Directory.Exists(basePath))
                {
                    resourceFiles = Directory.EnumerateFiles(basePath, $"{ResourceName}.{rootCulture}*.json");
                }

                if (!resourceFiles.Any())
                {
                    resourceFiles = GetResourceFiles(basePath, rootCulture);
                }
            }
            else
            {
                resourceFiles = GetResourceFiles(basePath, rootCulture);
            }

            foreach (var file in resourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var cultureName = fileName[(fileName.LastIndexOf('.') + 1)..];

                var fileCulture = CultureInfo.GetCultureInfo(cultureName);

                TryAddResources(file, fileCulture);
            }
        }

        IEnumerable<string> GetResourceFiles(string baseResourcesPath, string cultureName)
        {
            var resourcePath = ResourceName.Replace('.', Path.AltDirectorySeparatorChar);
            var resourcePathLastDirectorySeparatorIndex = resourcePath.LastIndexOf(Path.AltDirectorySeparatorChar);
            var resourceName = resourcePath[(resourcePathLastDirectorySeparatorIndex + 1)..];
            var resourcesPath = resourcePathLastDirectorySeparatorIndex == -1
                ? baseResourcesPath
                : Path.Combine(baseResourcesPath, resourcePath[..resourcePathLastDirectorySeparatorIndex]);

            return Directory.Exists(resourcesPath)
                ? Directory.EnumerateFiles(resourcesPath, $"{resourceName}.{cultureName}*.json")
                : [];
        }

        void TryAddResources(string resourceFile, CultureInfo resourceCulture)
        {
            var key = $"{ResourceName}.{resourceCulture.Name}";
            
            // Track loaded files to avoid re-loading
            var loadedFiles = _loadedFilesCache.GetOrAdd(key, _ => new HashSet<string>());
            if (!loadedFiles.Add(resourceFile))
            {
                // File already loaded for this key, skip
                return;
            }
            
            if (!_resourcesCache.ContainsKey(key))
            {
                var resources = JsonResourceLoader.Load(resourceFile);

                _resourcesCache.TryAdd(key, new ConcurrentDictionary<string, string>(resources));
            }
            else
            {
                // Merge resources from additional paths (don't override existing keys)
                var existingResources = _resourcesCache[key];
                var additionalResources = JsonResourceLoader.Load(resourceFile);
                
                foreach (var item in additionalResources)
                {
                    existingResources.TryAdd(item.Key, item.Value);
                }
            }
        }
    }

    private void SetupFileWatcher(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }
        
        var watcher = new JsonFileWatcher(path);
        watcher.Changed += RefreshResourcesCache;
        _jsonFileWatchers.Add(watcher);
    }

    private void RefreshResourcesCache(object sender, FileSystemEventArgs e)
    {
        var key = Path.GetFileNameWithoutExtension(e.FullPath);
        if (_resourcesCache.TryGetValue(key, out var resources))
        {
            if (!resources.IsEmpty)
            {
                resources.Clear();

                var freshResources = JsonResourceLoader.Load(e.FullPath);

                foreach (var item in freshResources)
                {
                    _resourcesCache[key].TryAdd(item.Key, item.Value);
                }
            }
        }
    }
}

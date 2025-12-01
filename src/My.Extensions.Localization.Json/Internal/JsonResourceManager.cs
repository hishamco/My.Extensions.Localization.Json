using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace My.Extensions.Localization.Json.Internal;

public class JsonResourceManager
{
    private readonly JsonFileWatcher _jsonFileWatcher;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _resourcesCache = new();


    public JsonResourceManager(string resourcesPath, string resourceName = null)
        : this(resourcesPath, resourceName, fallBackToParentUICultures: true)
    {
    }

    public JsonResourceManager(string resourcesPath, string resourceName, bool fallBackToParentUICultures)
    {
        ResourcesPath = resourcesPath;
        ResourceName = resourceName;
        FallBackToParentUICultures = fallBackToParentUICultures;
        
        _jsonFileWatcher = new(resourcesPath);
        _jsonFileWatcher.Changed += RefreshResourcesCache;
    }

    public string ResourceName { get; }

    public string ResourcesPath { get; }

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
        if (string.IsNullOrEmpty(ResourceName))
        {
            var file = Path.Combine(ResourcesPath, $"{culture.Name}.json");

            TryAddResources(file);
        }
        else
        {
            var resourceFiles = Enumerable.Empty<string>();
            var rootCulture = culture.Name[..2];
            if (ResourceName.Contains('.'))
            {
                resourceFiles = Directory.EnumerateFiles(ResourcesPath, $"{ResourceName}.{rootCulture}*.json");

                if (!resourceFiles.Any())
                {
                    resourceFiles = GetResourceFiles(rootCulture);
                }
            }
            else
            {
                resourceFiles = GetResourceFiles(rootCulture);
            }

            foreach (var file in resourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var cultureName = fileName[(fileName.LastIndexOf('.') + 1)..];

                culture = CultureInfo.GetCultureInfo(cultureName);

                TryAddResources(file);
            }
        }

        IEnumerable<string> GetResourceFiles(string culture)
        {
            var resourcePath = ResourceName.Replace('.', Path.AltDirectorySeparatorChar);
            var resourcePathLastDirectorySeparatorIndex = resourcePath.LastIndexOf(Path.AltDirectorySeparatorChar);
            var resourceName = resourcePath[(resourcePathLastDirectorySeparatorIndex + 1)..];
            var resourcesPath = resourcePathLastDirectorySeparatorIndex == -1
                ? ResourcesPath
                : Path.Combine(ResourcesPath, resourcePath[..resourcePathLastDirectorySeparatorIndex]);

            return Directory.Exists(resourcesPath)
                ? Directory.EnumerateFiles(resourcesPath, $"{resourceName}.{culture}*.json")
                : [];
        }

        void TryAddResources(string resourceFile)
        {
            var key = $"{ResourceName}.{culture.Name}";
            if (!_resourcesCache.ContainsKey(key))
            {
                var resources = JsonResourceLoader.Load(resourceFile);

                _resourcesCache.TryAdd(key, new ConcurrentDictionary<string, string>(resources));
            }
        }
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

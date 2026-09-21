using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace My.Extensions.Localization.Json.Internal;

/// <summary>
/// Provides access to localized string resources loaded from JSON files, supporting culture-specific lookups and
/// optional fallback to parent UI cultures.
/// </summary>
public class JsonResourceManager
{
    private readonly List<JsonFileWatcher> _jsonFileWatchers = [];
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _resourcesCache = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _loadedFilesCache = new();

    /// <summary>
    /// Initializes a new instance of the JsonResourceManager class using the specified resource directory and optional
    /// resource name.
    /// </summary>
    /// <param name="resourcesPath">The path to the directory containing the JSON resource files. Cannot be null or empty.</param>
    /// <param name="resourceName">The name of the resource to load. If null, the default resource name is used.</param>
    public JsonResourceManager(string resourcesPath, string resourceName = null)
        : this([resourcesPath], fallBackToParentUICultures: true, resourceName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the JsonResourceManager class using the specified resource file paths and
    /// configuration options.
    /// </summary>
    /// <param name="resourcesPaths">An array of file system paths to JSON resource files to be managed. If null, an empty array is used.</param>
    /// <param name="fallBackToParentUICultures">Indicates whether resource lookups should fall back to parent UI cultures when a resource is not found for the
    /// requested culture.</param>
    /// <param name="resourceName">The name of the resource to be managed. If null, the manager will use the default resource name resolution.</param>
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

    /// <summary>
    /// Initializes a new instance of the JsonResourceManager class using the specified resource file paths and an
    /// optional resource name.
    /// </summary>
    /// <param name="resourcesPaths">An array of file system paths to JSON resource files to be managed. Each path should point to a valid resource
    /// file. Cannot be null.</param>
    /// <param name="resourceName">The name of the resource to be used for lookups. If null, the default resource name will be used.</param>
    public JsonResourceManager(string[] resourcesPaths, string resourceName = null)
        : this(resourcesPaths, fallBackToParentUICultures: true, resourceName)
    {
    }

    /// <summary>
    /// Gets the name of the resource associated with this instance.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Gets the collection of file system paths to resource files associated with the current instance.
    /// </summary>
    public string[] ResourcesPaths { get; }

    /// <summary>
    /// Gets the file path to the resources file used by the application.
    /// </summary>
    public string ResourcesFilePath { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to fall back to parent UI cultures
    /// when a localized string is not found for the current culture.
    /// </summary>
    public bool FallBackToParentUICultures { get; }

    /// <summary>
    /// Retrieves the set of localized resources for the specified culture, optionally including resources from parent
    /// cultures.
    /// </summary>
    /// <param name="culture">The culture for which to retrieve the resource set. This determines which localized resources are returned.</param>
    /// <param name="tryParents">If <see langword="true"/>, resources from parent cultures are included in the result; otherwise, only resources
    /// for the specified culture are returned.</param>
    /// <returns>A <see cref="ConcurrentDictionary{string, string}"/> containing the resources for the specified culture, or <see
    /// langword="null"/> if no resources are available for that culture.</returns>
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
                key = $"{ResourceName}.{culture.Name}";
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

    /// <summary>
    /// Retrieves the localized string resource associated with the specified name for the current UI culture.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve. This value is case-sensitive and must not be null.</param>
    /// <returns>The localized string value if found; otherwise, null.</returns>
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

    /// <summary>
    /// Retrieves the localized string resource associated with the specified name and culture.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve. This value is case-sensitive and must not be null.</param>
    /// <param name="culture">The culture for which the resource should be retrieved. If the resource is not found for this culture and parent
    /// culture fallback is enabled, parent cultures will be searched.</param>
    /// <returns>The localized string value for the specified resource name and culture, or null if the resource is not found.</returns>
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

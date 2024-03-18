using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace My.Extensions.Localization.Json.Internal;

public class JsonResourceManager(string resourcesPath, string resourceName = null)
{
    private static readonly JsonDocumentOptions _jsonDocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _resourcesCache = new();

    public string ResourceName { get; } = resourceName;

    public string ResourcesPath { get; } = resourcesPath;

    public string ResourcesFilePath { get; private set; }

    public virtual ConcurrentDictionary<string, string> GetResourceSet(CultureInfo culture, bool tryParents)
    {
        TryLoadResourceSet(culture);

        if (!_resourcesCache.ContainsKey(culture.Name))
        {
            return null;
        }

        if (tryParents)
        {
            var allResources = new ConcurrentDictionary<string, string>();
            do
            {
                if (_resourcesCache.TryGetValue(culture.Name, out ConcurrentDictionary<string, string> resources))
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
            _resourcesCache.TryGetValue(culture.Name, out ConcurrentDictionary<string, string> resources);

            return resources;
        }
    }

    public virtual string GetString(string name)
    {
        var culture = CultureInfo.CurrentUICulture;
        GetResourceSet(culture, tryParents: true);

        if (_resourcesCache.IsEmpty)
        {
            return null;
        }

        do
        {
            if (_resourcesCache.ContainsKey(culture.Name))
            {
                if (_resourcesCache[culture.Name].TryGetValue(name, out string value))
                {
                    return value;
                }
            }

            culture = culture.Parent;
        } while (culture != culture.Parent);

        return null;
    }

    public virtual string GetString(string name, CultureInfo culture)
    {
        GetResourceSet(culture, tryParents: true);

        if (_resourcesCache.IsEmpty)
        {
            return null;
        }

        if (!_resourcesCache.ContainsKey(culture.Name))
        {
            return null;
        }

        return _resourcesCache[culture.Name].TryGetValue(name, out string value)
            ? value
            : null;
    }

    private void TryLoadResourceSet(CultureInfo culture)
    {
        if (string.IsNullOrEmpty(ResourceName))
        {
            var file = Path.Combine(ResourcesPath, $"{culture.Name}.json");
            var resources = LoadJsonResources(file);

            _resourcesCache.TryAdd(culture.Name, new ConcurrentDictionary<string, string>(resources.ToDictionary(r => r.Key, r => r.Value)));
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
                var resources = LoadJsonResources(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var cultureName = fileName[(fileName.LastIndexOf('.') + 1)..];

                culture = CultureInfo.GetCultureInfo(cultureName);

                if (_resourcesCache.TryGetValue(culture.Name, out ConcurrentDictionary<string, string> value))
                {
                    foreach (var resource in resources)
                    {
                        value.TryAdd(resource.Key, resource.Value);
                    }
                }
                else
                {
                    _resourcesCache.TryAdd(culture.Name, new ConcurrentDictionary<string, string>(resources.ToDictionary(r => r.Key, r => r.Value)));
                }
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
    }

    private static IDictionary<string, string> LoadJsonResources(string filePath)
    {
        var resources = new Dictionary<string, string>();
        if (File.Exists(filePath))
        {
            using var reader = new StreamReader(filePath);

            using var document = JsonDocument.Parse(reader.BaseStream, _jsonDocumentOptions);

            resources = document.RootElement.EnumerateObject().ToDictionary(e => e.Name, e => e.Value.ToString());
        }

        return resources;
    }
}

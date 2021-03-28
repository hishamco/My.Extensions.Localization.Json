using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace My.Extensions.Localization.Json.Internal
{
    public class JsonResourceManager
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _resourcesCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        private readonly Assembly _resourcesAssembly;

        public JsonResourceManager(string baseName, Assembly assembly)
        {
            this._resourcesAssembly = assembly;
            this.ResourceName = baseName;
        }

        public string ResourceName { get; }

        public string ResourcesPath { get; }

        public string ResourcesFilePath { get; private set; }

        public virtual ConcurrentDictionary<string, string> GetResourceSet(CultureInfo culture, bool tryParents)
        {
            TryLoadResourceSet(culture);

            if(!_resourcesCache.ContainsKey(culture.Name))
            {
                return null;
            }

            if (tryParents)
            {
                var allResources = new ConcurrentDictionary<string, string>();
                do
                {
                    if(_resourcesCache.TryGetValue(culture.Name, out ConcurrentDictionary<string, string> resources))
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

            if (_resourcesCache.Count == 0)
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

            if (_resourcesCache.Count == 0)
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
            var resourceFiles = new List<string>();

            var rootCulture = culture.Name.Substring(0, 2);

            if (ResourceName.Contains("."))
            {
                var availableResources = _resourcesAssembly?.GetManifestResourceNames();

                if (availableResources != null && availableResources.Length > 0)
                {
                    foreach (var resource in availableResources)
                    {
                        if (resource.Contains($"{ResourceName}.{rootCulture}"))
                        {
                            resourceFiles.Add(resource);
                        }
                    }
                }

                if (resourceFiles.Count() == 0)
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

                var cultureName = fileName.Substring(fileName.LastIndexOf(".") + 1);

                culture = CultureInfo.GetCultureInfo(cultureName);

                if (_resourcesCache.ContainsKey(culture.Name))
                {
                    foreach (var resource in resources)
                    {
                        _resourcesCache[culture.Name].TryAdd(resource.Key, resource.Value);
                    }
                }
                else
                {
                    _resourcesCache.TryAdd(culture.Name, new ConcurrentDictionary<string, string>(resources.ToDictionary(r => r.Key, r => r.Value)));
                }
            }

            List<string> GetResourceFiles(string culture)
            {
                var resoourceNames = _resourcesAssembly?.GetManifestResourceNames();

                var resources = new List<string>();

                if (resoourceNames != null && resoourceNames.Length > 0)
                {
                    foreach (var resource in resoourceNames)
                    {
                        if (resource.Contains($"{ResourceName}.{rootCulture}"))
                        {
                            resources.Add(resource);
                        }
                    }
                }

                return resources;
            }
        }

        private IDictionary<string, string> LoadJsonResources(string fileName)
        {
            using var fileStream = _resourcesAssembly.GetManifestResourceStream(fileName);

            if (fileStream == null) return null;

            using var streamReader = new StreamReader(fileStream);

            var content = streamReader.ReadToEnd();

            var keyValues = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, string>>(content);

            return keyValues ?? new Dictionary<string, string>();
        }
    }
}

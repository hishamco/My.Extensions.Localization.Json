using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using My.Extensions.Localization.Json.Caching;

namespace My.Extensions.Localization.Json.Internal
{
    public class JsonStringProvider : IResourceStringProvider
    {
        private readonly IResourceNamesCache _resourceNamesCache;
        private readonly JsonResourceManager _jsonResourceManager;
        private readonly Assembly _assembly;
        private readonly string _resourceBaseName;

        public JsonStringProvider(
            IResourceNamesCache resourceCache,
            JsonResourceManager jsonResourceManager,
            Assembly assembly,
            string baseName)
        {
            _jsonResourceManager = jsonResourceManager;
            _resourceNamesCache = resourceCache;
            _assembly = assembly;
            _resourceBaseName = baseName;
        }

        private string GetResourceCacheKey(CultureInfo culture)
        {
            var resourceName = _jsonResourceManager.ResourceName;

            return $"Culture={culture.Name};resourceName={resourceName};Assembly={_assembly.FullName}";
        }

        public IList<string> GetAllResourceStrings(CultureInfo culture, bool throwOnMissing)
        {
            var cacheKey = GetResourceCacheKey(culture);

            return _resourceNamesCache.GetOrAdd(cacheKey, _ =>
            {
                var resourceSet = _jsonResourceManager.GetResourceSet(culture, tryParents: false);
                if (resourceSet == null)
                {
                    if (throwOnMissing)
                    {
                        throw new MissingManifestResourceException($"The manifest resource for the culture '{culture.Name}' is missing.");
                    }
                    else
                    {
                        return null;
                    }
                }

                var names = new List<string>();
                foreach (var entry in resourceSet)
                {
                    names.Add(entry.Key);
                }

                return names;
            });
        }
    }
}

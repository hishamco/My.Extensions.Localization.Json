using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Localization.Internal;

namespace My.Extensions.Localization.Json.Internal
{
    public class JsonStringProvider : IResourceStringProvider
    {
        private readonly IResourceNamesCache _resourceNamesCache;
        private readonly JsonResourceManager _jsonResourceManager;

        public JsonStringProvider(IResourceNamesCache resourceCache, JsonResourceManager jsonResourceManager)
        {
            _jsonResourceManager = jsonResourceManager;
            _resourceNamesCache = resourceCache;
        }

        private string GetResourceCacheKey(CultureInfo culture)
        {
            var resourceName = _jsonResourceManager.ResourceName;

            return $"Culture={culture.Name};resourceName={resourceName}";
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

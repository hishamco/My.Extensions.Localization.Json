using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using My.Extensions.Localization.Json.Caching;

namespace My.Extensions.Localization.Json.Internal;

public class JsonStringProvider(IResourceNamesCache resourceNamesCache, JsonResourceManager jsonResourceManager) : IResourceStringProvider
{
    private string GetResourceCacheKey(CultureInfo culture)
    {
        var resourceName = jsonResourceManager.ResourceName;

        return $"Culture={culture.Name};resourceName={resourceName}";
    }

    public IList<string> GetAllResourceStrings(CultureInfo culture, bool throwOnMissing)
    {
        var cacheKey = GetResourceCacheKey(culture);

        return resourceNamesCache.GetOrAdd(cacheKey, _ =>
        {
            var resourceSet = jsonResourceManager.GetResourceSet(culture, tryParents: false);
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

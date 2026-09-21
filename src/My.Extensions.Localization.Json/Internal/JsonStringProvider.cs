using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using My.Extensions.Localization.Json.Caching;

namespace My.Extensions.Localization.Json.Internal;

/// <summary>
/// Provides resource string retrieval for JSON-based resource sets, supporting culture-specific access and caching of
/// resource names.
/// </summary>
/// <param name="resourceNamesCache">A cache used to store and retrieve lists of resource names for specific cultures, improving performance by avoiding
/// repeated resource enumeration.</param>
/// <param name="jsonResourceManager">The resource manager responsible for accessing JSON resource sets and their associated strings for a given culture.</param>
public class JsonStringProvider(IResourceNamesCache resourceNamesCache, JsonResourceManager jsonResourceManager) : IResourceStringProvider
{
    private string GetResourceCacheKey(CultureInfo culture)
    {
        var resourceName = jsonResourceManager.ResourceName;

        return $"Culture={culture.Name};resourceName={resourceName}";
    }

    /// <inheritdoc />
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

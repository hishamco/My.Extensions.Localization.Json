using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace My.Extensions.Localization.Json.Caching;

public class ResourceNamesCache : IResourceNamesCache
{
    private readonly ConcurrentDictionary<string, IList<string>> _cache = new();

    public IList<string> GetOrAdd(string name, Func<string, IList<string>> valueFactory)
        => _cache.GetOrAdd(name, valueFactory);
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace My.Extensions.Localization.Json.Caching;

/// <summary>
/// Provides a thread-safe cache for storing and retrieving lists of resource names by key.
/// </summary>
public class ResourceNamesCache : IResourceNamesCache
{
    private readonly ConcurrentDictionary<string, IList<string>> _cache = new();

    /// <inheritdoc />
    public IList<string> GetOrAdd(string name, Func<string, IList<string>> valueFactory)
        => _cache.GetOrAdd(name, valueFactory);
}

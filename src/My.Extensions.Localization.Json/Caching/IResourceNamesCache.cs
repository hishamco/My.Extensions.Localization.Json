using System;
using System.Collections.Generic;

namespace My.Extensions.Localization.Json.Caching;

/// <summary>
/// Defines a cache for storing and retrieving collections of resource names by key.
/// </summary>
public interface IResourceNamesCache
{
    /// <summary>
    /// Gets the list of strings associated with the specified name, or adds a new list using the provided factory if
    /// none exists.
    /// </summary>
    /// <param name="name">The key used to locate the associated list of strings. Cannot be null.</param>
    /// <param name="valueFactory">A function that generates a new list of strings if the specified name does not exist. Cannot be null.</param>
    /// <returns>The list of strings associated with the specified name. If the name was not present, returns the newly created
    /// list from the factory.</returns>
    IList<string> GetOrAdd(string name, Func<string, IList<string>> valueFactory);
}

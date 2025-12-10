using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using My.Extensions.Localization.Json.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace My.Extensions.Localization.Json;

using My.Extensions.Localization.Json.Caching;

/// <summary>
/// Provides string localization services using JSON-based resource files. Supports retrieving localized strings and
/// formatting them for the current or specified culture.
/// </summary>
public class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, object> _missingManifestCache = new();
    private readonly JsonResourceManager _jsonResourceManager;
    private readonly IResourceStringProvider _resourceStringProvider;
    private readonly ILogger _logger;

    private string _searchedLocation = string.Empty;

    /// <summary>
    /// Initializes a new instance of the JsonStringLocalizer class using the specified resource manager, resource names
    /// cache, and logger.
    /// </summary>
    /// <param name="jsonResourceManager">The resource manager that provides access to JSON-based localization resources.</param>
    /// <param name="resourceNamesCache">The cache used to store and retrieve resource names for efficient localization lookups.</param>
    /// <param name="logger">The logger used to record localization-related events and errors.</param>
    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceNamesCache resourceNamesCache,
        ILogger logger)
        : this(jsonResourceManager,
            new JsonStringProvider(resourceNamesCache, jsonResourceManager),
            logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the JsonStringLocalizer class using the specified resource manager, string
    /// provider, and logger.
    /// </summary>
    /// <param name="jsonResourceManager">The resource manager that provides access to JSON-based localization resources.</param>
    /// <param name="resourceStringProvider">The provider used to retrieve localized strings from resources.</param>
    /// <param name="logger">The logger used to record localization-related events and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="jsonResourceManager"/>, <paramref name="resourceStringProvider"/>, or <paramref
    /// name="logger"/> is null.</exception>
    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceStringProvider resourceStringProvider,
        ILogger logger)
    {
        _jsonResourceManager = jsonResourceManager ?? throw new ArgumentNullException(nameof(jsonResourceManager));
        _resourceStringProvider = resourceStringProvider ?? throw new ArgumentNullException(nameof(resourceStringProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Obsolete("This constructor has been deprected and will be removed in the upcoming major release.")]
    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceStringProvider resourceStringProvider,
        IResourceNamesCache resourceNamesCache,
        ILogger logger)
    {
        _jsonResourceManager = jsonResourceManager ?? throw new ArgumentNullException(nameof(jsonResourceManager));
        _resourceStringProvider = resourceStringProvider ?? throw new ArgumentNullException(nameof(resourceStringProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public LocalizedString this[string name]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(name);

            var value = GetStringSafely(name, null);

            return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
        }
    }

    /// <inheritdoc/>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(name);

            var format = GetStringSafely(name, null);
            var value = string.Format(format ?? name, arguments);

            return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _searchedLocation);
        }
    }

    /// <inheritdoc/>
    public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

    /// <summary>
    /// Returns all localized strings available for the specified culture, optionally including strings from parent
    /// cultures.
    /// </summary>
    /// <param name="includeParentCultures">true to include localized strings from parent cultures in addition to the specified culture; otherwise, false.</param>
    /// <param name="culture">The culture for which to retrieve localized strings. Cannot be null.</param>
    /// <returns>An enumerable collection of LocalizedString objects representing all available localized strings for the
    /// specified culture.</returns>
    protected virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        var resourceNames = includeParentCultures
            ? GetResourceNamesFromCultureHierarchy(culture).AsEnumerable()
            : _resourceStringProvider.GetAllResourceStrings(culture, true);

        foreach (var name in resourceNames)
        {
            var value = GetStringSafely(name, culture);
            yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
        }
    }

    /// <summary>
    /// Retrieves the localized string resource for the specified name and culture, returning null if the resource is
    /// missing or unavailable.
    /// </summary>
    /// <param name="name">The name of the string resource to retrieve. Cannot be null.</param>
    /// <param name="culture">The culture for which to retrieve the resource. If null, the current UI culture is used.</param>
    /// <returns>The localized string resource associated with the specified name and culture, or null if the resource is not
    /// found.</returns>
    protected string GetStringSafely(string name, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(name);

        var keyCulture = culture ?? CultureInfo.CurrentUICulture;
        var cacheKey = $"name={name}&culture={keyCulture.Name}";

        _logger.SearchedLocation(name, _jsonResourceManager.ResourcesFilePath, keyCulture);

        if (_missingManifestCache.ContainsKey(cacheKey))
        {
            return null;
        }

        try
        {
            return culture == null
                ? _jsonResourceManager.GetString(name)
                : _jsonResourceManager.GetString(name, culture);
        }
        catch (MissingManifestResourceException)
        {
            _missingManifestCache.TryAdd(cacheKey, null);
            
            return null;
        }
    }

    private HashSet<string> GetResourceNamesFromCultureHierarchy(CultureInfo startingCulture)
    {
        var currentCulture = startingCulture;
        var resourceNames = new HashSet<string>();

        while (currentCulture != currentCulture.Parent)
        {
            var cultureResourceNames = _resourceStringProvider.GetAllResourceStrings(currentCulture, false);

            if (cultureResourceNames != null)
            {
                foreach (var resourceName in cultureResourceNames)
                {
                    resourceNames.Add(resourceName);
                }
            }

            currentCulture = currentCulture.Parent;
        }

        return resourceNames;
    }
}

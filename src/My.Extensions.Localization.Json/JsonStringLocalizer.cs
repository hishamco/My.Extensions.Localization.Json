using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using My.Extensions.Localization.Json.Internal;

namespace My.Extensions.Localization.Json;

using My.Extensions.Localization.Json.Caching;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, object> _missingManifestCache = new();
    private readonly JsonResourceManager _jsonResourceManager;
    private readonly IResourceStringProvider _resourceStringProvider;
    private readonly ILogger _logger;

    private string _searchedLocation = string.Empty;

    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceNamesCache resourceNamesCache,
        ILogger logger)
        : this(jsonResourceManager,
            new JsonStringProvider(resourceNamesCache, jsonResourceManager),
            resourceNamesCache,
            logger)
    {

    }

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

    public LocalizedString this[string name]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(name);

            var value = GetStringSafely(name, null);

            return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
        }
    }

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

    public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

    [Obsolete("This method is obsolete and will be removed in a future version.")]
    public IStringLocalizer WithCulture(CultureInfo culture) => this;

    protected virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        var resourceNames = includeParentCultures
            ? GetResourceNamesFromCultureHierarchy(culture)
            : _resourceStringProvider.GetAllResourceStrings(culture, true);

        foreach (var name in resourceNames)
        {
            var value = GetStringSafely(name, culture);
            yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
        }
    }

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

    private IEnumerable<string> GetResourceNamesFromCultureHierarchy(CultureInfo startingCulture)
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

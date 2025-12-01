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

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, object> _missingManifestCache = new();
    private readonly JsonResourceManager _jsonResourceManager;
    private readonly IResourceStringProvider _resourceStringProvider;
    private readonly ILogger _logger;
    private readonly MissingLocalizationBehavior _missingLocalizationBehavior;

    private string _searchedLocation = string.Empty;

    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceNamesCache resourceNamesCache,
        ILogger logger)
        : this(jsonResourceManager,
            new JsonStringProvider(resourceNamesCache, jsonResourceManager),
            logger,
            MissingLocalizationBehavior.Ignore)
    {
    }

    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceNamesCache resourceNamesCache,
        ILogger logger,
        MissingLocalizationBehavior missingLocalizationBehavior)
        : this(jsonResourceManager,
            new JsonStringProvider(resourceNamesCache, jsonResourceManager),
            logger,
            missingLocalizationBehavior)
    {
    }

    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceStringProvider resourceStringProvider,
        ILogger logger)
        : this(jsonResourceManager, resourceStringProvider, logger, MissingLocalizationBehavior.Ignore)
    {
    }

    public JsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceStringProvider resourceStringProvider,
        ILogger logger,
        MissingLocalizationBehavior missingLocalizationBehavior)
    {
        _jsonResourceManager = jsonResourceManager ?? throw new ArgumentNullException(nameof(jsonResourceManager));
        _resourceStringProvider = resourceStringProvider ?? throw new ArgumentNullException(nameof(resourceStringProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _missingLocalizationBehavior = missingLocalizationBehavior;
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
        _missingLocalizationBehavior = MissingLocalizationBehavior.Ignore;
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

    protected string GetStringSafely(string name, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(name);

        var keyCulture = culture ?? CultureInfo.CurrentUICulture;
        var cacheKey = $"name={name}&culture={keyCulture.Name}";

        _logger.SearchedLocation(name, _jsonResourceManager.ResourcesFilePath, keyCulture);

        if (_missingManifestCache.ContainsKey(cacheKey))
        {
            HandleMissingLocalization(name, keyCulture);
            return null;
        }

        try
        {
            var value = culture == null
                ? _jsonResourceManager.GetString(name)
                : _jsonResourceManager.GetString(name, culture);

            if (value == null)
            {
                _missingManifestCache.TryAdd(cacheKey, null);
                HandleMissingLocalization(name, keyCulture);
            }

            return value;
        }
        catch (MissingManifestResourceException)
        {
            _missingManifestCache.TryAdd(cacheKey, null);
            HandleMissingLocalization(name, keyCulture);
            
            return null;
        }
    }

    private void HandleMissingLocalization(string name, CultureInfo culture)
    {
        switch (_missingLocalizationBehavior)
        {
            case MissingLocalizationBehavior.LogWarning:
                _logger.MissingLocalization(name, _jsonResourceManager.ResourcesFilePath, culture);
                break;
            case MissingLocalizationBehavior.ThrowException:
                throw new MissingLocalizationException(name, culture.Name, _jsonResourceManager.ResourcesFilePath);
            case MissingLocalizationBehavior.Ignore:
            default:
                break;
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

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using My.Extensions.Localization.Json.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace My.Extensions.Localization.Json
{
    using My.Extensions.Localization.Json.Caching;
    using System.Reflection;

    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, object> _missingManifestCache = new ConcurrentDictionary<string, object>();
        private readonly IResourceNamesCache _resourceNamesCache;
        private readonly JsonResourceManager _jsonResourceManager;
        private readonly IResourceStringProvider _resourceStringProvider;
        private readonly string _resourceBaseName;
        private readonly ILogger _logger;

        private string _searchedLocation;

        public JsonStringLocalizer(
            JsonResourceManager jsonResourceManager,
            Assembly resourceAssembly,
            string baseName,
            IResourceNamesCache resourceNamesCache,
            ILogger logger)
            : this(
                  jsonResourceManager,
                new AssemblyWrapper(resourceAssembly),
                baseName,
                resourceNamesCache,
                logger)
        {

        }

        internal JsonStringLocalizer(
             JsonResourceManager jsonResourceManager,
             AssemblyWrapper resourceAssemblyWrapper,
             string baseName,
             IResourceNamesCache resourceNamesCache,
             ILogger logger)
             : this(
                   jsonResourceManager,
                   new JsonStringProvider(
                       resourceNamesCache,
                       jsonResourceManager,
                       resourceAssemblyWrapper.Assembly,
                       baseName),
                   baseName,
                   resourceNamesCache,
                   logger)
        {
        }

        internal JsonStringLocalizer(
            JsonResourceManager jsonResourceManager,
            IResourceStringProvider resourceStringProvider,
            string baseName,
            IResourceNamesCache resourceNamesCache,
            ILogger logger)
        {
            if (jsonResourceManager == null)
            {
                throw new ArgumentNullException(nameof(jsonResourceManager));
            }

            if (resourceStringProvider == null)
            {
                throw new ArgumentNullException(nameof(resourceStringProvider));
            }

            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            if (resourceNamesCache == null)
            {
                throw new ArgumentNullException(nameof(resourceNamesCache));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _resourceStringProvider = resourceStringProvider;
            _jsonResourceManager = jsonResourceManager;
            _resourceBaseName = baseName;
            _resourceNamesCache = resourceNamesCache;
            _logger = logger;
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetStringSafely(name, null);

                return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetStringSafely(name, null);
                var value = string.Format(format ?? name, arguments);

                return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _searchedLocation);
            }
        }

        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        protected virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var resourceNames = includeParentCultures
                ? GetResourceNamesFromCultureHierarchy(culture)
                : _resourceStringProvider.GetAllResourceStrings(culture, true);

            foreach (var name in resourceNames)
            {
                var value = GetStringSafely(name, culture);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

        [Obsolete("This method is deprecated and will be removed in the major release.")]
        protected virtual string GetStringSafely(string name) => GetStringSafely(name, CultureInfo.CurrentUICulture);

        protected string GetStringSafely(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

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
}

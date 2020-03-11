using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using My.Extensions.Localization.Json.Internal;

namespace My.Extensions.Localization.Json
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, IEnumerable<KeyValuePair<string, string>>> _resourcesCache = new ConcurrentDictionary<string, IEnumerable<KeyValuePair<string, string>>>();
        private readonly string _resourcesPath;
        private readonly string _resourceName;
        private readonly ILogger _logger;
        private const string ROOT_RESOURCE_CACHE_NAME = "root";

        private string _searchedLocation;

        public JsonStringLocalizer(
            string resourcesPath,
            string resourceName,
            ILogger logger)
        {
            _resourcesPath = resourcesPath ?? throw new ArgumentNullException(nameof(resourcesPath));
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourcesPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal JsonStringLocalizer(
            string resourcesPath,
            ILogger logger)
        {
            _resourcesPath = resourcesPath ?? throw new ArgumentNullException(nameof(resourcesPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetStringSafely(name);

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

                var format = GetStringSafely(name);
                var value = string.Format(format ?? name, arguments);

                return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _searchedLocation);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        protected IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var resourceNames = includeParentCultures
                ? GetAllStringsFromCultureHierarchy(culture)
                : GetAllResourceStrings(culture);

            foreach (var name in resourceNames)
            {
                var value = GetStringSafely(name);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

        protected string GetStringSafely(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var culture = CultureInfo.CurrentUICulture;
            string value = null;

            while (culture != culture.Parent)
            {
                value = GetStringSafely(name, culture);
                _logger.SearchedLocation(name, _searchedLocation, culture);

                if (value != null)
                {
                    break;
                }

                culture = culture.Parent;
            }

            if (value == null)
            {
                value = GetStringSafely(name, null);
            }

            return value;
        }

        private string GetStringSafely(string name, CultureInfo culture)
        {
            string value = null;
            BuildResourcesCache(culture?.Name);

            if (_resourcesCache.TryGetValue(culture?.Name ?? ROOT_RESOURCE_CACHE_NAME, out IEnumerable<KeyValuePair<string, string>> resources))
            {
                var resource = resources?.SingleOrDefault(s => s.Key == name);

                value = resource?.Value ?? null;
            }

            return value;
        }

        private IEnumerable<string> GetAllStringsFromCultureHierarchy(CultureInfo startingCulture)
        {
            var currentCulture = startingCulture;
            var resourceNames = new HashSet<string>();

            while (currentCulture != currentCulture.Parent)
            {
                var cultureResourceNames = GetAllResourceStrings(currentCulture);

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

        private IEnumerable<string> GetAllResourceStrings(CultureInfo culture)
        {
            BuildResourcesCache(culture.Name);

            if (_resourcesCache.TryGetValue(culture.Name, out IEnumerable<KeyValuePair<string, string>> resources))
            {
                foreach (var resource in resources)
                {
                    yield return resource.Key;
                }
            }
            else
            {
                yield return null;
            }
        }

        private void BuildResourcesCache(string culture = null)
        {
            var cacheName = culture ?? ROOT_RESOURCE_CACHE_NAME;
            _resourcesCache.GetOrAdd(cacheName, _ =>
            {
                var resourceFile = $"{culture}.json";
                if (!string.IsNullOrEmpty(_resourceName))
                {
                    if (culture != null)
                    {
                        resourceFile = $"{_resourceName}.{culture}.json";
                    }
                    else
                    {
                        resourceFile = $"{_resourceName}.json";
                    }
                }

                _searchedLocation = Path.Combine(_resourcesPath, resourceFile);
                IEnumerable<KeyValuePair<string, string>> value = null;

                if (File.Exists(_searchedLocation))
                {
                    var builder = new ConfigurationBuilder()
                    .SetBasePath(_resourcesPath)
                    .AddJsonFile(resourceFile, optional: false, reloadOnChange: true);

                    var config = builder.Build();
                    value = config.AsEnumerable();
                }

                return value;
            });
        }
    }
}

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

        private string _searchedLocation;

        public JsonStringLocalizer(
            string resourcesPath,
            string resourceName,
            ILogger logger)
        {
            _resourcesPath = resourcesPath ?? throw new ArgumentNullException(nameof(resourcesPath));
            _resourceName = resourceName;
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

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected string GetStringSafely(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var culture = CultureInfo.CurrentUICulture;
            var resources = _resourcesCache.GetOrAdd(culture.Name, _ =>
            {
                var resourceFile = (string.IsNullOrEmpty(_resourceName) ? $"{culture.Name}" : $"{_resourceName}.{culture.Name}") + ".json";
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
            var resource = resources?.SingleOrDefault(s => s.Key == name);
            _logger.SearchedLocation(name, _searchedLocation, culture);

            return resource?.Value ?? null;
        }
    }
}

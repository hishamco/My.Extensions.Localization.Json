using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using My.Extensions.Localization.Json;

namespace LocalizationSample
{
    public class CustomJsonStringLocalizer : JsonStringLocalizer
    {
        private readonly string _resourcesPath;
        private readonly string _resourceName;

        public CustomJsonStringLocalizer(
            string resourcesPath,
            string resourceName,
            ILogger logger) : base(resourcesPath, resourceName, logger)
        {
            _resourcesPath = resourcesPath;
            _resourceName = resourceName;
        }

        protected override string GetStringSafely(string name)
        {
            var localizedValue = base.GetStringSafely(name);

            if (localizedValue == null && !string.IsNullOrEmpty(_resourceName))
            {
                var resources = _resourcesCache.GetOrAdd(string.Empty, _ =>
                {
                    var resourceFile = $"{_resourceName}.json";
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

                localizedValue = resource?.Value ?? null;
            }

            return localizedValue;
        }
    }
}

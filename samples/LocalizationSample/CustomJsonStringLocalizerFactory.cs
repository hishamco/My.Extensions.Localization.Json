using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using My.Extensions.Localization.Json;

namespace LocalizationSample
{
    public class CustomJsonStringLocalizerFactory : JsonStringLocalizerFactory
    {
        private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;
        private readonly ILoggerFactory _loggerFactory;

        public CustomJsonStringLocalizerFactory(
            IOptions<JsonLocalizationOptions> localizationOptions,
            ILoggerFactory loggerFactory) : base(localizationOptions, loggerFactory)
        {
            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }

            _resourcesType = localizationOptions.Value.ResourcesType;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected override JsonStringLocalizer CreateJsonStringLocalizer(string resourcesPath, string resourcename)
        {
            var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();

            return new CustomJsonStringLocalizer(
                resourcesPath,
                _resourcesType == ResourcesType.TypeBased ? resourcename : null,
                logger);
        }
    }
}

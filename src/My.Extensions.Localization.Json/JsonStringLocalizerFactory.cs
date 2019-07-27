using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace My.Extensions.Localization.Json
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly string _resourcesRelativePath;
        private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;
        private readonly ILoggerFactory _loggerFactory;

        public JsonStringLocalizerFactory(
            IOptions<JsonLocalizationOptions> localizationOptions,
            ILoggerFactory loggerFactory)
        {
            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }

            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
            _resourcesType = localizationOptions.Value.ResourcesType;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }

            var typeInfo = resourceSource.GetTypeInfo();
            var assembly = typeInfo.Assembly;
            var applicationRootPath = new DirectoryInfo(assembly.Location).Parent.Parent.Parent.Parent.Parent.FullName;
            var resourcesPath = Path.Combine(applicationRootPath, assembly.GetName().Name, GetResourcePath(assembly));

            return CreateJsonStringLocalizer(resourcesPath, typeInfo.Name);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var resourcesPath = Path.Combine(location, _resourcesRelativePath);

            return CreateJsonStringLocalizer(resourcesPath, null);
        }

        protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
            string resourcesPath,
            string resourcename)
        {
            var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();

            return _resourcesType == ResourcesType.TypeBased
                ? new JsonStringLocalizer(
                    resourcesPath,
                    resourcename,
                    logger)
                : new JsonStringLocalizer(
                    resourcesPath,
                    logger);
        }

        private string GetResourcePath(Assembly assembly)
        {
            var resourceLocationAttribute = assembly.GetCustomAttribute<ResourceLocationAttribute>();

            return resourceLocationAttribute == null
                ? _resourcesRelativePath
                : resourceLocationAttribute.ResourceLocation;
        }
    }
}

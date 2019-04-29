using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace My.Extensions.Localization.Json
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly string _resourcesRelativePath;
        private readonly ILoggerFactory _loggerFactory;

        public JsonStringLocalizerFactory(
            IOptions<LocalizationOptions> localizationOptions,
            ILoggerFactory loggerFactory)
        {
            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }

            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
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
            var basePath = assembly.Location.Substring(0, assembly.Location.IndexOf(Assembly.GetEntryAssembly().GetName().Name));
            var filePathList = new List<string>() {
                Path.Combine(basePath, assembly.GetName().Name, GetResourcePath(assembly)),
                Path.Combine(basePath, Assembly.GetEntryAssembly().GetName().Name, GetResourcePath(assembly)),
                Path.Combine(AppContext.BaseDirectory, GetResourcePath(assembly))
            }.Distinct();

            foreach(var i in filePathList)
            {
                if (Directory.Exists(i) && new DirectoryInfo(i).EnumerateFiles().Select(o => o.Name).Where(o => o.StartsWith(typeInfo.Name)).Any())
                {
                    return CreateJsonStringLocalizer(i, typeInfo.Name);
                }
            }

            return CreateJsonStringLocalizer(filePathList.Last(),typeInfo.Name);
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

            return CreateJsonStringLocalizer(location, baseName);
        }

        protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
            string resourcesPath,
            string resourcename)
        {
            return new JsonStringLocalizer(resourcesPath, resourcename, _loggerFactory.CreateLogger<JsonStringLocalizer>());
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

using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using My.Extensions.Localization.Json.Internal;

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

            // TODO: Check why an exception happen before the host build
            if (resourceSource.Name == "Controller")
            {
                return CreateJsonStringLocalizer(Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(resourceSource.Assembly)), TryFixInnerClassPath("Controller"));
            }

            var typeInfo = resourceSource.GetTypeInfo();
            var assembly = typeInfo.Assembly;
            var assemblyName = resourceSource.Assembly.GetName().Name;
            var typeName = $"{assemblyName}.{typeInfo.Name}" == typeInfo.FullName
                ? typeInfo.Name
                : typeInfo.FullName.Substring(assemblyName.Length + 1);
            var resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(assembly));

            typeName = TryFixInnerClassPath(typeName);

            return CreateJsonStringLocalizer(resourcesPath, typeName);
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

            baseName = TryFixInnerClassPath(baseName);

            var assemblyName = new AssemblyName(location);
            var assembly = Assembly.Load(assemblyName);
            var resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(assembly));
            string resourceName = null;

            if (_resourcesType == ResourcesType.TypeBased)
            {
                resourceName = TrimPrefix(baseName, location + ".");
            }

            return CreateJsonStringLocalizer(resourcesPath, resourceName);
        }

        protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
            string resourcesPath,
            string resourcename)
        {
            var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();

            return new JsonStringLocalizer(
                resourcesPath,
                _resourcesType == ResourcesType.TypeBased ? resourcename : null,
                logger);
        }

        private string GetResourcePath(Assembly assembly)
        {
            var resourceLocationAttribute = assembly.GetCustomAttribute<ResourceLocationAttribute>();

            return resourceLocationAttribute == null
                ? _resourcesRelativePath
                : resourceLocationAttribute.ResourceLocation;
        }

        private static string TrimPrefix(string name, string prefix)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }

            return name;
        }

        private string TryFixInnerClassPath(string path)
        {
            const char innerClassSeparator = '+';
            var fixedPath = path;

            if (path.Contains(innerClassSeparator))
            {
                fixedPath = path.Replace(innerClassSeparator, '.');
            }

            return fixedPath;
        }
    }
}
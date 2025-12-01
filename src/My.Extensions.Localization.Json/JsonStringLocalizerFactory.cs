using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using My.Extensions.Localization.Json.Internal;

namespace My.Extensions.Localization.Json;

using My.Extensions.Localization.Json.Caching;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IResourceNamesCache _resourceNamesCache = new ResourceNamesCache();
    private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizerCache = new();
    private readonly string[] _resourcesPaths;
    private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;
    private readonly ILoggerFactory _loggerFactory;

    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> localizationOptions,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(localizationOptions);

        _resourcesPaths = localizationOptions.Value.ResourcesPath ?? Array.Empty<string>();
        _resourcesType = localizationOptions.Value.ResourcesType;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        // TODO: Check why an exception happen before the host build
        if (resourceSource.Name == "Controller")
        {
            var resourcesPaths = GetResourcePaths(resourceSource.Assembly);
            
            return _localizerCache.GetOrAdd(resourceSource.Name, _ => CreateJsonStringLocalizer(resourcesPaths, TryFixInnerClassPath("Controller")));
        }

        var typeInfo = resourceSource.GetTypeInfo();
        var assembly = typeInfo.Assembly;
        var assemblyName = resourceSource.Assembly.GetName().Name;
        var typeName = $"{assemblyName}.{typeInfo.Name}" == typeInfo.FullName
            ? typeInfo.Name
            : TrimPrefix(typeInfo.FullName, assemblyName + ".");

        var paths = GetResourcePaths(assembly);
        typeName = TryFixInnerClassPath(typeName);

        return _localizerCache.GetOrAdd($"culture={CultureInfo.CurrentUICulture.Name}, typeName={typeName}", _ => CreateJsonStringLocalizer(paths, typeName));
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        ArgumentNullException.ThrowIfNull(baseName);
        ArgumentNullException.ThrowIfNull(location);

        return _localizerCache.GetOrAdd($"baseName={baseName},location={location}", _ =>
        {
            var assemblyName = new AssemblyName(location);
            var assembly = Assembly.Load(assemblyName);
            var resourcesPaths = GetResourcePaths(assembly);
            string resourceName = null;
            if (baseName == string.Empty)
            {
                resourceName = baseName;

                return CreateJsonStringLocalizer(resourcesPaths, resourceName);
            }

            if (_resourcesType == ResourcesType.TypeBased)
            {
                baseName = TryFixInnerClassPath(baseName);
                resourceName = TrimPrefix(baseName, location + ".");
            }

            return CreateJsonStringLocalizer(resourcesPaths, resourceName);
        });
    }

    protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
        string[] resourcesPaths,
        string resourceName)
    {
        var resourceManager = _resourcesType == ResourcesType.TypeBased
            ? new JsonResourceManager(resourcesPaths, resourceName)
            : new JsonResourceManager(resourcesPaths);
        var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();

        return new JsonStringLocalizer(resourceManager, _resourceNamesCache, logger);
    }

    private string[] GetResourcePaths(Assembly assembly)
    {
        var resourceLocationAttribute = assembly.GetCustomAttribute<ResourceLocationAttribute>();
        
        if (resourceLocationAttribute != null)
        {
            return new[] { Path.Combine(PathHelpers.GetApplicationRoot(), resourceLocationAttribute.ResourceLocation) };
        }

        return _resourcesPaths
            .Select(p => Path.Combine(PathHelpers.GetApplicationRoot(), p))
            .ToArray();
    }

    private static string TrimPrefix(string name, string prefix)
    {
        if (name.StartsWith(prefix, StringComparison.Ordinal))
        {
            return name[prefix.Length..];
        }

        return name;
    }

    private static string TryFixInnerClassPath(string path)
    {
        const char innerClassSeparator = '+';
        var fixedPath = path;

        if (path.Contains(innerClassSeparator.ToString()))
        {
            fixedPath = path.Replace(innerClassSeparator, '.');
        }

        return fixedPath;
    }
}
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
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
    private readonly string _resourcesRelativePath;
    private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;
    private readonly ILoggerFactory _loggerFactory;

    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> localizationOptions,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(localizationOptions);

        _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
        _resourcesType = localizationOptions.Value.ResourcesType;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Gets the cache used to store resource names.
    /// </summary>
    protected IResourceNamesCache ResourceNamesCache => _resourceNamesCache;

    /// <summary>
    /// Gets the cache used to store localizer instances.
    /// </summary>
    protected ConcurrentDictionary<string, JsonStringLocalizer> LocalizerCache => _localizerCache;

    /// <summary>
    /// Gets the resources relative path.
    /// </summary>
    protected string ResourcesRelativePath => _resourcesRelativePath;

    /// <summary>
    /// Gets the resources type.
    /// </summary>
    protected ResourcesType ResourcesType => _resourcesType;

    /// <summary>
    /// Gets the logger factory.
    /// </summary>
    protected ILoggerFactory LoggerFactory => _loggerFactory;

    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        string resourcesPath = string.Empty;

        // TODO: Check why an exception happen before the host build
        if (resourceSource.Name == "Controller")
        {
            resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(resourceSource.Assembly));
            
            return _localizerCache.GetOrAdd(resourceSource.Name, _ => CreateJsonStringLocalizer(resourcesPath, TryFixInnerClassPath("Controller")));
        }

        var typeInfo = resourceSource.GetTypeInfo();
        var assembly = typeInfo.Assembly;
        var assemblyName = resourceSource.Assembly.GetName().Name;
        var typeName = $"{assemblyName}.{typeInfo.Name}" == typeInfo.FullName
            ? typeInfo.Name
            : TrimPrefix(typeInfo.FullName, assemblyName + ".");

        resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(assembly));
        typeName = TryFixInnerClassPath(typeName);

        return _localizerCache.GetOrAdd($"culture={CultureInfo.CurrentUICulture.Name}, typeName={typeName}", _ => CreateJsonStringLocalizer(resourcesPath, typeName));
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        ArgumentNullException.ThrowIfNull(baseName);
        ArgumentNullException.ThrowIfNull(location);

        return _localizerCache.GetOrAdd($"baseName={baseName},location={location}", _ =>
        {
            var assemblyName = new AssemblyName(location);
            var assembly = Assembly.Load(assemblyName);
            var resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(assembly));
            string resourceName = null;
            if (baseName == string.Empty)
            {
                resourceName = baseName;

                return CreateJsonStringLocalizer(resourcesPath, resourceName);
            }

            if (_resourcesType == ResourcesType.TypeBased)
            {
                baseName = TryFixInnerClassPath(baseName);
                resourceName = TrimPrefix(baseName, location + ".");
            }

            return CreateJsonStringLocalizer(resourcesPath, resourceName);
        });
    }

    protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
        string resourcesPath,
        string resourceName)
    {
        var resourceManager = _resourcesType == ResourcesType.TypeBased
            ? new JsonResourceManager(resourcesPath, resourceName)
            : new JsonResourceManager(resourcesPath);
        var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();

        return new JsonStringLocalizer(resourceManager, _resourceNamesCache, logger);
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
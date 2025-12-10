using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
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
    private readonly bool _fallBackToParentUICultures = true;
    private readonly ILoggerFactory _loggerFactory;

    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> localizationOptions,
        ILoggerFactory loggerFactory)
        : this(localizationOptions, loggerFactory, null)
    {
    }

    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> localizationOptions,
        ILoggerFactory loggerFactory,
        IOptions<RequestLocalizationOptions> requestLocalizationOptions)
    {
        ArgumentNullException.ThrowIfNull(localizationOptions);

        _resourcesPaths = localizationOptions.Value.ResourcesPath ?? [];
        _resourcesType = localizationOptions.Value.ResourcesType;
        _fallBackToParentUICultures = requestLocalizationOptions?.Value?.FallBackToParentUICultures ?? true;
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

        // TODO: Check why an exception happen before the host build
        if (resourceSource.Name == "Controller")
        {
            var resourcesPaths = GetResourcePaths(resourceSource.Assembly);
            
            return _localizerCache.GetOrAdd(resourceSource.Name, _ => CreateJsonStringLocalizer(resourcesPaths, TryFixInnerClassPath("Controller")));
        }

        var typeInfo = resourceSource.GetTypeInfo();
        var assembly = typeInfo.Assembly;
        var rootNamespace = GetRootNamespace(assembly);
        var typeName = $"{rootNamespace}.{typeInfo.Name}" == typeInfo.FullName
            ? typeInfo.Name
            : TrimPrefix(typeInfo.FullName, rootNamespace + ".");

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
                var rootNamespace = GetRootNamespace(assembly);
                resourceName = TrimPrefix(baseName, rootNamespace + ".");
            }

            return CreateJsonStringLocalizer(resourcesPaths, resourceName);
        });
    }

    protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
        string[] resourcesPaths,
        string resourceName)
    {
        var resourceManager = _resourcesType == ResourcesType.TypeBased
            ? new JsonResourceManager(resourcesPaths, _fallBackToParentUICultures, resourceName)
            : new JsonResourceManager(resourcesPaths, _fallBackToParentUICultures, null);
        var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();

        return new JsonStringLocalizer(resourceManager, _resourceNamesCache, logger);
    }

    private string[] GetResourcePaths(Assembly assembly)
    {
        var resourceLocationAttribute = assembly.GetCustomAttribute<ResourceLocationAttribute>();
        
        if (resourceLocationAttribute != null)
        {
            return [Path.Combine(PathHelpers.GetApplicationRoot(), resourceLocationAttribute.ResourceLocation)];
        }

        return [.. _resourcesPaths.Select(p => Path.Combine(PathHelpers.GetApplicationRoot(), p))];
    }

    private static string GetRootNamespace(Assembly assembly)
    {
        var rootNamespaceAttribute = assembly.GetCustomAttribute<RootNamespaceAttribute>();

        return rootNamespaceAttribute?.RootNamespace
            ?? assembly.GetName().Name;
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
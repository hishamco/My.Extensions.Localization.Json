using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private readonly string _resourcesRelativePath;
    private readonly IList<string> _additionalResourcesPaths;
    private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;
    private readonly ILoggerFactory _loggerFactory;

    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> localizationOptions,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(localizationOptions);

        _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
        _additionalResourcesPaths = localizationOptions.Value.AdditionalResourcesPaths ?? new List<string>();
        _resourcesType = localizationOptions.Value.ResourcesType;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

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
        var rootNamespace = GetRootNamespace(assembly);
        var typeName = $"{rootNamespace}.{typeInfo.Name}" == typeInfo.FullName
            ? typeInfo.Name
            : TrimPrefix(typeInfo.FullName, rootNamespace + ".");

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
                var rootNamespace = GetRootNamespace(assembly);
                resourceName = TrimPrefix(baseName, rootNamespace + ".");
            }

            return CreateJsonStringLocalizer(resourcesPath, resourceName);
        });
    }

    protected virtual JsonStringLocalizer CreateJsonStringLocalizer(
        string resourcesPath,
        string resourceName)
    {
        var additionalResourcesPaths = _additionalResourcesPaths
            .Select(p => Path.Combine(PathHelpers.GetApplicationRoot(), p))
            .ToArray();
        
        var resourceManager = _resourcesType == ResourcesType.TypeBased
            ? new JsonResourceManager(resourcesPath, resourceName, additionalResourcesPaths)
            : new JsonResourceManager(resourcesPath, additionalResourcesPaths);
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
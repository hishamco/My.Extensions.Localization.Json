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

/// <summary>
/// Provides an implementation of <see cref="IStringLocalizerFactory"/> that loads localized strings from JSON resource
/// files. Enables localization support for applications using JSON-based resources.
/// </summary>
public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IResourceNamesCache _resourceNamesCache = new ResourceNamesCache();
    private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizerCache = new();
    private readonly string[] _resourcesPaths;
    private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;
    private readonly bool _fallBackToParentUICultures = true;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the JsonStringLocalizerFactory class using the specified localization options and
    /// logger factory.
    /// </summary>
    /// <param name="localizationOptions">The options used to configure JSON-based localization behavior.</param>
    /// <param name="loggerFactory">The factory used to create logger instances for logging localization events and errors.</param>
    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> localizationOptions,
        ILoggerFactory loggerFactory)
        : this(localizationOptions, loggerFactory, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the JsonStringLocalizerFactory class using the specified localization and logging
    /// options.
    /// </summary>
    /// <param name="localizationOptions">The localization options that configure resource paths and resource type for JSON-based localization. Cannot be
    /// null.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for localization operations. Cannot be null.</param>
    /// <param name="requestLocalizationOptions">The request localization options that determine culture fallback behavior. May be null; if null, fallback to
    /// parent UI cultures is enabled by default.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="localizationOptions"/> or <paramref name="loggerFactory"/> is null.</exception>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <summary>
    /// Creates a new instance of the <see cref="JsonStringLocalizer"/> using the specified resource paths and resource
    /// name.
    /// </summary>
    /// <param name="resourcesPaths">An array of file system paths that specify the locations of JSON resource files to be used for localization.</param>
    /// <param name="resourceName">The name of the resource to be localized. If <paramref name="resourceName"/> is null, localization will be based
    /// on the provided resource paths only.</param>
    /// <returns>A <see cref="JsonStringLocalizer"/> configured to provide localized strings from the specified resources.</returns>
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
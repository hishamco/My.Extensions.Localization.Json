using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using My.Extensions.Localization.Json;
using My.Extensions.Localization.Json.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering JSON-based localization services with an <see
/// cref="IServiceCollection"/>.
/// </summary>
public static class JsonLocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds JSON-based localization services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the JSON localization services will be added. Cannot be null.</param>
    /// <returns>The same service collection instance, with JSON localization services registered.</returns>
    public static IServiceCollection AddJsonLocalization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();

        AddJsonLocalizationServices(services);

        return services;
    }

    /// <summary>
    /// Adds JSON-based localization services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the JSON localization services will be added. Cannot be null.</param>
    /// <param name="setupAction">An action to configure the JSON localization options. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> with JSON localization services registered.</returns>
    public static IServiceCollection AddJsonLocalization(this IServiceCollection services, Action<JsonLocalizationOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(setupAction);

        AddJsonLocalizationServices(services, setupAction);

        return services;
    }

    internal static void AddJsonLocalizationServices(IServiceCollection services)
    {
        services.TryAddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        services.TryAddTransient(typeof(IStringLocalizer), typeof(StringLocalizer));
    }

    internal static void AddJsonLocalizationServices(IServiceCollection services, Action<JsonLocalizationOptions> setupAction)
    {
        AddJsonLocalizationServices(services);
        services.Configure(setupAction);
    }
}
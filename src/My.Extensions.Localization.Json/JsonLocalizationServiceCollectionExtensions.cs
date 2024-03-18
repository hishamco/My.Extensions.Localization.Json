using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using My.Extensions.Localization.Json;
using My.Extensions.Localization.Json.Internal;

namespace Microsoft.Extensions.DependencyInjection;

public static class JsonLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddJsonLocalization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();

        AddJsonLocalizationServices(services);

        return services;
    }

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
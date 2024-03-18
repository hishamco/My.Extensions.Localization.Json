using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    internal static int Count<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => Count(services, typeof(TService), typeof(TImplementation));

    internal static int Count(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        ArgumentNullException.ThrowIfNull(implementationType);

        var matches = services
            .Where(sd => sd.ServiceType == serviceType && sd.ImplementationType == implementationType)
            .ToArray();

        return matches.Length;
    }
}
using System.Collections.Generic;
using System.Globalization;

namespace My.Extensions.Localization.Json.Internal;

/// <summary>
/// Defines a provider for retrieving all resource strings for a specified culture.
/// </summary>
public interface IResourceStringProvider
{
    /// <summary>
    /// Retrieves all resource strings for the specified culture.
    /// </summary>
    /// <param name="culture">The culture for which to retrieve resource strings. Cannot be null.</param>
    /// <param name="throwOnMissing">Specifies whether to throw an exception if resource strings for the specified culture are missing. If <see
    /// langword="true"/>, an exception is thrown when resources are not found; otherwise, an empty list is returned.</param>
    /// <returns>A list of resource strings associated with the specified culture. The list is empty if no resources are found
    /// and <paramref name="throwOnMissing"/> is <see langword="false"/>.</returns>
    IList<string> GetAllResourceStrings(CultureInfo culture, bool throwOnMissing);
}

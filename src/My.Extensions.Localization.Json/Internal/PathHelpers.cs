using System.IO;
using System.Reflection;

namespace My.Extensions.Localization.Json.Internal;

/// <summary>
/// Provides helper methods for working with file system paths related to the application's location.
/// </summary>
public static class PathHelpers
{
    /// <summary>
    /// Gets the root directory of the currently executing application.
    /// </summary>
    /// <returns>A string containing the full path to the application's root directory. Returns null if the directory cannot be
    /// determined.</returns>
    public static string GetApplicationRoot() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
}
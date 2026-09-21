namespace My.Extensions.Localization.Json;

/// <summary>
/// Specifies the behavior when a localization resource is not found.
/// </summary>
public enum MissingLocalizationBehavior
{
    /// <summary>
    /// Ignores the missing localization and uses the key as the value.
    /// This is the default behavior.
    /// </summary>
    Ignore,

    /// <summary>
    /// Logs a warning when a localization is not found.
    /// </summary>
    LogWarning,

    /// <summary>
    /// Throws a <see cref="MissingLocalizationException"/> when a localization is not found.
    /// </summary>
    ThrowException
}

namespace My.Extensions.Localization.Json;

/// <summary>
/// Specifies the strategy used to locate and manage application resources, such as strings or images, for localization
/// or configuration purposes.
/// </summary>
public enum ResourcesType
{
    /// <summary>
    /// Specifies that the operation or value is determined based on cultural or locale-specific rules.
    /// </summary>
    CultureBased,
    /// <summary>
    /// Represents an object or operation that is determined or influenced by its type information.
    /// </summary>
    TypeBased
}
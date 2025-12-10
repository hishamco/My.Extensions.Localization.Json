using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json;

/// <summary>
/// Provides configuration options for JSON-based localization, including resource type and resource path settings.
/// </summary>
public class JsonLocalizationOptions : LocalizationOptions
{
    /// <summary>
    /// Gets or sets the strategy used to determine how resources are categorized or accessed.
    /// </summary>
    public ResourcesType ResourcesType { get; set; } = ResourcesType.TypeBased;

    /// <summary>
    /// Gets or sets the collection of file system paths to resource directories used by the application.
    /// </summary>
    public new string[] ResourcesPath { get; set; } = [];
}
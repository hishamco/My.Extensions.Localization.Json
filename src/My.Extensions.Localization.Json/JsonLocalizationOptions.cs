using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json;

public class JsonLocalizationOptions : LocalizationOptions
{
    public ResourcesType ResourcesType { get; set; } = ResourcesType.TypeBased;

    /// <summary>
    /// Gets or sets a value indicating whether to fall back to parent UI cultures
    /// when a localized string is not found for the current culture.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c> to maintain backward compatibility.
    /// </remarks>
    public bool FallBackToParentUICultures { get; set; } = true;
}
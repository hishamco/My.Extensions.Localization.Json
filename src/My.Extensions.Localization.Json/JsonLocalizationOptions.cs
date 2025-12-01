using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json;

public class JsonLocalizationOptions : LocalizationOptions
{
    public ResourcesType ResourcesType { get; set; } = ResourcesType.TypeBased;

    /// <summary>
    /// Gets or sets a value indicating whether to use the full generic type name for resource files.
    /// When set to <c>true</c> (default), the full generic type name (e.g., "GenericClass`1[[...]]") is used.
    /// When set to <c>false</c>, the generic type markers are stripped (e.g., "GenericClass"), making it easier to name resource files.
    /// </summary>
    public bool UseGenericResources { get; set; } = true;
}
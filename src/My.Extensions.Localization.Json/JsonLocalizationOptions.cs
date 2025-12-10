using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json;

public class JsonLocalizationOptions : LocalizationOptions
{
    /// <summary>
    /// Gets or sets the behavior when a localization resource is not found.
    /// The default is <see cref="MissingLocalizationBehavior.Ignore"/>.
    /// </summary>
    public MissingLocalizationBehavior MissingLocalizationBehavior { get; set; } = MissingLocalizationBehavior.Ignore;

    public ResourcesType ResourcesType { get; set; } = ResourcesType.TypeBased;

    public new string[] ResourcesPath { get; set; } = [];
}
using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json;

public class JsonLocalizationOptions : LocalizationOptions
{
    public ResourcesType ResourcesType { get; set; } = ResourcesType.TypeBased;

    public IList<string> AdditionalResourcesPaths { get; set; } = new List<string>();
}
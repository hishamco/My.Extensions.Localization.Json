using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json
{
    public class JsonLocalizationOptions : LocalizationOptions
    {
        public ResourcesType ResourcesType { get; set; } = ResourcesType.TypeBased;
    }
}
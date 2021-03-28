using Microsoft.Extensions.Localization;
using System.Reflection;

namespace My.Extensions.Localization.Json
{
    public class JsonLocalizationOptions : LocalizationOptions
    {
        public Assembly ResourceAssembly { get; set; }
    }
}
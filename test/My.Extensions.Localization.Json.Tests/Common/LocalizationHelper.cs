using System.Globalization;

namespace My.Extensions.Localization.Json.Tests.Common
{
    public static class LocalizationHelper
    {
        public static void SetCurrentCulture(string culture)
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
        }
    }
}
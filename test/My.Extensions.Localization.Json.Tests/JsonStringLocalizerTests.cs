using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests
{
    public class JsonStringLocalizerTests
    {
        private readonly IStringLocalizer _localizer;
        private readonly Mock<IOptions<LocalizationOptions>> _localizationOptions;
        private readonly ILogger _logger;
        private static readonly string _resourcePath = @"C:\Users\Hisham\Source\Repos\My.Extensions.Localization.Json\test\My.Extensions.Localization.Json.Tests\Resources";

        public JsonStringLocalizerTests()
        {
            _localizationOptions = new Mock<IOptions<LocalizationOptions>>();
            _localizationOptions.Setup(o => o.Value)
                .Returns(() => new LocalizationOptions { ResourcesPath = "Resources" });
            _logger = NullLogger.Instance;
            _localizer = new JsonStringLocalizer(_resourcePath, nameof(Test), _logger);
        }

        [Theory]
        [InlineData("fr-FR", "Hello", "Bonjour")]
        [InlineData("fr-FR", "Bye", "Bye")]
        [InlineData("ar", "Hello", "مرحبا")]
        [InlineData("ar", "Bye", "Bye")]
        public void GetTranslation(string culture, string name, string expected)
        {
            // Arrange
            string translation = null;

            // Act
            LocalizationHelper.SetCurrentCulture(culture);
            translation = _localizer[name];

            // Assert
            Assert.Equal(expected, translation);
        }

        [InlineData("fr-FR", "Hello {0}", "Bonjour Hisham", "Hisham")]
        [InlineData("fr-FR", "Bye {0}", "Bye Hisham", "Hisham")]
        [InlineData("ar", "Hello {0}", "مرحبا هشام", "هشام")]
        [InlineData("ar", "Bye {0}", "Bye هشام", "هشام")]
        public void GetTranslationWithArgs(string culture, string name, string expected, string arg)
        {
            // Arrange
            string translation = null;

            // Act
            LocalizationHelper.SetCurrentCulture(culture);
            translation = _localizer[name, arg];

            // Assert
            Assert.Equal(expected, translation);
        }
    }
}
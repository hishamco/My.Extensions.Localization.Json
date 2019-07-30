using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests
{
    public class JsonStringLocalizerFactoryTests
    {
        private readonly Mock<IOptions<JsonLocalizationOptions>> _localizationOptions;
        private readonly ILoggerFactory _loggerFactory;

        public JsonStringLocalizerFactoryTests()
        {
            _localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
            _loggerFactory = NullLoggerFactory.Instance;
        }

        [Fact]
        public void JsonStringLocalizerFactory_CreateLocalizerWithType()
        {
            SetupLocalizationOptions("Resources");
            LocalizationHelper.SetCurrentCulture("fr-FR");

            // Arrange
            var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, _loggerFactory);

            // Act
            var localizer = localizerFactory.Create(typeof(Test));

            // Assert
            Assert.NotNull(localizer);
            Assert.Equal("Bonjour", localizer["Hello"]);
        }

        [Theory]
        [InlineData(ResourcesType.TypeBased)]
        [InlineData(ResourcesType.CultureBased)]
        public void CreateLocalizerWithBasenameAndLocation(ResourcesType resourcesType)
        {
            SetupLocalizationOptions("Resources", resourcesType);
            LocalizationHelper.SetCurrentCulture("fr-FR");

            // Arrange
            var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, _loggerFactory);
            var location = "My.Extensions.Localization.Json.Tests";
            var basename = $"{location}.{nameof(Test)}";

            // Act
            var localizer = localizerFactory.Create(basename, location);

            // Assert
            Assert.NotNull(localizer);
            Assert.Equal("Bonjour", localizer["Hello"]);
        }

        private void SetupLocalizationOptions(string resourcesPath, ResourcesType resourcesType = ResourcesType.TypeBased)
            => _localizationOptions.Setup(o => o.Value)
                .Returns(() => new JsonLocalizationOptions {
                    ResourcesPath = resourcesPath,
                    ResourcesType = resourcesType
                });
    }
}
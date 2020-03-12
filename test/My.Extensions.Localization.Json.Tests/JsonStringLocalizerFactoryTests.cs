using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
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
        private readonly Mock<IOptions<RequestLocalizationOptions>> _requestLocalizationOptions;
        private readonly ILoggerFactory _loggerFactory;

        public JsonStringLocalizerFactoryTests()
        {
            _localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
            _requestLocalizationOptions = new Mock<IOptions<RequestLocalizationOptions>>();
            _loggerFactory = NullLoggerFactory.Instance;
        }

        [Fact]
        public void JsonStringLocalizerFactory_CreateLocalizerWithType()
        {
            var culture = "fr-FR";
            SetupLocalizationOptions("Resources");
            SetupRequestLocalizationOptions(culture);
            LocalizationHelper.SetCurrentCulture(culture);

            // Arrange
            var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, _requestLocalizationOptions.Object, _loggerFactory);

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
            var culture = "fr-FR";
            SetupLocalizationOptions("Resources", resourcesType);
            SetupRequestLocalizationOptions(culture);
            LocalizationHelper.SetCurrentCulture(culture);

            // Arrange
            var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, _requestLocalizationOptions.Object, _loggerFactory);
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

        private void SetupRequestLocalizationOptions(string culture)
            => _requestLocalizationOptions.Setup(o => o.Value)
                .Returns(() => new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(culture)
                });
    }
}
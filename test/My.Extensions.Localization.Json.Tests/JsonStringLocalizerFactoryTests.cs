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
            _localizationOptions.Setup(o => o.Value)
                .Returns(() => new JsonLocalizationOptions { ResourcesPath = "Resources" });
            _loggerFactory = NullLoggerFactory.Instance;
        }

        [Fact]
        public void JsonStringLocalizerFactory_CreateLocalizerWithType()
        {
            // Arrange
            var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, _loggerFactory);

            // Act
            LocalizationHelper.SetCurrentCulture("fr-FR");
            var localizer = localizerFactory.Create(typeof(Test));

            // Assert
            Assert.NotNull(localizer);
        }

        // Uncomment this test when this https://github.com/hishamco/My.Extensions.Localization.Json/issues/14 bug is fixed

        //[Fact]
        //public void CreateLocalizerWithBasenameAndLocation()
        //{
        //    // Arrange
        //    var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, _loggerFactory);
        //    var basename = nameof(JsonStringLocalizerFactoryTests);
        //    var location = Path.Combine(nameof(My.Extensions.Localization.Json.Tests), nameof(Test));

        //    // Act
        //    LocalizationHelper.SetCurrentCulture("fr-FR");
        //    var localizer = localizerFactory.Create(basename, location);

        //    // Assert
        //    Assert.NotNull(localizer);
        //}
    }
}
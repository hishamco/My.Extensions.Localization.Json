using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Xunit;

namespace My.Extensions.Localization.Json.Tests
{
    public class JsonLocalizationServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddJsonLocalization()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            JsonLocalizationServiceCollectionExtensions.AddJsonLocalization(services);

            // Assert
            Assert.Equal(1, services.Count<IStringLocalizerFactory, JsonStringLocalizerFactory>());
            Assert.Equal(1, services.Count(typeof(IStringLocalizer<>), typeof(StringLocalizer<>)));
        }

        [Fact]
        public void AddJsonLocalizationWithOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var localizationOptions = new JsonLocalizationOptions();

            // Act
            JsonLocalizationServiceCollectionExtensions.AddJsonLocalization(services,
                options => options.ResourcesPath = "Resources");

            var localizationConfigureOptions = (ConfigureNamedOptions<JsonLocalizationOptions>)services
                .SingleOrDefault(sd => sd.ServiceType == typeof(IConfigureOptions<JsonLocalizationOptions>))
                ?.ImplementationInstance;

            // Assert
            Assert.Equal(1, services.Count(typeof(IStringLocalizerFactory), typeof(JsonStringLocalizerFactory)));
            Assert.Equal(1, services.Count(typeof(IStringLocalizer<>), typeof(StringLocalizer<>)));
            Assert.NotNull(localizationConfigureOptions);

            localizationConfigureOptions.Action.Invoke(localizationOptions);

            Assert.Equal("Resources", localizationOptions.ResourcesPath);
        }
    }
}
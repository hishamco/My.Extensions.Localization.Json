using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Internal;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests
{
    public class JsonStringLocalizerTests
    {
        private readonly IStringLocalizer _localizer;
        private readonly Mock<IOptions<LocalizationOptions>> _localizationOptions;
        private readonly ILogger _logger;

        public JsonStringLocalizerTests()
        {
            _localizationOptions = new Mock<IOptions<LocalizationOptions>>();
            _localizationOptions.Setup(o => o.Value)
                .Returns(() => new LocalizationOptions { ResourcesPath = "Resources" });
            _logger = NullLogger.Instance;

            var resourcePath = Path.Combine(PathHelpers.GetApplicationRoot(), _localizationOptions.Object.Value.ResourcesPath);
            _localizer = new JsonStringLocalizer(resourcePath, nameof(Test), _logger);
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


        [Theory]
        [InlineData(true, 3)]
        [InlineData(false, 2)]
        public void JsonStringLocalizer_GetAllStrings(bool includeParent, int expected)
        {
            // Arrange
            IEnumerable<LocalizedString> localizedStrings = null;

            // Act
            LocalizationHelper.SetCurrentCulture("fr-FR");
            localizedStrings = _localizer.GetAllStrings(includeParent);

            // Assert
            Assert.Equal(expected, localizedStrings.Count());
        }

        [Fact]
        public async void CultureBasedResourcesUsesUsesIStringLocalizer()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddJsonLocalization(options =>
                    {
                        options.ResourcesPath = "Resources";
                        options.ResourcesType = ResourcesType.CultureBased;
                    });
                })
                .Configure(app =>
                {
                    app.UseRequestLocalization("en-US", "fr-FR");

                    app.Run(context =>
                    {
                        var localizer = context.RequestServices.GetService<IStringLocalizer<JsonStringLocalizer>>();

                        LocalizationHelper.SetCurrentCulture("fr-FR");

                        Assert.Equal("Bonjour", localizer["Hello"]);

                        return Task.FromResult(0);
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync("/");
            }
        }
    }
}
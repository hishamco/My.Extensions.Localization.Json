using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
            var basename = $"{location}.Common.{nameof(Test)}";

            // Act
            var localizer = localizerFactory.Create(basename, location);

            // Assert
            Assert.NotNull(localizer);
            Assert.Equal("Bonjour", localizer["Hello"]);
        }

        [Fact]
        public async Task LocalizerReturnsTranslationFromInnerClass()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
                })
                .Configure(app =>
                {
                    app.UseRequestLocalization("en", "ar");

                    app.Run(context =>
                    {
                        var localizer = context.RequestServices.GetService<IStringLocalizer<Model>>();

                        LocalizationHelper.SetCurrentCulture("ar");

                        Assert.Equal("مرحباً", localizer["Hello"]);

                        return Task.FromResult(0);
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync("/");
            }
        }

        private void SetupLocalizationOptions(string resourcesPath, ResourcesType resourcesType = ResourcesType.TypeBased)
            => _localizationOptions.Setup(o => o.Value)
                .Returns(() => new JsonLocalizationOptions {
                    ResourcesPath = resourcesPath
                });

        public class InnerClassStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddMvc();
                services.AddLocalization();
                services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
            }

            public void Configure(IApplicationBuilder app, IStringLocalizer<Model> localizer)
            {
                var supportedCultures = new[] { "ar", "en" };
                app.UseRequestLocalization(options =>
                    options
                        .AddSupportedCultures(supportedCultures)
                        .AddSupportedUICultures(supportedCultures)
                        .SetDefaultCulture("ar")
                );

                app.Run(async (context) =>
                {
                    var loc = localizer["Hello"];
                    await context.Response.WriteAsync(localizer["Hello"]);
                });
            }
        }

        public class Model
        {
            public string Hello { get; set; }
        }
    }
}
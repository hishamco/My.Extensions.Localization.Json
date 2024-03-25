using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests;

public class JsonStringLocalizerTests
{
    private readonly IStringLocalizer _localizer;

    public JsonStringLocalizerTests()
    {
        var _localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        _localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions { ResourcesPath = "Resources" });
        var localizerFactory = new JsonStringLocalizerFactory(_localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        _localizer = localizerFactory.Create(basename, location);
    }

    [Theory]
    [InlineData("fr-FR", "Hello", "Bonjour")]
    [InlineData("fr-FR", "Bye", "Bye")]
    [InlineData("ar", "Hello", "مرحبا")]
    [InlineData("ar", "Bye", "Bye")]
    public void GetTranslation(string culture, string name, string expected)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);
        
        // Act
        string translation = _localizer[name];

        // Assert
        Assert.Equal(expected, translation);
    }

    [Theory]
    [InlineData("fr-FR", "Hello, {0}", "Bonjour, Hisham", "Hisham")]
    [InlineData("fr-FR", "Bye {0}", "Bye Hisham", "Hisham")]
    [InlineData("ar", "Hello, {0}", "مرحبا, هشام", "هشام")]
    [InlineData("ar", "Bye {0}", "Bye هشام", "هشام")]
    public void GetTranslationWithArgs(string culture, string name, string expected, string arg)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);

        // Act
        string translation = _localizer[name, arg];

        // Assert
        Assert.Equal(expected, translation);
    }

    [Theory]
    [InlineData("fr-FR", "Hello", "Bonjour")]
    [InlineData("fr-FR", "Hello, {0}", "Bonjour, {0}")]
    [InlineData("fr-FR", "Yes", "Oui")]
    [InlineData("fr-FR", "No", "No")]
    [InlineData("fr", "Hello", "Bonjour")]
    [InlineData("fr", "Yes", "Oui")]
    [InlineData("fr", "No", "No")]
    public void GetTranslationWithCultureFallback(string culture, string name, string expected)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);
        
        // Act
        string translation = _localizer[name];

        // Assert
        Assert.Equal(expected, translation);
    }


    [Theory]
    [InlineData("zh-CN", "Hello", "你好")]
    [InlineData("zh-CN", "Hello, {0}", "你好，{0}")]
    public void GetTranslationWithCommentsOrCommas(string culture, string name, string expected)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);

        // Act
        string translation = _localizer[name];

        // Assert
        Assert.Equal(expected, translation);
    }

    [Fact]
    public void GetTranslation_StronglyTypeResourceName()
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture("ar");

        // Act
        var translation = _localizer.GetString<SharedResource>(r => r.Hello);

        // Assert
        Assert.Equal("مرحبا", translation);
    }

    [Theory]
    [InlineData(true, 9)]
    [InlineData(false, 8)]
    public void JsonStringLocalizer_GetAllStrings(bool includeParent, int expected)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture("fr-FR");
        
        // Act
        var localizedStrings = _localizer.GetAllStrings(includeParent);

        // Assert
        Assert.Equal(expected, localizedStrings.Count());
    }

    [Fact]
    public async void CultureBasedResourcesUsesIStringLocalizer()
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

                app.Run(async context =>
                {
                    var localizer = context.RequestServices.GetService<IStringLocalizer<JsonStringLocalizer>>();

                    LocalizationHelper.SetCurrentCulture("fr-FR");

                    Assert.Equal("Bonjour", localizer["Hello"]);

                    await Task.CompletedTask;
                });
            });

        using var server = new TestServer(webHostBuilder);
        var client = server.CreateClient();
        var response = await client.GetAsync("/");
    }

    [Theory]
    [InlineData("fr-FR", "Book.Page.One", "Page Un")]
    [InlineData("fr-FR", "Book.Page.Two", "Page Deux")]
    [InlineData("fr-FR", "Articles[0].Content", "Contenu 1")]
    [InlineData("fr-FR", "Articles[1].Content", "Contenu 2")]
    public void GetTranslationUsingKeyHeirarchy(string culture, string name, string expected)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);

        // Act
        string translation = _localizer[name];

        // Assert
        Assert.Equal(expected, translation);
    }

    private class SharedResource
    {
        public string Hello { get; set; }

        public string Bye { get; set; }
    }
}
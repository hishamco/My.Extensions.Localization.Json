using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace My.Extensions.Localization.Json.Tests;

public class JsonStringLocalizerTests
{
    private readonly IStringLocalizer _localizer;

    public JsonStringLocalizerTests()
    {
        var _localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        _localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions { ResourcesPath = ["Resources"] });
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
    public async Task CultureBasedResourcesUsesIStringLocalizer()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddJsonLocalization(options =>
        {
            options.ResourcesPath = ["Resources"];
            options.ResourcesType = ResourcesType.CultureBased;
        });

        builder.WebHost.UseTestServer();

        await using var app = builder.Build();
        
        app.UseRequestLocalization("en-US", "fr-FR");

        app.MapGet("/", async (IStringLocalizer<JsonStringLocalizer> localizer) =>
        {
            LocalizationHelper.SetCurrentCulture("fr-FR");
            Assert.Equal("Bonjour", localizer["Hello"]);
            return "OK";
        });

        await app.StartAsync();

        var client = app.GetTestClient();
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

    [Fact]
    public void GetTranslation_WithFallBackToParentUICulturesDisabled_DoesNotFallbackToParentCulture()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions 
            { 
                ResourcesPath = ["Resources"]
            });
        
        var requestLocalizationOptions = new Mock<IOptions<RequestLocalizationOptions>>();
        requestLocalizationOptions.Setup(o => o.Value)
            .Returns(() => new RequestLocalizationOptions 
            { 
                FallBackToParentUICultures = false
            });
        
        var localizerFactory = new JsonStringLocalizerFactory(
            localizationOptions.Object, 
            NullLoggerFactory.Instance,
            requestLocalizationOptions.Object);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);
        
        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        // "Yes" exists only in Test.fr.json (parent culture), not in Test.fr-FR.json
        var translation = localizer["Yes"];

        // Assert
        // When FallBackToParentUICultures is disabled, it should not find "Yes" in fr-FR
        // and return the key itself as the translation was not found
        Assert.Equal("Yes", translation.Value);
        Assert.True(translation.ResourceNotFound);
    }

    [Fact]
    public void GetTranslation_WithFallBackToParentUICulturesEnabled_FallsBackToParentCulture()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions 
            { 
                ResourcesPath = ["Resources"]
            });
        
        var requestLocalizationOptions = new Mock<IOptions<RequestLocalizationOptions>>();
        requestLocalizationOptions.Setup(o => o.Value)
            .Returns(() => new RequestLocalizationOptions 
            { 
                FallBackToParentUICultures = true
            });
        
        var localizerFactory = new JsonStringLocalizerFactory(
            localizationOptions.Object, 
            NullLoggerFactory.Instance,
            requestLocalizationOptions.Object);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);
        
        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        // "Yes" exists only in Test.fr.json (parent culture), not in Test.fr-FR.json
        var translation = localizer["Yes"];

        // Assert
        // When FallBackToParentUICultures is enabled (default), it should find "Yes" in fr (parent)
        Assert.Equal("Oui", translation.Value);
        Assert.False(translation.ResourceNotFound);
    }
  
    [Theory]
    [InlineData("fr-FR", "Yes", "Oui")]  // "Yes" only exists in fr.json, should fallback from fr-FR to fr
    [InlineData("fr-FR", "Hello", "Bonjour")]  // "Hello" exists in both, fr-FR should take precedence
    public void GetTranslationWithParentCultureFallback_FallsBackToParentCulture(string culture, string name, string expected)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);

        // Act
        string translation = _localizer[name];

        // Assert
        Assert.Equal(expected, translation);
    }

    [Theory]
    [InlineData("fr-FR", "Yes", false)]  // "Yes" found in parent culture fr.json
    [InlineData("fr-FR", "Hello", false)]  // "Hello" found in fr-FR.json
    [InlineData("fr-FR", "NonExistentKey", true)]  // Key doesn't exist anywhere
    public void GetTranslation_ResourceNotFound_ReturnsCorrectFlag(string culture, string name, bool expectedResourceNotFound)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);

        // Act
        var localizedString = _localizer[name];

        // Assert
        Assert.Equal(expectedResourceNotFound, localizedString.ResourceNotFound);
    }

    [Theory]
    [InlineData("fr-FR", "Yes")]  // "Yes" only exists in fr.json, should fallback
    [InlineData("fr-FR", "Hello")]  // "Hello" exists in fr-FR.json
    public void GetAllStrings_WithIncludeParentCultures_IncludesParentCultureStrings(string culture, string keyThatShouldExist)
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture(culture);

        // Act
        var localizedStrings = _localizer.GetAllStrings(includeParentCultures: true);

        // Assert
        Assert.Contains(localizedStrings, s => s.Name == keyThatShouldExist);
    }

    private class SharedResource
    {
        public string Hello { get; set; }

        public string Bye { get; set; }
    }
}
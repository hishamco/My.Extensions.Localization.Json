using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests;

public class MultipleResourcesPathsTests
{
    [Fact]
    public void LocalizerReturnsTranslationFromFirstPath()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = new[] { "Resources", "AdditionalResources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var translation = localizer["Hello"];

        // Assert
        Assert.Equal("Bonjour", translation);
    }

    [Fact]
    public void LocalizerReturnsTranslationFromSecondPath()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = new[] { "Resources", "AdditionalResources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var translation = localizer["AdditionalKey"];

        // Assert
        Assert.Equal("Clé supplémentaire", translation);
    }

    [Fact]
    public void LocalizerMergesResourcesFromMultiplePaths()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = new[] { "Resources", "AdditionalResources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act & Assert - key from first path
        var translationFromFirst = localizer["Hello"];
        Assert.Equal("Bonjour", translationFromFirst);

        // Act & Assert - key from second path
        var translationFromSecond = localizer["AdditionalKey"];
        Assert.Equal("Clé supplémentaire", translationFromSecond);
    }

    [Fact]
    public void LocalizerFirstPathTakesPrecedence()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = new[] { "Resources", "AdditionalResources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act - Hello exists in both paths
        var translation = localizer["Hello"];

        // Assert - First path value should win
        Assert.Equal("Bonjour", translation);
    }

    [Fact]
    public void LocalizerWorksWithSinglePath()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = new[] { "Resources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var translation = localizer["Hello"];

        // Assert
        Assert.Equal("Bonjour", translation);
    }

    [Fact]
    public void LocalizerWorksWithMultiplePathsIncludingNonExistent()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = new[] { "Resources", "AdditionalResources", "NonExistentPath" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var translationFromFirst = localizer["Hello"];
        var translationFromSecond = localizer["AdditionalKey"];

        // Assert
        Assert.Equal("Bonjour", translationFromFirst);
        Assert.Equal("Clé supplémentaire", translationFromSecond);
    }
}

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests;

public class AdditionalResourcesPathsTests
{
    [Fact]
    public void LocalizerReturnsTranslationFromPrimaryPath()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                AdditionalResourcesPaths = { "AdditionalResources" }
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
    public void LocalizerReturnsTranslationFromAdditionalPath()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                AdditionalResourcesPaths = { "AdditionalResources" }
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
                ResourcesPath = "Resources",
                AdditionalResourcesPaths = { "AdditionalResources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act & Assert - key from primary
        var translationFromPrimary = localizer["Hello"];
        Assert.Equal("Bonjour", translationFromPrimary);

        // Act & Assert - key from additional
        var translationFromAdditional = localizer["AdditionalKey"];
        Assert.Equal("Clé supplémentaire", translationFromAdditional);
    }

    [Fact]
    public void LocalizerPrimaryPathTakesPrecedence()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                AdditionalResourcesPaths = { "AdditionalResources" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act - Hello exists in both primary and additional paths
        var translation = localizer["Hello"];

        // Assert - Primary path value should win
        Assert.Equal("Bonjour", translation);
    }

    [Fact]
    public void LocalizerWorksWithNoAdditionalPaths()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources"
                // AdditionalResourcesPaths not specified, should use empty list
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
    public void LocalizerWorksWithMultipleAdditionalPaths()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                AdditionalResourcesPaths = { "AdditionalResources", "NonExistentPath" }
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var translationFromPrimary = localizer["Hello"];
        var translationFromAdditional = localizer["AdditionalKey"];

        // Assert
        Assert.Equal("Bonjour", translationFromPrimary);
        Assert.Equal("Clé supplémentaire", translationFromAdditional);
    }
}

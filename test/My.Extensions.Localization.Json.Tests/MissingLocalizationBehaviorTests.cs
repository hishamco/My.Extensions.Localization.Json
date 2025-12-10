using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests;

public class MissingLocalizationBehaviorTests
{
    [Fact]
    public void MissingLocalization_WithIgnoreBehavior_ReturnsKey()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                MissingLocalizationBehavior = MissingLocalizationBehavior.Ignore
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var result = localizer["NonExistentKey"];

        // Assert
        Assert.Equal("NonExistentKey", result.Value);
        Assert.True(result.ResourceNotFound);
    }

    [Fact]
    public void MissingLocalization_WithThrowExceptionBehavior_ThrowsMissingLocalizationException()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                MissingLocalizationBehavior = MissingLocalizationBehavior.ThrowException
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act & Assert
        var exception = Assert.Throws<MissingLocalizationException>(() => localizer["NonExistentKey"]);
        Assert.Equal("NonExistentKey", exception.Key);
        Assert.Equal("fr-FR", exception.Culture);
    }

    [Fact]
    public void MissingLocalization_WithLogWarningBehavior_LogsWarning()
    {
        // Arrange
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<JsonStringLocalizer>>();
        
        // Enable logging for all levels
        logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
        
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                MissingLocalizationBehavior = MissingLocalizationBehavior.LogWarning
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, loggerFactory.Object);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act
        var result = localizer["NonExistentKey"];

        // Assert
        Assert.Equal("NonExistentKey", result.Value);
        Assert.True(result.ResourceNotFound);
        
        // Verify logging was called - the warning should be logged
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void ExistingLocalization_DoesNotTriggerMissingBehavior()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                MissingLocalizationBehavior = MissingLocalizationBehavior.ThrowException
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act & Assert - Should not throw because the key exists
        var result = localizer["Hello"];
        Assert.Equal("Bonjour", result.Value);
        Assert.False(result.ResourceNotFound);
    }

    [Fact]
    public void MissingLocalizationWithArguments_WithThrowExceptionBehavior_ThrowsMissingLocalizationException()
    {
        // Arrange
        var localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions
            {
                ResourcesPath = "Resources",
                MissingLocalizationBehavior = MissingLocalizationBehavior.ThrowException
            });
        var localizerFactory = new JsonStringLocalizerFactory(localizationOptions.Object, NullLoggerFactory.Instance);
        var location = "My.Extensions.Localization.Json.Tests";
        var basename = $"{location}.Common.{nameof(Test)}";
        var localizer = localizerFactory.Create(basename, location);

        LocalizationHelper.SetCurrentCulture("fr-FR");

        // Act & Assert
        var exception = Assert.Throws<MissingLocalizationException>(() => localizer["NonExistentKey {0}", "arg1"]);
        Assert.Equal("NonExistentKey {0}", exception.Key);
    }

    [Fact]
    public void DefaultBehavior_IsIgnore()
    {
        // Arrange
        var options = new JsonLocalizationOptions();

        // Assert
        Assert.Equal(MissingLocalizationBehavior.Ignore, options.MissingLocalizationBehavior);
    }

    [Fact]
    public void MissingLocalizationException_ContainsCorrectProperties()
    {
        // Arrange & Act
        var exception = new MissingLocalizationException("TestKey", "en-US", "/path/to/resources");

        // Assert
        Assert.Equal("TestKey", exception.Key);
        Assert.Equal("en-US", exception.Culture);
        Assert.Equal("/path/to/resources", exception.SearchedLocation);
        Assert.Contains("TestKey", exception.Message);
        Assert.Contains("en-US", exception.Message);
        Assert.Contains("/path/to/resources", exception.Message);
    }
}

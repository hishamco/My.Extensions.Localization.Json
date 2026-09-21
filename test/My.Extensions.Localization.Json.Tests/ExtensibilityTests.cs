using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using My.Extensions.Localization.Json.Caching;
using My.Extensions.Localization.Json.Internal;
using My.Extensions.Localization.Json.Tests.Common;
using Xunit;

namespace My.Extensions.Localization.Json.Tests;

public class ExtensibilityTests
{
    private readonly Mock<IOptions<JsonLocalizationOptions>> _localizationOptions = new();
    private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

    public ExtensibilityTests()
    {
        _localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions { ResourcesPath = ["Resources"] });
    }

    [Fact]
    public void CustomJsonStringLocalizerFactory_CanOverrideCreateJsonStringLocalizer()
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture("fr-FR");
        var factory = new CustomJsonStringLocalizerFactory(_localizationOptions.Object, _loggerFactory);

        // Act
        var localizer = factory.Create(typeof(Test));

        // Assert
        Assert.NotNull(localizer);
        Assert.True(factory.CreateJsonStringLocalizerWasCalled);
    }

    [Fact]
    public void CustomJsonStringLocalizerFactory_CanAccessProtectedProperties()
    {
        // Arrange
        var factory = new CustomJsonStringLocalizerFactory(_localizationOptions.Object, _loggerFactory);

        // Assert
        Assert.NotNull(factory.GetResourceNamesCache());
        Assert.NotNull(factory.GetLocalizerCache());
        Assert.Contains("Resources", factory.GetResourcesRelativePath());
        Assert.Equal(ResourcesType.TypeBased, factory.GetResourcesType());
        Assert.NotNull(factory.GetLoggerFactory());
    }

    [Fact]
    public void CustomJsonStringLocalizer_CanOverrideGetStringSafely()
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture("fr-FR");
        var factory = new CustomLocalizerFactory(_localizationOptions.Object, _loggerFactory);
        var localizer = factory.Create(typeof(Test)) as CustomJsonStringLocalizer;

        // Act
        var result = localizer["Hello"];

        // Assert
        Assert.NotNull(localizer);
        Assert.True(localizer.GetStringSafelyWasCalled);
        Assert.Equal("Bonjour", result);
    }

    [Fact]
    public void CustomJsonStringLocalizer_CanOverrideGetAllStrings()
    {
        // Arrange
        LocalizationHelper.SetCurrentCulture("fr-FR");
        var factory = new CustomLocalizerFactory(_localizationOptions.Object, _loggerFactory);
        var localizer = factory.Create(typeof(Test)) as CustomJsonStringLocalizer;

        // Act
        var result = localizer.GetAllStrings(true);

        // Assert
        Assert.NotNull(localizer);
        Assert.True(localizer.GetAllStringsWasCalled);
        Assert.Contains(result, r => r.Name == "Hello" && r.Value == "Bonjour");
    }

    private class CustomJsonStringLocalizerFactory(IOptions<JsonLocalizationOptions> localizationOptions, ILoggerFactory loggerFactory)
        : JsonStringLocalizerFactory(localizationOptions, loggerFactory)
    {
        public bool CreateJsonStringLocalizerWasCalled { get; private set; }

        protected override JsonStringLocalizer CreateJsonStringLocalizer(string[] resourcesPaths, string resourceName)
        {
            CreateJsonStringLocalizerWasCalled = true;

            return base.CreateJsonStringLocalizer(resourcesPaths, resourceName);
        }

        // Expose protected properties for testing
        public IResourceNamesCache GetResourceNamesCache() => ResourceNamesCache;

        public ConcurrentDictionary<string, JsonStringLocalizer> GetLocalizerCache() => LocalizerCache;

        public string[] GetResourcesRelativePath() => ResourcesPaths;

        public ResourcesType GetResourcesType() => ResourcesType;

        public ILoggerFactory GetLoggerFactory() => LoggerFactory;
    }

    private class CustomLocalizerFactory(IOptions<JsonLocalizationOptions> localizationOptions, ILoggerFactory loggerFactory)
        : JsonStringLocalizerFactory(localizationOptions, loggerFactory)
    {
        protected override JsonStringLocalizer CreateJsonStringLocalizer(string[] resourcesPaths, string resourceName)
        {
            var resourceManager = ResourcesType == ResourcesType.TypeBased
                ? new JsonResourceManager(resourcesPaths, resourceName)
                : new JsonResourceManager(resourcesPaths);
            var logger = LoggerFactory.CreateLogger<CustomJsonStringLocalizer>();

            return new CustomJsonStringLocalizer(resourceManager, ResourceNamesCache, logger);
        }
    }

    private class CustomJsonStringLocalizer(
        JsonResourceManager jsonResourceManager,
        IResourceNamesCache resourceNamesCache,
        ILogger logger) : JsonStringLocalizer(jsonResourceManager, resourceNamesCache, logger)
    {
        public bool GetStringSafelyWasCalled { get; private set; }

        public bool GetAllStringsWasCalled { get; private set; }

        protected override string GetStringSafely(string name, CultureInfo culture)
        {
            GetStringSafelyWasCalled = true;

            return base.GetStringSafely(name, culture);
        }

        public override IEnumerable<Microsoft.Extensions.Localization.LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            GetAllStringsWasCalled = true;

            return base.GetAllStrings(includeParentCultures);
        }
    }
}

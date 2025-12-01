using System;
using System.Collections.Concurrent;
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
    private readonly Mock<IOptions<JsonLocalizationOptions>> _localizationOptions;
    private readonly ILoggerFactory _loggerFactory;

    public ExtensibilityTests()
    {
        _localizationOptions = new Mock<IOptions<JsonLocalizationOptions>>();
        _localizationOptions.Setup(o => o.Value)
            .Returns(() => new JsonLocalizationOptions { ResourcesPath = "Resources" });
        _loggerFactory = NullLoggerFactory.Instance;
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
        Assert.Equal("Resources", factory.GetResourcesRelativePath());
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
    }

    /// <summary>
    /// Custom factory that overrides CreateJsonStringLocalizer to demonstrate extensibility.
    /// </summary>
    private class CustomJsonStringLocalizerFactory : JsonStringLocalizerFactory
    {
        public bool CreateJsonStringLocalizerWasCalled { get; private set; }

        public CustomJsonStringLocalizerFactory(
            IOptions<JsonLocalizationOptions> localizationOptions,
            ILoggerFactory loggerFactory)
            : base(localizationOptions, loggerFactory)
        {
        }

        protected override JsonStringLocalizer CreateJsonStringLocalizer(string resourcesPath, string resourceName)
        {
            CreateJsonStringLocalizerWasCalled = true;
            return base.CreateJsonStringLocalizer(resourcesPath, resourceName);
        }

        // Expose protected properties for testing
        public IResourceNamesCache GetResourceNamesCache() => ResourceNamesCache;
        public ConcurrentDictionary<string, JsonStringLocalizer> GetLocalizerCache() => LocalizerCache;
        public string GetResourcesRelativePath() => ResourcesRelativePath;
        public ResourcesType GetResourcesType() => ResourcesType;
        public ILoggerFactory GetLoggerFactory() => LoggerFactory;
    }

    /// <summary>
    /// Custom factory that creates CustomJsonStringLocalizer instances.
    /// </summary>
    private class CustomLocalizerFactory : JsonStringLocalizerFactory
    {
        public CustomLocalizerFactory(
            IOptions<JsonLocalizationOptions> localizationOptions,
            ILoggerFactory loggerFactory)
            : base(localizationOptions, loggerFactory)
        {
        }

        protected override JsonStringLocalizer CreateJsonStringLocalizer(string resourcesPath, string resourceName)
        {
            var resourceManager = ResourcesType == ResourcesType.TypeBased
                ? new JsonResourceManager(resourcesPath, resourceName)
                : new JsonResourceManager(resourcesPath);
            var logger = LoggerFactory.CreateLogger<CustomJsonStringLocalizer>();

            return new CustomJsonStringLocalizer(resourceManager, ResourceNamesCache, logger);
        }
    }

    /// <summary>
    /// Custom localizer that overrides GetStringSafely to demonstrate extensibility.
    /// </summary>
    private class CustomJsonStringLocalizer : JsonStringLocalizer
    {
        public bool GetStringSafelyWasCalled { get; private set; }
        public bool GetAllStringsWasCalled { get; private set; }

        public CustomJsonStringLocalizer(
            JsonResourceManager jsonResourceManager,
            IResourceNamesCache resourceNamesCache,
            ILogger logger)
            : base(jsonResourceManager, resourceNamesCache, logger)
        {
        }

        protected override string GetStringSafely(string name, CultureInfo culture)
        {
            GetStringSafelyWasCalled = true;
            return base.GetStringSafely(name, culture);
        }

        public override System.Collections.Generic.IEnumerable<Microsoft.Extensions.Localization.LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            GetAllStringsWasCalled = true;
            return base.GetAllStrings(includeParentCultures);
        }
    }
}

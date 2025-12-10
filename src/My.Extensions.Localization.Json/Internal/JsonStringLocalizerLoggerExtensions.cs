using System;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace My.Extensions.Localization.Json.Internal;

internal static class JsonStringLocalizerLoggerExtensions
{
    private static readonly Action<ILogger, string, string, CultureInfo, Exception> _searchedLocation;
    private static readonly Action<ILogger, string, string, CultureInfo, Exception> _missingLocalization;

    static JsonStringLocalizerLoggerExtensions()
    {
        _searchedLocation = LoggerMessage.Define<string, string, CultureInfo>(
            LogLevel.Debug,
            1,
            $"{nameof(JsonStringLocalizer)} searched for '{{Key}}' in '{{LocationSearched}}' with culture '{{Culture}}'.");

        _missingLocalization = LoggerMessage.Define<string, string, CultureInfo>(
            LogLevel.Warning,
            2,
            $"{nameof(JsonStringLocalizer)} could not find localization for '{{Key}}' in '{{LocationSearched}}' with culture '{{Culture}}'.");
    }

    public static void SearchedLocation(this ILogger logger, string key, string searchedLocation, CultureInfo culture)
    {
        _searchedLocation(logger, key, searchedLocation, culture, null);
    }

    public static void MissingLocalization(this ILogger logger, string key, string searchedLocation, CultureInfo culture)
    {
        _missingLocalization(logger, key, searchedLocation, culture, null);
    }
}

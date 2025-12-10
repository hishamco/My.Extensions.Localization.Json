using System;

namespace My.Extensions.Localization.Json;

/// <summary>
/// The exception that is thrown when a localization resource is not found
/// and the <see cref="MissingLocalizationBehavior.ThrowException"/> behavior is configured.
/// </summary>
public class MissingLocalizationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MissingLocalizationException"/> class.
    /// </summary>
    public MissingLocalizationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingLocalizationException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MissingLocalizationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingLocalizationException"/> class
    /// with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MissingLocalizationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingLocalizationException"/> class
    /// with details about the missing localization.
    /// </summary>
    /// <param name="key">The localization key that was not found.</param>
    /// <param name="culture">The culture for which the localization was not found.</param>
    /// <param name="searchedLocation">The location where the localization was searched.</param>
    public MissingLocalizationException(string key, string culture, string searchedLocation)
        : base($"Localization for key '{key}' was not found for culture '{culture}' in '{searchedLocation}'.")
    {
        Key = key;
        Culture = culture;
        SearchedLocation = searchedLocation;
    }

    /// <summary>
    /// Gets the localization key that was not found.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the culture for which the localization was not found.
    /// </summary>
    public string Culture { get; }

    /// <summary>
    /// Gets the location where the localization was searched.
    /// </summary>
    public string SearchedLocation { get; }
}

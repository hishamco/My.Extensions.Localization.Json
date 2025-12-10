using Microsoft.Extensions.Localization;
using System;
using System.Linq.Expressions;

namespace My.Extensions.Localization.Json;

/// <summary>
/// Provides extension methods for retrieving localized strings using strongly-typed resource property expressions.
/// </summary>
public static class StringLocalizerExtensions
{
    /// <summary>
    /// Retrieves a localized string for the specified property of a resource type using a strongly-typed expression.
    /// </summary>
    /// <typeparam name="TResource">The resource type containing the property for which to retrieve the localized string.</typeparam>
    /// <param name="stringLocalizer">The string localizer instance used to obtain localized values for the resource type.</param>
    /// <param name="propertyExpression">An expression that identifies the string property of the resource type to localize. Must refer to a property of
    /// type <see cref="string"/>.</param>
    /// <returns>A <see cref="LocalizedString"/> containing the localized value for the specified property. If no localized value
    /// is found, the property name is returned as the value.</returns>
    public static LocalizedString GetString<TResource>(this IStringLocalizer stringLocalizer, Expression<Func<TResource, string>> propertyExpression)
        => stringLocalizer[(propertyExpression.Body as MemberExpression).Member.Name];
}
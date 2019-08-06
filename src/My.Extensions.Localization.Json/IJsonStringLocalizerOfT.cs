using Microsoft.Extensions.Localization;
using System;
using System.Linq.Expressions;

namespace My.Extensions.Localization.Json
{
    public interface IJsonStringLocalizer<T> : IStringLocalizer<T>
    {
        LocalizedString this[Expression<Func<T, string>> propertyExpression] { get; }
    }
}
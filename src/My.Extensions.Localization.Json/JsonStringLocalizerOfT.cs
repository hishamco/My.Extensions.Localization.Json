using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.Extensions.Localization;

namespace My.Extensions.Localization.Json
{
    public class JsonStringLocalizer<TResourceSource> : IJsonStringLocalizer<TResourceSource>
    {
        private IStringLocalizer _localizer;

        public JsonStringLocalizer(IStringLocalizerFactory factory)
        {
            if(factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _localizer = factory.Create(typeof(TResourceSource));
        }

        public LocalizedString this[Expression<Func<TResourceSource, string>> propertyExpression]
            => this[(propertyExpression.Body as MemberExpression).Member.Name];

        public LocalizedString this[string name] => _localizer[name];

        public LocalizedString this[string name, params object[] arguments] => _localizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _localizer.GetAllStrings(includeParentCultures);

        public IStringLocalizer WithCulture(CultureInfo culture) => _localizer.WithCulture(culture);
    }
}
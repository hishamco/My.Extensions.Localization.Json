using System;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonLocalizationMvcDataAnnotationsExtensions
    {
        public static IMvcBuilder AddDataAnnotationsJsonLocalization(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.AddDataAnnotationsLocalization(o =>
            {
                var provider = builder.Services.BuildServiceProvider();
                var localizer = provider.GetService<IStringLocalizer>();
                o.DataAnnotationLocalizerProvider = (t, f) => localizer;
            });
            return builder;
        }
    }
}
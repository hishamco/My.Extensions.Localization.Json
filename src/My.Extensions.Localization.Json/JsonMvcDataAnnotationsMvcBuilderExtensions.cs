using System;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonLocalizationMvcDataAnnotationsMvcBuilderExtensions
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
                var factory = provider.GetService<IStringLocalizerFactory>();
                var localizer = provider.GetService<IStringLocalizer>();

                o.DataAnnotationLocalizerProvider = (t, f) => localizer;
            });
            return builder;
        }
    }
}
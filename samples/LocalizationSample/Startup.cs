using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LocalizationSample.Resources;

namespace LocalizationSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Keep the following two lined in the same order. Or else, the `ResourcesPath` will not be guaranteed to be the same for both.
            services.AddJsonLocalization(options => options.ResourcesPath = "Resources");

            // Use this when having Mcv.
            // This use the localization files at the `ResourcesPath` provided above.
            services.AddMvc().AddDataAnnotationsJsonLocalization();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IStringLocalizer<Startup> localizer1, IStringLocalizer<Model> localizer2)
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US"),
                new CultureInfo("fr-FR")
            };
            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            app.UseRequestLocalization(options);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync($"{localizer1["Hello"]} - {localizer2["Hello"]}!!");
            });
        }
    }
}

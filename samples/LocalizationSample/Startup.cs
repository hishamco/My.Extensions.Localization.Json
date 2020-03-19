using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using My.Extensions.Localization.Json;

namespace LocalizationSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
            services.TryAddSingleton<IStringLocalizerFactory, CustomJsonStringLocalizerFactory>();
            services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            services.Configure<JsonLocalizationOptions>(options => options.ResourcesPath = "Resources");
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, IStringLocalizer<Startup> localizer)
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
                await context.Response.WriteAsync($"{localizer["Hello"]}!!");
                await context.Response.WriteAsync($"{localizer["Yes"]}!!");
            });
        }

        // Just for strongly typed resource
        public string Hello { get; set; }
    }
}

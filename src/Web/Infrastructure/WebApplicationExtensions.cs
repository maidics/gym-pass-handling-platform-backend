using System.Reflection;
using FitPass.Application.Common.Settings;
using FitPass.Infrastructure.Common;
using Microsoft.AspNetCore.Localization;

namespace FitPass.Web.Infrastructure;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        private RouteGroupBuilder MapGroup(EndpointGroupBase group)
        {
            var groupName = group.GroupName ?? group.GetType().Name;

            return app.MapGroup($"/api/{groupName}").WithGroupName(groupName).WithTags(groupName);
        }

        public WebApplication MapEndpoints()
        {
            var endpointGroupType = typeof(EndpointGroupBase);

            var assembly = Assembly.GetExecutingAssembly();

            var endpointGroupTypes = assembly
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(endpointGroupType));

            foreach (var type in endpointGroupTypes)
            {
                if (Activator.CreateInstance(type) is EndpointGroupBase instance)
                {
                    instance.Map(app.MapGroup(instance));
                }
            }

            return app;
        }

        public WebApplication AddLocalization()
        {
            var cultureSettings = app
                .Configuration.GetSection(ConfigurationSections.Cultures)
                .Get<CultureSettings>();

            Guard.Against.Null(cultureSettings);

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(cultureSettings.DefaultCulture)
                .AddSupportedCultures(cultureSettings.SupportedCultures)
                .AddSupportedUICultures(cultureSettings.SupportedCultures);

            localizationOptions.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider(),
            ];

            app.UseRequestLocalization(localizationOptions);

            return app;
        }
    }
}

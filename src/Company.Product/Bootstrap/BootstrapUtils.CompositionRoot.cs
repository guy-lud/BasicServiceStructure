using ExistForAll.SimpleSettings;
using ExistForAll.SimpleSettings.Binders;
using ExistForAll.SimpleSettings.Extensions.GenericHost;
using Microsoft.OpenApi.Models;

namespace Company.Product.Bootstrap;

public static partial class BootstrapUtils
{
    internal static WebApplicationBuilder ComposeRoot(this WebApplicationBuilder applicationBuilder, IConfiguration configuration)
    {
        var services = applicationBuilder.Services; 
        
        RegisterAppInsights(services, configuration);

        services.AddControllers(options =>
            {
                //options.Filters.Add(typeof(HttpGlobalExceptionFilter/));

            }) // Added for functional tests
            //.AddApplicationPart(typeof(BasketController).Assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        services.AddSwaggerGen(options =>
        {            
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "XXX HTTP API",
                Version = "v1",
                Description = "Service HTTP API"
            });
        });

        services.AddSimpleSettings(builder =>
        {
            builder.SetSettingsSuffix<ISettingsBuilderOptions>("Settings")
                .AddAssembly<Program>()
                .AddConfiguration(configuration)
                .AddEnvironmentVariable()
                .AddCommandLine();
        });

        services.AddCustomHealthCheck(configuration);
        
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddOptions();

        return applicationBuilder;
    }
    
    private static void RegisterAppInsights(IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration);
        services.AddApplicationInsightsKubernetesEnricher();
    }
}
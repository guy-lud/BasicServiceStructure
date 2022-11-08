using Microsoft.OpenApi.Models;
using ExistForAll.SimpleSettings;
using ExistForAll.SimpleSettings.Binders;
using ExistForAll.SimpleSettings.Extensions.GenericHost;

namespace CompanyName.Product.Bootstrap;

public static partial class BootstrapUtils
{
    internal static void ComposeRoot(IServiceCollection services, IConfiguration configuration)
    {

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

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                        TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                        {
                            { "basket", "Basket API" }
                        }
                    }
                }
            });

            //options.OperationFilter<AuthorizeCheckOperationFilter>();
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
    }
    
    private static void RegisterAppInsights(IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration);
        services.AddApplicationInsightsKubernetesEnricher();
    }
}
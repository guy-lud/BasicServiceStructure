using System.Net;
using HealthChecks.UI.Client;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

namespace CompanyName.Product;


public class MyException : Exception
{
    public HttpStatusCode Code { get; set; }
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public virtual void ConfigureServices(IServiceCollection services)
    {

        services.AddProblemDetails(dude =>
        {
            dude.Map<MyException>(x =>
            {
                return new StatusCodeProblemDetails((int)x.Code);
            });
        });
        
        RegisterAppInsights(services);

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

            // options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            // {
            //     Type = SecuritySchemeType.OAuth2,
            //     Flows = new OpenApiOAuthFlows()
            //     {
            //         Implicit = new OpenApiOAuthFlow()
            //         {
            //             AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
            //             TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
            //             Scopes = new Dictionary<string, string>()
            //             {
            //                 { "basket", "Basket API" }
            //             }
            //         }
            //     }
            // });

            //options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        //ConfigureAuthService(services);

        services.AddCustomHealthCheck(Configuration);
        
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddOptions();

        //return services.BuildServiceProvider();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        app.UseProblemDetails();

            var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseSwagger()
            .UseSwaggerUI(setup =>
            {
                setup.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Basket.API V1");
                setup.OAuthClientId("basketswaggerui");
                setup.OAuthAppName("Basket Swagger UI");
            });

        app.UseRouting();
        app.UseCors("CorsPolicy");
        ConfigureAuth(app);

        app.UseStaticFiles();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            endpoints.MapControllers();
          
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            endpoints.MapGet("/test", context =>
            {
                throw new MyException()
                {
                    Code = HttpStatusCode.Unauthorized
                };
            });
            
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
    }

    private void RegisterAppInsights(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry(Configuration);
        services.AddApplicationInsightsKubernetesEnricher();
    }

    private void ConfigureAuthService(IServiceCollection services)
    {
        // prevent from mapping "sub" claim to nameidentifier.
    //     JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
    //
    //     var identityUrl = Configuration.GetValue<string>("IdentityUrl");
    //
    //     services.AddAuthentication(options =>
    //     {
    //         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //
    //     }).AddJwtBearer(options =>
    //     {
    //         options.Authority = identityUrl;
    //         options.RequireHttpsMetadata = false;
    //         options.Audience = "basket";
    //     });
    }

    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}

public static class CustomExtensionMethods
{
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());
        return services;
    }
}
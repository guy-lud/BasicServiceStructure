using ExistsForAll.Shepherd.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Company.Product.Bootstrap;

public static partial class BootstrapUtils
{
    internal static WebApplicationBuilder SimpleInjectorComposeRoot(this WebApplicationBuilder webApplicationBuilder, Container container)
    {
        var services = webApplicationBuilder.Services;
        container.Scan(x => x.WithAssembly<Program>());
        
        services.AddSimpleInjector(container, options =>
        {
            options.AddLogging()
                .AddAspNetCore()
                .AddControllerActivation();
        });

        return webApplicationBuilder;
    }

    internal static Container CreateSimpleInjectorContainer()
    {
        return new Container()
        {
            Options =
            {
                DefaultLifestyle = Lifestyle.Singleton,
                DefaultScopedLifestyle = new AsyncScopedLifestyle()
            }
        };
    }
}
using ExistsForAll.Shepherd.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace CompanyName.Product.Bootstrap;

public static partial class BootstrapUtils
{
    internal static void SimpleInjectorComposeRoot(IServiceCollection services, Container container)
    {
        container.Scan(x => x.WithAssembly<Program>());
        
        services.AddSimpleInjector(container, options =>
        {
            options.AddAspNetCore()
                .AddControllerActivation();

            options.AddLogging();
            options.AddLocalization(); //?
        });

        
        
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
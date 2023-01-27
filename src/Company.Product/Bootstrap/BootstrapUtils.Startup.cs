using SimpleInjector;

namespace Company.Product.Bootstrap;

public static partial class BootstrapUtils
{
    public static WebApplication UsingStatements(this WebApplication target, Container container)
    {
        target.Services.UseSimpleInjector(container);
        return target;
    }
}
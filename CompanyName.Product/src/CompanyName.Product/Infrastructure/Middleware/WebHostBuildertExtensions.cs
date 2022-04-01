namespace CompanyName.Product.Infrastructure.Middleware;

public static class WebHostBuildertExtensions
{
    public static IWebHostBuilder UseFailing(this IWebHostBuilder builder, Action<FailingOptions> options)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IStartupFilter>(new FailingStartupFilter(options));
        });
        return builder;
    }
}
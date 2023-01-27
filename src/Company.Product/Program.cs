using Serilog;
using SimpleInjector;
using static Company.Product.Bootstrap.BootstrapUtils;

var configuration = GetConfiguration();

var applicationName = configuration.GetValue<string?>("ApplicationName", "no-name");

Log.Logger = CreateSerilogLogger(configuration, applicationName);

var container = CreateSimpleInjectorContainer();

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", applicationName);

    var hostBuilder = CreateStandardWebHostBuilder(configuration, args);

    var app = hostBuilder.ComposeRoot(configuration)
        .SimpleInjectorComposeRoot(container)
        .Build();

    Log.Information("Starting web host ({ApplicationContext})...", applicationName);

    app.UseSimpleInjector(container);
    
    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", applicationName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

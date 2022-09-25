using System.Net;
using CompanyName.Product;
using CompanyName.Product.Infrastructure.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Formatting.Json;
using ILogger = Serilog.ILogger;

const string appName = "AppName";
var configuration = GetConfiguration();

Log.Logger = CreateSerilogLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", appName);
            
    var host = BuildWebHost(configuration, args);

    Log.Information("Starting web host ({ApplicationContext})...", appName);
            
    await host.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", appName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}


IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json",  false)
        .AddYamlFile("appsettings.yaml", true)
        .AddEnvironmentVariables();

    return builder.Build();
}

ILogger CreateSerilogLogger(IConfiguration config)
{
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", appName)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter())
        .ReadFrom.Configuration(config)
        .CreateLogger();
}

IWebHost BuildWebHost(IConfiguration config, string[] args)
{
    return WebHost.CreateDefaultBuilder(args)
        .ConfigureLogging(loggerBuilder =>
        {
            loggerBuilder.ClearProviders();
        })
        .UseSerilog()
        .CaptureStartupErrors(false)
        .ConfigureKestrel(options =>
        {
            var ports = GetDefinedPorts(config);
            options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });

            options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });

        })
        .ConfigureAppConfiguration(x => x.AddConfiguration(config))
        .UseFailing(options =>
        {
            options.ConfigPath = "/Failing";
            options.NotFilteredPaths.AddRange(new[] { "/hc", "/liveness" });
        })
        //.UseStartup<Startup>()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Build();
}

(int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
{
    var grpcPort = config.GetValue("GRPC_PORT", 5001);
    var port = config.GetValue("PORT", 5000);
    return (port, grpcPort);
}

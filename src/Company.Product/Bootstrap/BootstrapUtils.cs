using System.Net;
using Company.Product.Infrastructure.Middleware;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace Company.Product.Bootstrap;

public static partial class BootstrapUtils
{
    internal static WebApplicationBuilder CreateStandardWebHostBuilder(IConfiguration configuration, string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Host
            .UseSerilog()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.CaptureStartupErrors(false)
                    .ConfigureKestrel(options =>
                    {
                        var ports = GetDefinedPorts(configuration);
                        options.Listen(IPAddress.Any, ports.httpPort,
                            listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });

                        options.Listen(IPAddress.Any, ports.grpcPort,
                            listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                    })
                    .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
                    .UseFailing(options =>
                    {
                        options.ConfigPath = "/Failing";
                        options.NotFilteredPaths.AddRange(new[] { "/hc", "/liveness" });
                    })
                    .UseContentRoot(Directory.GetCurrentDirectory());
            });

        return builder;
    }
    
    private static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
    {
        var grpcPort = config.GetValue("GRPC_PORT", 5001);
        var port = config.GetValue("PORT", 5000);
        return (port, grpcPort);
    }
    
    internal static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddYamlFile("appsettings.yaml", true)
            .AddEnvironmentVariables();

        return builder.Build();
    }
    
    internal static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration, string? applicationName)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationContext", applicationName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}
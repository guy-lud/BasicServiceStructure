using System.Net;
using CompanyName.Product.Infrastructure.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace CompanyName.Product;

public class Program
{
    public static string AppName => "AppName";
    
    public static async Task<int> Main(string[] args)
    {
        var configuration = GetConfiguration();

        Log.Logger = CreateSerilogLogger(configuration);

        try
        {
            Log.Information("Configuring web host ({ApplicationContext})...", AppName);
            
            var host = BuildWebHost(configuration, args);

            Log.Information("Starting web host ({ApplicationContext})...", AppName);
            
            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddYamlFile("appsettings.yaml", true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    private static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationContext", Program.AppName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    private static IWebHost BuildWebHost(IConfiguration configuration, string[] args)
    {
        return WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(false)
            .ConfigureKestrel(options =>
            {
                var ports = GetDefinedPorts(configuration);
                options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });

                options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });

            })
            .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
            .UseFailing(options =>
            {
                options.ConfigPath = "/Failing";
                options.NotFilteredPaths.AddRange(new[] { "/hc", "/liveness" });
            })
            .UseStartup<Startup>()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseSerilog()
            .Build();
    }
    
    private static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
    {
        var grpcPort = config.GetValue("GRPC_PORT", 5001);
        var port = config.GetValue("PORT", 5000);
        return (port, grpcPort);
    }
}
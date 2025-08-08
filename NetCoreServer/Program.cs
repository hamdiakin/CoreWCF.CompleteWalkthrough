using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NetCoreServer
{
    class Program
    {
        private static readonly JsonSerializerSettings JsonSettings = JsonUtilities.StandardSettings;

        static async Task Main(string[] args)
        {
            Console.Title = "ZeroMQ Echo Server with Integrated Inventory Notifications";

            try
            {
                var host = CreateHostBuilder(args).Build();
                
                Console.WriteLine("Starting ZeroMQ Echo Server with integrated inventory notifications...");
                Console.WriteLine("The server will handle both echo requests and publish inventory notifications.");
                Console.WriteLine("Press Ctrl+C to stop the server");

                // Run the host (this will start the EchoServerService which includes the publisher)
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                // Use minimal fallback logging if DI logger is not available
                Console.Error.WriteLine($"Server terminated unexpectedly: {ex.Message}");
                Console.Error.WriteLine(ex.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IEchoService, EchoService>();
                    services.AddHostedService<EchoServerService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
    }
}
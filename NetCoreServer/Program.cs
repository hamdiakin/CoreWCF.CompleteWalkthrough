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
            Console.Title = "ZeroMQ Echo Server";

            try
            {
                var host = CreateHostBuilder(args).Build();
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
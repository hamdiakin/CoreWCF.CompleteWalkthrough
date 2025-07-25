using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetCoreClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Title = "ZeroMQ .NET Client";

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<EchoClient>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var client = host.Services.GetRequiredService<EchoClient>();

            try
            {
                logger.LogInformation("Testing ZeroMQ Echo Service...");

                // Test simple Echo
                await TestEchoAsync(client, logger);

                // Test ComplexEcho
                await TestComplexEchoAsync(client, logger);

                // Test FailEcho (fault handling)
                await TestFailEchoAsync(client, logger);

                // Test EchoForPermission
                await TestEchoForPermissionAsync(client, logger);

                logger.LogInformation("All tests completed.");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during client execution");
            }
        }

        private static async Task TestEchoAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("Testing Echo method...");
            try
            {
                var result = await client.EchoAsync("Hello World from ZeroMQ!");
                logger.LogInformation("Response: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Echo failed");
            }
        }

        private static async Task TestComplexEchoAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("Testing ComplexEcho method...");
            try
            {
                var message = new EchoMessage { Text = "Complex message from ZeroMQ!" };
                var result = await client.ComplexEchoAsync(message);
                logger.LogInformation("Response: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ComplexEcho failed");
            }
        }

        private static async Task TestFailEchoAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("Testing FailEcho method (should throw exception)...");
            try
            {
                var result = await client.FailEchoAsync("This should fail");
                logger.LogWarning("Unexpected success: {Result}", result);
            }
            catch (ServiceException ex)
            {
                logger.LogInformation("Expected exception caught: {Message} (Reason: {Reason})", ex.Message, ex.Reason);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception");
            }
        }

        private static async Task TestEchoForPermissionAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("Testing EchoForPermission method...");
            try
            {
                var result = await client.EchoForPermissionAsync("Permission test message");
                logger.LogInformation("Response: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EchoForPermission failed");
            }
        }
    }
}
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using ZmqServer;

namespace NetCoreServer
{
    class Program
    {
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
                Console.WriteLine($"Server terminated unexpectedly: {ex.Message}");
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IEchoService, EchoService>();
                    services.AddHostedService<ZmqServerService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
    }

    public class ZmqServerService : BackgroundService
    {
        private readonly ILogger<ZmqServerService> logger;
        private readonly IEchoService _echoService;

        public ZmqServerService(ILogger<ZmqServerService> logger, IEchoService echoService)
        {
            this.logger = logger;
            _echoService = echoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting ZeroMQ Echo Server...");

            using var server = new ResponseSocket();
            server.Bind(AppConfig.TcpEndpoint);

            logger.LogInformation($"ZeroMQ server listening on {AppConfig.TcpEndpoint}");
            logger.LogInformation("Available methods: Echo, ComplexEcho, FailEcho, EchoForPermission");
            logger.LogInformation("Press Ctrl+C to stop the server");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if there's a message available (non-blocking)
                    if (server.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out string? requestJson))
                    {
                        await ProcessMessageAsync(server, requestJson, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                }
            }

            logger.LogInformation("ZeroMQ server stopping...");
        }

        private async Task ProcessMessageAsync(ResponseSocket server, string requestJson, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug($"Received raw message: {requestJson}");

                var request = JsonConvert.DeserializeObject<ZmqRequest>(requestJson);
                if (request == null)
                {
                    var errorResponse = new ZmqResponse
                    {
                        Success = false,
                        Error = new EchoFault
                        {
                            Text = "Invalid request format",
                            Reason = "InvalidJson"
                        }
                    };

                    var errorJson = JsonConvert.SerializeObject(errorResponse);
                    server.SendFrame(errorJson);
                    return;
                }

                logger.LogInformation($"Processing {request.Method} request (ID: {request.RequestId})");

                // Process the request
                var response = await Task.FromResult(_echoService.ProcessRequest(request));

                // Send response
                var responseJson = JsonConvert.SerializeObject(response);
                server.SendFrame(responseJson);

                logger.LogDebug($"Sent response: {responseJson}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message");

                var errorResponse = new ZmqResponse
                {
                    Success = false,
                    Error = new EchoFault
                    {
                        Text = ex.Message,
                        Reason = "ProcessingError"
                    }
                };

                try
                {
                    var errorJson = JsonConvert.SerializeObject(errorResponse);
                    server.SendFrame(errorJson);
                }
                catch (Exception sendEx)
                {
                    logger.LogError(sendEx, "Failed to send error response");
                }
            }
        }
    }
}
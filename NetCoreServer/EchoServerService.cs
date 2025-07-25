using Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;

namespace NetCoreServer
{
    public class EchoServerService : BackgroundService
    {
        private readonly ILogger<EchoServerService> logger;
        private readonly IEchoService echoService;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public EchoServerService(ILogger<EchoServerService> logger, IEchoService echoService)
        {
            this.logger = logger;
            this.echoService = echoService;
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

                var request = JsonSerializer.Deserialize<EchoRequest>(requestJson, JsonOptions);
                if (request == null)
                {
                    var errorResponse = new EchoResponse
                    {
                        Success = false,
                        Error = new EchoFault
                        {
                            Text = "Invalid request format",
                            Reason = "InvalidJson"
                        }
                    };

                    var errorJson = JsonSerializer.Serialize(errorResponse, JsonOptions);
                    server.SendFrame(errorJson);
                    return;
                }

                logger.LogInformation($"Processing {request.Method} request (ID: {request.RequestId})");

                // Process the request
                var response = await echoService.ProcessRequest(request);

                // Send response
                var responseJson = JsonSerializer.Serialize(response, JsonOptions);
                server.SendFrame(responseJson);

                logger.LogDebug($"Sent response: {responseJson}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message");

                var errorResponse = new EchoResponse
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
                    var errorJson = JsonSerializer.Serialize(errorResponse, JsonOptions);
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

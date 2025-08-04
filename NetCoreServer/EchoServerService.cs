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
            logger.LogInformation("Available methods: Echo, ComplexEcho, FailEcho, EchoForPermission, ProcessUserProfile, ValidateUserData, ProcessWithOptions, GetProcessingOptions, UpdateUserStatus, ProcessComplexData");
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

                // Call interface methods directly based on the method type
                var response = await ProcessRequestDirectly(request);

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

        private async Task<EchoResponse> ProcessRequestDirectly(EchoRequest request)
        {
            try
            {
                switch (request.Method)
                {
                    case ServiceMethodType.Echo:
                        var echoResult = await echoService.Echo(request.Payload ?? string.Empty);
                        return new EchoResponse
                        {
                            RequestId = request.RequestId,
                            Success = true,
                            Result = echoResult
                        };

                    case ServiceMethodType.ComplexEcho:
                        if (string.IsNullOrWhiteSpace(request.Payload))
                        {
                            return new EchoResponse
                            {
                                RequestId = request.RequestId,
                                Success = true,
                                Result = null
                            };
                        }
                        
                        try
                        {
                            var message = JsonSerializer.Deserialize<EchoMessage>(request.Payload, JsonOptions);
                            var complexResult = await echoService.ComplexEcho(message ?? new EchoMessage());
                            return new EchoResponse
                            {
                                RequestId = request.RequestId,
                                Success = true,
                                Result = complexResult
                            };
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to deserialize or process ComplexEcho payload");
                            return new EchoResponse
                            {
                                RequestId = request.RequestId,
                                Success = false,
                                Error = new EchoFault { Text = "Invalid ComplexEcho payload", Reason = "InvalidJson" }
                            };
                        }

                    case ServiceMethodType.FailEcho:
                        try
                        {
                            var failResult = await echoService.FailEcho(request.Payload ?? string.Empty);
                            return new EchoResponse
                            {
                                RequestId = request.RequestId,
                                Success = true,
                                Result = failResult
                            };
                        }
                        catch (ServiceException ex)
                        {
                            return new EchoResponse
                            {
                                RequestId = request.RequestId,
                                Success = false,
                                Error = new EchoFault { Text = ex.Message, Reason = ex.Reason }
                            };
                        }

                    case ServiceMethodType.EchoForPermission:
                        var permissionResult = await echoService.EchoForPermission(request.Payload ?? string.Empty);
                        return new EchoResponse
                        {
                            RequestId = request.RequestId,
                            Success = true,
                            Result = permissionResult
                        };

                    // Complex methods
                    case ServiceMethodType.ProcessUserProfile:
                        return await ProcessComplexMethodAsync(request, async (payload) =>
                        {
                            var user = JsonSerializer.Deserialize<UserProfile>(payload.GetProperty("user").GetRawText(), JsonOptions);
                            var operationId = payload.GetProperty("operationId").GetString();
                            var validateOnly = payload.GetProperty("validateOnly").GetBoolean();
                            
                            var result = await echoService.ProcessUserProfile(user!, operationId!, validateOnly);
                            return JsonSerializer.Serialize(result, JsonOptions);
                        });

                    case ServiceMethodType.ValidateUserData:
                        return await ProcessComplexMethodAsync(request, async (payload) =>
                        {
                            var user = JsonSerializer.Deserialize<UserProfile>(payload.GetProperty("user").GetRawText(), JsonOptions);
                            var strictValidation = payload.GetProperty("strictValidation").GetBoolean();
                            var maxErrors = payload.GetProperty("maxErrors").GetInt32();
                            
                            var result = await echoService.ValidateUserData(user!, strictValidation, maxErrors);
                            return JsonSerializer.Serialize(result, JsonOptions);
                        });

                    case ServiceMethodType.ProcessWithOptions:
                        return await ProcessComplexMethodAsync(request, async (payload) =>
                        {
                            var data = payload.GetProperty("data").GetString();
                            var options = JsonSerializer.Deserialize<ProcessingOptions>(payload.GetProperty("options").GetRawText(), JsonOptions);
                            var isPriority = payload.GetProperty("isPriority").GetBoolean();
                            
                            var result = await echoService.ProcessWithOptions(data!, options!, isPriority);
                            return result;
                        });

                    case ServiceMethodType.GetProcessingOptions:
                        return await ProcessComplexMethodAsync(request, async (payload) =>
                        {
                            var profileType = payload.GetProperty("profileType").GetString();
                            var includeAdvanced = payload.GetProperty("includeAdvanced").GetBoolean();
                            
                            var result = await echoService.GetProcessingOptions(profileType!, includeAdvanced);
                            return JsonSerializer.Serialize(result, JsonOptions);
                        });

                    case ServiceMethodType.UpdateUserStatus:
                        return await ProcessComplexMethodAsync(request, async (payload) =>
                        {
                            var user = JsonSerializer.Deserialize<UserProfile>(payload.GetProperty("user").GetRawText(), JsonOptions);
                            var newStatus = payload.GetProperty("newStatus").GetString();
                            var notifyUser = payload.GetProperty("notifyUser").GetBoolean();
                            var priority = payload.GetProperty("priority").GetInt32();
                            
                            var result = await echoService.UpdateUserStatus(user!, newStatus!, notifyUser, priority);
                            return result.ToString();
                        });

                    case ServiceMethodType.ProcessComplexData:
                        return await ProcessComplexMethodAsync(request, async (payload) =>
                        {
                            var user = JsonSerializer.Deserialize<UserProfile>(payload.GetProperty("user").GetRawText(), JsonOptions);
                            var options = JsonSerializer.Deserialize<ProcessingOptions>(payload.GetProperty("options").GetRawText(), JsonOptions);
                            var operation = payload.GetProperty("operation").GetString();
                            var dryRun = payload.GetProperty("dryRun").GetBoolean();
                            
                            var result = await echoService.ProcessComplexData(user!, options!, operation!, dryRun);
                            return JsonSerializer.Serialize(result, JsonOptions);
                        });

                    default:
                        return new EchoResponse
                        {
                            RequestId = request.RequestId,
                            Success = false,
                            Error = new EchoFault
                            {
                                Text = $"Unknown method: {request.Method}",
                                Reason = "InvalidMethod"
                            }
                        };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing request {Method}", request.Method);
                return new EchoResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Error = new EchoFault
                    {
                        Text = ex.Message,
                        Reason = "ProcessingError"
                    }
                };
            }
        }

        private async Task<EchoResponse> ProcessComplexMethodAsync(EchoRequest request, Func<JsonElement, Task<string>> processor)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Payload))
                {
                    return new EchoResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Error = new EchoFault { Text = "Missing payload", Reason = "InvalidPayload" }
                    };
                }

                var payload = JsonSerializer.Deserialize<JsonElement>(request.Payload, JsonOptions);
                var result = await processor(payload);
                
                return new EchoResponse
                {
                    RequestId = request.RequestId,
                    Success = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing complex method {Method}", request.Method);
                return new EchoResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Error = new EchoFault { Text = ex.Message, Reason = "ComplexMethodError" }
                };
            }
        }
    }
}

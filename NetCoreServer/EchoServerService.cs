using Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;
using System.Collections.Concurrent;

namespace NetCoreServer
{
    public class EchoServerService : BackgroundService
    {
        private readonly ILogger<EchoServerService> logger;
        private readonly IEchoService echoService;
        private readonly ConcurrentDictionary<string, DateTime> activeConnections = new();
        private readonly SemaphoreSlim connectionSemaphore;

        public EchoServerService(ILogger<EchoServerService> logger, IEchoService echoService)
        {
            this.logger = logger;
            this.echoService = echoService;
            this.connectionSemaphore = new SemaphoreSlim(Constants.MaxConcurrentConnections, Constants.MaxConcurrentConnections);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting ZeroMQ Echo Server with 1-to-N support...");

            using var server = new RouterSocket();
            server.Bind(AppConfig.TcpEndpoint);

            logger.LogInformation($"ZeroMQ server listening on {AppConfig.TcpEndpoint}");
            logger.LogInformation($"Maximum concurrent connections: {Constants.MaxConcurrentConnections}");
            logger.LogInformation("Available methods: Echo, ComplexEcho, FailEcho, EchoForPermission, ProcessUserProfile, ValidateUserData, ProcessWithOptions, GetProcessingOptions, UpdateUserStatus, ProcessComplexData");
            logger.LogInformation("Press Ctrl+C to stop the server");

            // Start connection cleanup task
            _ = Task.Run(() => CleanupInactiveConnectionsAsync(stoppingToken), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // RouterSocket receives multipart messages: [identity][empty][data...]
                    var message = new NetMQMessage();
                    if (server.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(Constants.NetworkTimeoutMs), ref message))
                    {
                        logger.LogDebug($"Received {message.FrameCount} frames from client");
                        
                        if (message.FrameCount >= 3) // [identity][empty][request]
                        {
                            var identity = message[0]; // Client identity (binary)
                            var empty = message[1];    // Empty delimiter
                            var requestFrame = message[2]; // Request JSON
                            
                            var requestJson = requestFrame.ConvertToString();
                            logger.LogDebug($"Received request JSON: '{requestJson}'");
                            
                            // Process message asynchronously without blocking
                            _ = Task.Run(async () => await ProcessMessageAsync(server, identity, requestJson, stoppingToken));
                        }
                        else
                        {
                            logger.LogWarning($"Invalid message format - received {message.FrameCount} frames, expected >= 3");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                }
            }

            logger.LogInformation("ZeroMQ server stopping...");
        }

        private async Task ProcessMessageAsync(RouterSocket server, NetMQFrame identity, string requestJson, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(requestJson))
            {
                logger.LogWarning("Received invalid message format");
                return;
            }

            // Generate a temporary client ID for connection tracking
            var clientId = $"temp_{Guid.NewGuid():N}";
            
            logger.LogInformation($"Processing message - ClientId: '{clientId}', RequestJson: '{requestJson}'");

            // Check if we can accept this connection
            if (!await TryAcquireConnectionSlotAsync(clientId))
            {
                logger.LogWarning($"Connection limit reached. Rejecting client {clientId}");
                var errorResponse = CreateErrorResponse(string.Empty, Constants.ServerAtCapacityMessage, Constants.CapacityLimitReason);
                var errorJson = JsonUtilities.SafeSerialize(errorResponse);
                
                server.SendFrame(errorJson);
                return;
            }

            try
            {
                logger.LogDebug($"Received message from client {clientId}: {requestJson}");

                // Validate that requestJson is valid JSON before trying to deserialize
                logger.LogDebug($"Validating JSON: '{requestJson}'");
                if (!JsonUtilities.IsValidJson(requestJson))
                {
                    logger.LogWarning($"Invalid JSON received from client {clientId}: {requestJson}");
                    var errorResponse = CreateErrorResponse(string.Empty, "Invalid JSON format", Constants.InvalidJsonReason);
                    await SendResponseAsync(server, identity, errorResponse);
                    return;
                }

                var request = JsonUtilities.SafeDeserialize<EchoRequest>(requestJson);
                if (request == null)
                {
                    var errorResponse = CreateErrorResponse(string.Empty, Constants.InvalidComplexEchoPayloadMessage, Constants.InvalidJsonReason);
                    await SendResponseAsync(server, identity, errorResponse);
                    return;
                }

                logger.LogInformation($"Processing {request.Method} request from {clientId} (ID: {request.RequestId})");

                // Call interface methods directly based on the method type
                var response = await ProcessRequestDirectly(request);

                // Send response
                await SendResponseAsync(server, identity, response);

                logger.LogDebug($"Sent response to {clientId}: {JsonUtilities.SafeSerialize(response)}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message from {ClientId}", clientId);

                var errorResponse = CreateErrorResponse(string.Empty, ex.Message, Constants.ProcessingErrorReason);
                await SendResponseAsync(server, identity, errorResponse);
            }
            finally
            {
                // Release the connection slot
                ReleaseConnectionSlot(clientId);
            }
        }

        private Task SendResponseAsync(RouterSocket server, NetMQFrame identity, EchoResponse response)
        {
            try
            {
                var responseJson = JsonUtilities.SafeSerialize(response);
                
                // RouterSocket sends: [identity][empty][response]
                var responseMessage = new NetMQMessage();
                responseMessage.Append(identity);           // Client identity for routing
                responseMessage.Append(NetMQFrame.Empty);   // Empty delimiter
                responseMessage.Append(responseJson);       // Response JSON
                
                logger.LogInformation($"Sending response for request {response.RequestId}");
                
                // Ensure thread-safe sending
                lock (server)
                {
                    server.SendMultipartMessage(responseMessage);
                }
                
                logger.LogInformation($"Response sent successfully for request {response.RequestId}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send response for request {RequestId}", response?.RequestId);
            }
            return Task.CompletedTask;
        }

        private async Task<bool> TryAcquireConnectionSlotAsync(string clientId)
        {
            if (await connectionSemaphore.WaitAsync(TimeSpan.FromSeconds(1)))
            {
                activeConnections[clientId] = DateTime.UtcNow;
                logger.LogDebug($"Connection acquired for {clientId}. Active connections: {activeConnections.Count}");
                return true;
            }
            return false;
        }

        private void ReleaseConnectionSlot(string clientId)
        {
            activeConnections.TryRemove(clientId, out _);
            connectionSemaphore.Release();
            logger.LogDebug($"Connection released for {clientId}. Active connections: {activeConnections.Count}");
        }

        private async Task CleanupInactiveConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cutoffTime = DateTime.UtcNow.AddMinutes(-Constants.ConnectionTimeoutMinutes);
                    var inactiveConnections = activeConnections
                        .Where(kvp => kvp.Value < cutoffTime)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var clientId in inactiveConnections)
                    {
                        activeConnections.TryRemove(clientId, out _);
                        connectionSemaphore.Release();
                        logger.LogInformation($"Cleaned up inactive connection: {clientId}");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during connection cleanup");
                }
            }
        }

        // Helper method to create successful responses
        private static EchoResponse CreateSuccessResponse(string requestId, string? result)
        {
            return new EchoResponse
            {
                RequestId = requestId,
                Success = true,
                Result = result
            };
        }

        // Helper method to create error responses
        private static EchoResponse CreateErrorResponse(string requestId, string errorText, string reason)
        {
            return new EchoResponse
            {
                RequestId = requestId,
                Success = false,
                Error = new EchoFault
                {
                    Text = errorText,
                    Reason = reason
                }
            };
        }

        // Helper method to deserialize JSON payload safely
        private T? DeserializePayload<T>(string payload) where T : class
        {
            var result = JsonUtilities.SafeDeserialize<T>(payload);
            if (result == null)
            {
                logger.LogError("Failed to deserialize payload to {Type}", typeof(T).Name);
            }
            return result;
        }

        // Helper method to extract property from JsonElement safely
        private static T? GetPropertyValue<T>(JsonElement payload, string propertyName)
        {
            return JsonUtilities.SafeGetProperty<T?>(payload, propertyName);
        }

        // Helper method to serialize objects to JSON
        private static string SerializeToJson<T>(T obj)
        {
            return JsonUtilities.SafeSerialize(obj);
        }

        // Helper method to convert results to strings safely
        private static string ConvertToString<T>(T value)
        {
            return JsonUtilities.SafeToString(value);
        }

        // Helper method for processing complex methods with object serialization
        private async Task<EchoResponse> ProcessComplexMethodWithSerialization<T>(EchoRequest request, Func<JsonElement, Task<T>> processor)
        {
            return await ProcessComplexMethodAsync(request, async (payload) =>
            {
                var result = await processor(payload);
                return SerializeToJson(result);
            });
        }

        // Helper method for processing complex methods with string conversion
        private async Task<EchoResponse> ProcessComplexMethodWithStringConversion<T>(EchoRequest request, Func<JsonElement, Task<T>> processor)
        {
            return await ProcessComplexMethodAsync(request, async (payload) =>
            {
                var result = await processor(payload);
                return ConvertToString(result);
            });
        }

        private async Task<EchoResponse> ProcessRequestDirectly(EchoRequest request)
        {
            try
            {
                return request.Method switch
                {
                    ServiceMethodType.Echo => await ProcessEchoAsync(request),
                    ServiceMethodType.ComplexEcho => await ProcessComplexEchoAsync(request),
                    ServiceMethodType.FailEcho => await ProcessFailEchoAsync(request),
                    ServiceMethodType.EchoForPermission => await ProcessEchoForPermissionAsync(request),
                    ServiceMethodType.ProcessUserProfile => await ProcessUserProfileMethodAsync(request),
                    ServiceMethodType.ValidateUserData => await ProcessValidateUserDataMethodAsync(request),
                    ServiceMethodType.ProcessWithOptions => await ProcessWithOptionsMethodAsync(request),
                    ServiceMethodType.GetProcessingOptions => await ProcessGetProcessingOptionsMethodAsync(request),
                    ServiceMethodType.UpdateUserStatus => await ProcessUpdateUserStatusMethodAsync(request),
                    ServiceMethodType.ProcessComplexData => await ProcessComplexDataMethodAsync(request),
                    _ => CreateErrorResponse(request.RequestId, $"{Constants.UnknownMethodMessage}: {request.Method}", Constants.InvalidMethodReason)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing request {Method}", request.Method);
                return CreateErrorResponse(request.RequestId, ex.Message, Constants.ProcessingErrorReason);
            }
        }

        private async Task<EchoResponse> ProcessEchoAsync(EchoRequest request)
        {
            var result = await echoService.Echo(request.Payload ?? string.Empty);
            return CreateSuccessResponse(request.RequestId, result);
        }

        private async Task<EchoResponse> ProcessComplexEchoAsync(EchoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Payload))
            {
                return CreateSuccessResponse(request.RequestId, null);
            }

            var message = DeserializePayload<EchoMessage>(request.Payload);
            if (message == null)
            {
                logger.LogError("Failed to deserialize ComplexEcho payload");
                return CreateErrorResponse(request.RequestId, Constants.InvalidComplexEchoPayloadMessage, Constants.InvalidJsonReason);
            }

            var result = await echoService.ComplexEcho(message);
            return CreateSuccessResponse(request.RequestId, result);
        }

        private async Task<EchoResponse> ProcessFailEchoAsync(EchoRequest request)
        {
            try
            {
                var result = await echoService.FailEcho(request.Payload ?? string.Empty);
                return CreateSuccessResponse(request.RequestId, result);
            }
            catch (ServiceException ex)
            {
                return CreateErrorResponse(request.RequestId, ex.Message, ex.Reason);
            }
        }

        private async Task<EchoResponse> ProcessEchoForPermissionAsync(EchoRequest request)
        {
            var result = await echoService.EchoForPermission(request.Payload ?? string.Empty);
            return CreateSuccessResponse(request.RequestId, result);
        }

        private async Task<EchoResponse> ProcessUserProfileMethodAsync(EchoRequest request)
        {
            return await ProcessComplexMethodWithSerialization(request, async (payload) =>
            {
                var user = GetPropertyValue<UserProfile>(payload, "user");
                var operationId = GetPropertyValue<string>(payload, "operationId");
                var validateOnly = GetPropertyValue<bool>(payload, "validateOnly");

                var result = await echoService.ProcessUserProfile(user!, operationId!, validateOnly);
                return result;
            });
        }

        private async Task<EchoResponse> ProcessValidateUserDataMethodAsync(EchoRequest request)
        {
            return await ProcessComplexMethodWithSerialization(request, async (payload) =>
            {
                var user = GetPropertyValue<UserProfile>(payload, "user");
                var strictValidation = GetPropertyValue<bool>(payload, "strictValidation");
                var maxErrors = GetPropertyValue<int>(payload, "maxErrors");

                var result = await echoService.ValidateUserData(user!, strictValidation, maxErrors);
                return result;
            });
        }

        private async Task<EchoResponse> ProcessWithOptionsMethodAsync(EchoRequest request)
        {
            return await ProcessComplexMethodAsync(request, async (payload) =>
            {
                var data = GetPropertyValue<string>(payload, "data");
                var options = GetPropertyValue<ProcessingOptions>(payload, "options");
                var isPriority = GetPropertyValue<bool>(payload, "isPriority");

                var result = await echoService.ProcessWithOptions(data!, options!, isPriority);
                return result;
            });
        }

        private async Task<EchoResponse> ProcessGetProcessingOptionsMethodAsync(EchoRequest request)
        {
            return await ProcessComplexMethodWithSerialization(request, async (payload) =>
            {
                var profileType = GetPropertyValue<string>(payload, "profileType");
                var includeAdvanced = GetPropertyValue<bool>(payload, "includeAdvanced");

                var result = await echoService.GetProcessingOptions(profileType!, includeAdvanced);
                return result;
            });
        }

        private async Task<EchoResponse> ProcessUpdateUserStatusMethodAsync(EchoRequest request)
        {
            return await ProcessComplexMethodWithStringConversion(request, async (payload) =>
            {
                var user = GetPropertyValue<UserProfile>(payload, "user");
                var newStatus = GetPropertyValue<string>(payload, "newStatus");
                var notifyUser = GetPropertyValue<bool>(payload, "notifyUser");
                var priority = GetPropertyValue<int>(payload, "priority");

                var result = await echoService.UpdateUserStatus(user!, newStatus!, notifyUser, priority);
                return result;
            });
        }

        private async Task<EchoResponse> ProcessComplexDataMethodAsync(EchoRequest request)
        {
            return await ProcessComplexMethodWithSerialization(request, async (payload) =>
            {
                var user = GetPropertyValue<UserProfile>(payload, "user");
                var options = GetPropertyValue<ProcessingOptions>(payload, "options");
                var operation = GetPropertyValue<string>(payload, "operation");
                var dryRun = GetPropertyValue<bool>(payload, "dryRun");

                var result = await echoService.ProcessComplexData(user!, options!, operation!, dryRun);
                return result;
            });
        }

        private async Task<EchoResponse> ProcessComplexMethodAsync(EchoRequest request, Func<JsonElement, Task<string>> processor)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Payload))
                {
                    return CreateErrorResponse(request.RequestId, Constants.MissingPayloadMessage, Constants.InvalidPayloadReason);
                }

                var payload = JsonUtilities.SafeDeserialize<JsonElement>(request.Payload);
                var result = await processor(payload);
                
                return CreateSuccessResponse(request.RequestId, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing complex method {Method}", request.Method);
                return CreateErrorResponse(request.RequestId, ex.Message, Constants.ComplexMethodErrorReason);
            }
        }
    }
}

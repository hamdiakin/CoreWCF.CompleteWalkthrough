using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Common;

public class EchoService : IEchoService
{
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ILogger<EchoService> logger;

    public EchoService(ILogger<EchoService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> Echo(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        logger.LogInformation("Received {Text} from client!", text);
        return await Task.FromResult(text);
    }

    public async Task<string?> ComplexEcho(EchoMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        logger.LogInformation("Received {Text} from client!", message.Text);
        return await Task.FromResult(message?.Text);
    }

    public Task<string> FailEcho(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Task.FromException<string>(new ServiceException("Echo failed as requested.", "FailReason"));
    }

    public async Task<string> EchoForPermission(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return await Task.FromResult(text);
    }

    public async Task<EchoResponse> ProcessRequest(EchoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var response = new EchoResponse
            {
                RequestId = request.RequestId,
                Success = true
            };

            var methodMap = new Dictionary<string, Func<string?, Task<string?>>>
            {
                { "echo", async payload => await Echo(payload ?? string.Empty) },
                { "complexecho", async payload =>
                    {
                        if (string.IsNullOrWhiteSpace(payload)) return null;
                        try
                        {
                            var message = JsonSerializer.Deserialize<EchoMessage>(payload, jsonOptions);
                            return await ComplexEcho(message ?? new EchoMessage());
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to deserialize or process ComplexEcho payload.");
                            return null;
                        }
                    }
                },
                { "failecho", async payload => await FailEcho(payload ?? string.Empty) },
                { "echoforpermission", async payload => await EchoForPermission(payload ?? string.Empty) }
            };

            if (methodMap.TryGetValue(request.Method.ToLower(), out var handler))
            {
                response.Result = await handler(request.Payload);
            }
            else
            {
                response.Success = false;
                response.Error = new EchoFault
                {
                    Text = $"Unknown method: {request.Method}",
                    Reason = "InvalidMethod"
                };
            }

            return response;
        }
        catch (ServiceException ex)
        {
            logger.LogWarning(ex, "ServiceException occurred while processing request {RequestId}", request.RequestId);
            return new EchoResponse
            {
                RequestId = request.RequestId,
                Success = false,
                Error = new EchoFault
                {
                    Text = ex.Message,
                    Reason = ex.Reason
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while processing request {RequestId}", request.RequestId);
            return new EchoResponse
            {
                RequestId = request.RequestId,
                Success = false,
                Error = new EchoFault
                {
                    Text = ex.Message,
                    Reason = "UnknownError"
                }
            };
        }
    }
}
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

    public async Task<UserProfile> ProcessUserProfile(UserProfile user, string operationId, bool validateOnly)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(operationId);
        
        logger.LogInformation("Processing user profile for operation {OperationId}, validateOnly: {ValidateOnly}", operationId, validateOnly);
        
        // Simulate processing
        var processedUser = new UserProfile
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Age = user.Age,
            IsActive = validateOnly ? user.IsActive : true,
            Roles = new List<string>(user.Roles) { "Processed" },
            Metadata = new Dictionary<string, object>(user.Metadata)
            {
                ["processedAt"] = DateTime.UtcNow,
                ["operationId"] = operationId,
                ["validateOnly"] = validateOnly
            }
        };
        
        return await Task.FromResult(processedUser);
    }

    public async Task<ValidationResult> ValidateUserData(UserProfile user, bool strictValidation, int maxErrors)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        logger.LogInformation("Validating user data with strictValidation: {StrictValidation}, maxErrors: {MaxErrors}", strictValidation, maxErrors);
        
        var result = new ValidationResult
        {
            IsValid = true,
            ValidationData = new Dictionary<string, object>
            {
                ["validationTimestamp"] = DateTime.UtcNow,
                ["strictValidation"] = strictValidation,
                ["maxErrors"] = maxErrors
            }
        };
        
        // Simulate validation logic
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            result.Errors.Add("Name is required");
            result.IsValid = false;
        }
        
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            result.Errors.Add("Email is required");
            result.IsValid = false;
        }
        
        if (user.Age < 0 || user.Age > 150)
        {
            result.Errors.Add("Age must be between 0 and 150");
            result.IsValid = false;
        }
        
        if (strictValidation && user.Roles.Count == 0)
        {
            result.Warnings.Add("User has no roles assigned");
        }
        
        return await Task.FromResult(result);
    }

    public async Task<string> ProcessWithOptions(string data, ProcessingOptions options, bool isPriority)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(options);
        
        logger.LogInformation("Processing data with options, isPriority: {IsPriority}, mode: {Mode}", isPriority, options.ProcessingMode);
        
        // Simulate processing with options
        var result = $"Processed: {data}";
        if (isPriority)
        {
            result += " [PRIORITY]";
        }
        
        if (options.EnableLogging)
        {
            result += $" [Logged with {options.MaxRetries} max retries]";
        }
        
        return await Task.FromResult(result);
    }

    public async Task<ProcessingOptions> GetProcessingOptions(string profileType, bool includeAdvanced)
    {
        ArgumentNullException.ThrowIfNull(profileType);
        
        logger.LogInformation("Getting processing options for profile type: {ProfileType}, includeAdvanced: {IncludeAdvanced}", profileType, includeAdvanced);
        
        var options = new ProcessingOptions
        {
            EnableLogging = true,
            MaxRetries = profileType.ToLower() == "premium" ? 5 : 3,
            Timeout = TimeSpan.FromSeconds(profileType.ToLower() == "premium" ? 60 : 30),
            ProcessingMode = profileType.ToLower() == "premium" ? "Premium" : "Standard",
            CustomSettings = new Dictionary<string, string>
            {
                ["profileType"] = profileType,
                ["includeAdvanced"] = includeAdvanced.ToString()
            }
        };
        
        if (includeAdvanced)
        {
            options.CustomSettings["advancedFeatures"] = "enabled";
            options.CustomSettings["priority"] = "high";
        }
        
        return await Task.FromResult(options);
    }

    public async Task<bool> UpdateUserStatus(UserProfile user, string newStatus, bool notifyUser, int priority)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(newStatus);
        
        logger.LogInformation("Updating user status to {NewStatus}, notifyUser: {NotifyUser}, priority: {Priority}", newStatus, notifyUser, priority);
        
        // Simulate status update
        var success = !string.IsNullOrWhiteSpace(newStatus) && priority >= 0;
        
        if (success && notifyUser)
        {
            logger.LogInformation("User notification sent for status update to {NewStatus}", newStatus);
        }
        
        return await Task.FromResult(success);
    }

    public async Task<Dictionary<string, object>> ProcessComplexData(UserProfile user, ProcessingOptions options, string operation, bool dryRun)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(operation);
        
        logger.LogInformation("Processing complex data for operation: {Operation}, dryRun: {DryRun}", operation, dryRun);
        
        var result = new Dictionary<string, object>
        {
            ["userId"] = user.Id,
            ["operation"] = operation,
            ["dryRun"] = dryRun,
            ["processedAt"] = DateTime.UtcNow,
            ["processingMode"] = options.ProcessingMode,
            ["maxRetries"] = options.MaxRetries,
            ["userName"] = user.Name,
            ["userEmail"] = user.Email,
            ["userAge"] = user.Age,
            ["userActive"] = user.IsActive,
            ["userRoles"] = user.Roles,
            ["customSettings"] = options.CustomSettings
        };
        
        if (!dryRun)
        {
            result["status"] = "completed";
            result["processingTime"] = DateTime.UtcNow - DateTime.UtcNow.AddSeconds(-1);
        }
        else
        {
            result["status"] = "simulated";
        }
        
        return await Task.FromResult(result);
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

            switch (request.Method)
            {
                case ServiceMethodType.Echo:
                    response.Result = await Echo(request.Payload ?? string.Empty);
                    break;

                case ServiceMethodType.ComplexEcho:
                    if (string.IsNullOrWhiteSpace(request.Payload))
                    {
                        response.Result = null;
                    }
                    else
                    {
                        try
                        {
                            var message = JsonSerializer.Deserialize<EchoMessage>(request.Payload, jsonOptions);
                            response.Result = await ComplexEcho(message ?? new EchoMessage());
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to deserialize or process ComplexEcho payload.");
                            response.Result = null;
                        }
                    }
                    break;

                case ServiceMethodType.FailEcho:
                    try
                    {
                        response.Result = await FailEcho(request.Payload ?? string.Empty);
                    }
                    catch (ServiceException ex)
                    {
                        response.Success = false;
                        response.Error = new EchoFault
                        {
                            Text = ex.Message,
                            Reason = ex.Reason
                        };
                    }
                    break;

                case ServiceMethodType.EchoForPermission:
                    response.Result = await EchoForPermission(request.Payload ?? string.Empty);
                    break;

                default:
                    response.Success = false;
                    response.Error = new EchoFault
                    {
                        Text = $"Unknown method: {request.Method}",
                        Reason = "InvalidMethod"
                    };
                    break;
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
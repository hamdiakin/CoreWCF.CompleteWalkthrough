using Common;
using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;

namespace NetCoreClient
{
    public class EchoClient : IDisposable
    {
        private readonly RequestSocket socket;
        private readonly object lockObject = new object();
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public EchoClient()
        {
            socket = new RequestSocket();
            socket.Connect("tcp://localhost:8088");
        }

        public async Task<string> EchoAsync(string text)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.Echo,
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<string?> ComplexEchoAsync(EchoMessage message)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.ComplexEcho,
                Payload = JsonSerializer.Serialize(message, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            return response.Result;
        }

        public async Task<string> FailEchoAsync(string text)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.FailEcho,
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<string> EchoForPermissionAsync(string text)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.EchoForPermission,
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        // New complex method implementations
        public async Task<UserProfile> ProcessUserProfileAsync(UserProfile user, string operationId, bool validateOnly)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.ProcessUserProfile,
                Payload = JsonSerializer.Serialize(new
                {
                    User = user,
                    OperationId = operationId,
                    ValidateOnly = validateOnly
                }, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            if (response.Result != null)
            {
                return JsonSerializer.Deserialize<UserProfile>(response.Result, JsonOptions) ?? new UserProfile();
            }
            return new UserProfile();
        }

        public async Task<ValidationResult> ValidateUserDataAsync(UserProfile user, bool strictValidation, int maxErrors)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.ValidateUserData,
                Payload = JsonSerializer.Serialize(new
                {
                    User = user,
                    StrictValidation = strictValidation,
                    MaxErrors = maxErrors
                }, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            if (response.Result != null)
            {
                return JsonSerializer.Deserialize<ValidationResult>(response.Result, JsonOptions) ?? new ValidationResult();
            }
            return new ValidationResult();
        }

        public async Task<string> ProcessWithOptionsAsync(string data, ProcessingOptions options, bool isPriority)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.ProcessWithOptions,
                Payload = JsonSerializer.Serialize(new
                {
                    Data = data,
                    Options = options,
                    IsPriority = isPriority
                }, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<ProcessingOptions> GetProcessingOptionsAsync(string profileType, bool includeAdvanced)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.GetProcessingOptions,
                Payload = JsonSerializer.Serialize(new
                {
                    ProfileType = profileType,
                    IncludeAdvanced = includeAdvanced
                }, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            if (response.Result != null)
            {
                return JsonSerializer.Deserialize<ProcessingOptions>(response.Result, JsonOptions) ?? new ProcessingOptions();
            }
            return new ProcessingOptions();
        }

        public async Task<bool> UpdateUserStatusAsync(UserProfile user, string newStatus, bool notifyUser, int priority)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.UpdateUserStatus,
                Payload = JsonSerializer.Serialize(new
                {
                    User = user,
                    NewStatus = newStatus,
                    NotifyUser = notifyUser,
                    Priority = priority
                }, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            return bool.TryParse(response.Result, out var result) && result;
        }

        public async Task<Dictionary<string, object>> ProcessComplexDataAsync(UserProfile user, ProcessingOptions options, string operation, bool dryRun)
        {
            var request = new EchoRequest
            {
                Method = ServiceMethodType.ProcessComplexData,
                Payload = JsonSerializer.Serialize(new
                {
                    User = user,
                    Options = options,
                    Operation = operation,
                    DryRun = dryRun
                }, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            if (response.Result != null)
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(response.Result, JsonOptions) ?? new Dictionary<string, object>();
            }
            return new Dictionary<string, object>();
        }

        private async Task<EchoResponse> SendRequestAsync(EchoRequest request)
        {
            return await Task.Run(() =>
            {
                lock (lockObject)
                {
                    var requestJson = JsonSerializer.Serialize(request, JsonOptions);

                    // Send request
                    socket.SendFrame(requestJson);

                    // Receive response
                    var responseJson = socket.ReceiveFrameString();
                    var response = JsonSerializer.Deserialize<EchoResponse>(responseJson, JsonOptions);

                    if (response == null)
                    {
                        throw new InvalidOperationException("Received null response from server");
                    }

                    if (!response.Success && response.Error != null)
                    {
                        throw new ServiceException(response.Error.Text ?? "Unknown error", response.Error.Reason ?? "Unknown");
                    }

                    return response;
                }
            });
        }

        public void Dispose()
        {
            socket?.Dispose();
        }
    }

}

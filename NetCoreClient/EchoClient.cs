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

        public EchoClient()
        {
            socket = new RequestSocket();
            socket.Connect(Constants.DefaultTcpEndpoint);
        }

        #region Simple Echo Methods

        public async Task<string> EchoAsync(string text)
        {
            var request = CreateSimpleRequest(ServiceMethodType.Echo, text);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        public async Task<string?> ComplexEchoAsync(EchoMessage message)
        {
            var request = CreateComplexRequest(ServiceMethodType.ComplexEcho, message);
            var response = await SendRequestAsync(request);
            return response.Result;
        }

        public async Task<string> FailEchoAsync(string text)
        {
            var request = CreateSimpleRequest(ServiceMethodType.FailEcho, text);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        public async Task<string> EchoForPermissionAsync(string text)
        {
            var request = CreateSimpleRequest(ServiceMethodType.EchoForPermission, text);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        #endregion

        #region Complex Methods

        public async Task<UserProfile> ProcessUserProfileAsync(UserProfile user, string operationId, bool validateOnly)
        {
            var payload = new
            {
                User = user,
                OperationId = operationId,
                ValidateOnly = validateOnly
            };

            var request = CreateComplexRequest(ServiceMethodType.ProcessUserProfile, payload);
            var response = await SendRequestAsync(request);
            return DeserializeResponse<UserProfile>(response);
        }

        public async Task<ValidationResult> ValidateUserDataAsync(UserProfile user, bool strictValidation, int maxErrors)
        {
            var payload = new
            {
                User = user,
                StrictValidation = strictValidation,
                MaxErrors = maxErrors
            };

            var request = CreateComplexRequest(ServiceMethodType.ValidateUserData, payload);
            var response = await SendRequestAsync(request);
            return DeserializeResponse<ValidationResult>(response);
        }

        public async Task<string> ProcessWithOptionsAsync(string data, ProcessingOptions options, bool isPriority)
        {
            var payload = new
            {
                Data = data,
                Options = options,
                IsPriority = isPriority
            };

            var request = CreateComplexRequest(ServiceMethodType.ProcessWithOptions, payload);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        public async Task<ProcessingOptions> GetProcessingOptionsAsync(string profileType, bool includeAdvanced)
        {
            var payload = new
            {
                ProfileType = profileType,
                IncludeAdvanced = includeAdvanced
            };

            var request = CreateComplexRequest(ServiceMethodType.GetProcessingOptions, payload);
            var response = await SendRequestAsync(request);
            return DeserializeResponse<ProcessingOptions>(response);
        }

        public async Task<bool> UpdateUserStatusAsync(UserProfile user, string newStatus, bool notifyUser, int priority)
        {
            var payload = new
            {
                User = user,
                NewStatus = newStatus,
                NotifyUser = notifyUser,
                Priority = priority
            };

            var request = CreateComplexRequest(ServiceMethodType.UpdateUserStatus, payload);
            var response = await SendRequestAsync(request);
            return ParseBooleanResponse(response);
        }

        public async Task<Dictionary<string, object>> ProcessComplexDataAsync(UserProfile user, ProcessingOptions options, string operation, bool dryRun)
        {
            var payload = new
            {
                User = user,
                Options = options,
                Operation = operation,
                DryRun = dryRun
            };

            var request = CreateComplexRequest(ServiceMethodType.ProcessComplexData, payload);
            var response = await SendRequestAsync(request);
            return DeserializeResponse<Dictionary<string, object>>(response);
        }

        #endregion

        #region Helper Methods for Request Creation

        /// <summary>
        /// Creates a simple EchoRequest with string payload
        /// </summary>
        private static EchoRequest CreateSimpleRequest(ServiceMethodType method, string payload)
        {
            return new EchoRequest
            {
                Method = method,
                Payload = payload
            };
        }

        /// <summary>
        /// Creates a complex EchoRequest with JSON-serialized payload
        /// </summary>
        private static EchoRequest CreateComplexRequest(ServiceMethodType method, object payload)
        {
            return new EchoRequest
            {
                Method = method,
                Payload = JsonUtilities.SafeSerialize(payload)
            };
        }

        #endregion

        #region Helper Methods for Response Handling

        /// <summary>
        /// Safely deserializes response result to specified type with fallback to new instance
        /// </summary>
        private static T DeserializeResponse<T>(EchoResponse response) where T : new()
        {
            return JsonUtilities.SafeDeserializeWithNew<T>(response.Result ?? string.Empty);
        }

        /// <summary>
        /// Safely parses boolean response result
        /// </summary>
        private static bool ParseBooleanResponse(EchoResponse response)
        {
            return bool.TryParse(response.Result, out var result) && result;
        }

        /// <summary>
        /// Gets string result with empty string fallback
        /// </summary>
        private static string GetStringResult(EchoResponse response)
        {
            return response.Result ?? string.Empty;
        }

        #endregion

        #region Core Communication

        private async Task<EchoResponse> SendRequestAsync(EchoRequest request)
        {
            return await Task.Run(() =>
            {
                lock (lockObject)
                {
                    var requestJson = JsonUtilities.SafeSerialize(request);

                    // Send request
                    socket.SendFrame(requestJson);

                    // Receive response
                    var responseJson = socket.ReceiveFrameString();
                    var response = JsonUtilities.SafeDeserialize<EchoResponse>(responseJson);

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

        #endregion
    }
}

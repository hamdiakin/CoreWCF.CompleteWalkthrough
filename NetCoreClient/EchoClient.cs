using Common;
using Common.Animals;
using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;
using System.Collections.Concurrent;

namespace NetCoreClient
{
    public class EchoClient : IDisposable
    {
        private readonly DealerSocket socket;
        private readonly object lockObject = new object();
        private readonly Timer healthCheckTimer;
        private volatile bool isConnected = false;

        public EchoClient()
        {

            socket = new DealerSocket();
            socket.Connect(Constants.DefaultTcpEndpoint);
            
            // Using synchronous request-response pattern
            
            // Start health check timer
            healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(Constants.HealthCheckIntervalSeconds), TimeSpan.FromSeconds(Constants.HealthCheckIntervalSeconds));
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

        #region Animal Methods

        /// <summary>
        /// Processes any animal type polymorphically
        /// </summary>
        public async Task<Animal?> ProcessAnimalAsync(Animal animal)
        {
            var request = CreateComplexRequest(ServiceMethodType.ProcessAnimal, animal);
            var response = await SendRequestAsync(request);
            
            if (response.Success && !string.IsNullOrWhiteSpace(response.Result))
            {
                return JsonUtilities.SafeDeserializePolymorphic<Animal>(response.Result);
            }
            
            return null;
        }

        /// <summary>
        /// Gets information about any animal
        /// </summary>
        public async Task<string> GetAnimalInfoAsync(Animal animal)
        {
            var request = CreateComplexRequest(ServiceMethodType.GetAnimalInfo, animal);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        /// <summary>
        /// Makes an animal sound
        /// </summary>
        public async Task<string> MakeAnimalSoundAsync(Animal animal)
        {
            var request = CreateComplexRequest(ServiceMethodType.MakeAnimalSound, animal);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        /// <summary>
        /// Processes a group of animals
        /// </summary>
        public async Task<Dictionary<string, object>> ProcessAnimalGroupAsync(List<Animal> animals)
        {
            var request = CreateComplexRequest(ServiceMethodType.ProcessAnimalGroup, animals);
            var response = await SendRequestAsync(request);
            return DeserializeResponse<Dictionary<string, object>>(response);
        }
        
        /// <summary>
        /// Gets all animals from the server
        /// </summary>
        public async Task<List<Animal>> GetAllAnimalsAsync()
        {
            var request = CreateSimpleRequest(ServiceMethodType.GetAllAnimals, string.Empty);
            var response = await SendRequestAsync(request);
            return DeserializeResponse<List<Animal>>(response);
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

                    // DealerSocket sends: [empty][request] to RouterSocket
                    var requestMessage = new NetMQMessage();
                    requestMessage.Append(NetMQFrame.Empty);  // Empty delimiter
                    requestMessage.Append(requestJson);       // Request JSON
                    
                    socket.SendMultipartMessage(requestMessage);

                    // DealerSocket receives response from RouterSocket
                    var responseMessage = socket.ReceiveMultipartMessage();
                    
                    // Find the last non-empty frame (which should contain the JSON response)
                    string responseJson = "";
                    for (int i = responseMessage.FrameCount - 1; i >= 0; i--)
                    {
                        var frameContent = responseMessage[i].ConvertToString();
                        if (!string.IsNullOrEmpty(frameContent))
                        {
                            responseJson = frameContent;
                            break;
                        }
                    }
                    
                    if (string.IsNullOrEmpty(responseJson))
                    {
                        throw new InvalidOperationException("No valid JSON response found in message");
                    }
                    
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



        private void HealthCheckCallback(object? state)
        {
            try
            {
                // Send a simple ping to check connection health
                var pingRequest = CreateSimpleRequest(ServiceMethodType.Echo, "ping");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendRequestAsync(pingRequest);
                        isConnected = true;
                    }
                    catch
                    {
                        isConnected = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Health check failed: {ex.Message}");
                isConnected = false;
            }
        }

        public bool IsConnected => isConnected;

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                healthCheckTimer?.Dispose();
                socket?.Dispose();
            }
        }

        #endregion
    }
}

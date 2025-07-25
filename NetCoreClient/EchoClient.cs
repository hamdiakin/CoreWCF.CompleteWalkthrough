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
                Method = "Echo",
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<string?> ComplexEchoAsync(EchoMessage message)
        {
            var request = new EchoRequest
            {
                Method = "ComplexEcho",
                Payload = JsonSerializer.Serialize(message, JsonOptions)
            };

            var response = await SendRequestAsync(request);
            return response.Result;
        }

        public async Task<string> FailEchoAsync(string text)
        {
            var request = new EchoRequest
            {
                Method = "FailEcho",
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<string> EchoForPermissionAsync(string text)
        {
            var request = new EchoRequest
            {
                Method = "EchoForPermission",
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
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

using Common;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace NetCoreClient
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "ZeroMQ .NET Client";

            try
            {
                using var client = new ZmqEchoClient();

                Console.WriteLine("Testing ZeroMQ Echo Service...\n");

                // Test simple Echo
                await TestEchoAsync(client);

                // Test ComplexEcho
                await TestComplexEchoAsync(client);

                // Test FailEcho (fault handling)
                await TestFailEchoAsync(client);

                // Test EchoForPermission
                await TestEchoForPermissionAsync(client);

                Console.WriteLine("\nAll tests completed. Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static async Task TestEchoAsync(ZmqEchoClient client)
        {
            Console.WriteLine("Testing Echo method...");
            try
            {
                var result = await client.EchoAsync("Hello World from ZeroMQ!");
                Console.WriteLine($"Response: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Echo failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static async Task TestComplexEchoAsync(ZmqEchoClient client)
        {
            Console.WriteLine("Testing ComplexEcho method...");
            try
            {
                var message = new EchoMessage { Text = "Complex message from ZeroMQ!" };
                var result = await client.ComplexEchoAsync(message);
                Console.WriteLine($"Response: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ComplexEcho failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static async Task TestFailEchoAsync(ZmqEchoClient client)
        {
            Console.WriteLine("Testing FailEcho method (should throw exception)...");
            try
            {
                var result = await client.FailEchoAsync("This should fail");
                Console.WriteLine($"Unexpected success: {result}");
            }
            catch (ZmqServiceException ex)
            {
                Console.WriteLine($"Expected exception caught: {ex.Message} (Reason: {ex.Reason})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static async Task TestEchoForPermissionAsync(ZmqEchoClient client)
        {
            Console.WriteLine("Testing EchoForPermission method...");
            try
            {
                var result = await client.EchoForPermissionAsync("Permission test message");
                Console.WriteLine($"Response: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EchoForPermission failed: {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    public class ZmqEchoClient : IDisposable
    {
        private readonly RequestSocket _socket;
        private readonly object _lockObject = new object();

        public ZmqEchoClient()
        {
            _socket = new RequestSocket();
            _socket.Connect("tcp://localhost:8088");
        }

        public async Task<string> EchoAsync(string text)
        {
            var request = new ZmqRequest
            {
                Method = "Echo",
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<string?> ComplexEchoAsync(EchoMessage message)
        {
            var request = new ZmqRequest
            {
                Method = "ComplexEcho",
                Payload = JsonConvert.SerializeObject(message)
            };

            var response = await SendRequestAsync(request);
            return response.Result;
        }

        public async Task<string> FailEchoAsync(string text)
        {
            var request = new ZmqRequest
            {
                Method = "FailEcho",
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        public async Task<string> EchoForPermissionAsync(string text)
        {
            var request = new ZmqRequest
            {
                Method = "EchoForPermission",
                Payload = text
            };

            var response = await SendRequestAsync(request);
            return response.Result ?? string.Empty;
        }

        private async Task<ZmqResponse> SendRequestAsync(ZmqRequest request)
        {
            return await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    var requestJson = JsonConvert.SerializeObject(request);

                    // Send request
                    _socket.SendFrame(requestJson);

                    // Receive response
                    var responseJson = _socket.ReceiveFrameString();
                    var response = JsonConvert.DeserializeObject<ZmqResponse>(responseJson);

                    if (response == null)
                    {
                        throw new InvalidOperationException("Received null response from server");
                    }

                    if (!response.Success && response.Error != null)
                    {
                        throw new ZmqServiceException(response.Error.Text ?? "Unknown error", response.Error.Reason ?? "Unknown");
                    }

                    return response;
                }
            });
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }

    public class ZmqServiceException : Exception
    {
        public string Reason { get; }

        public ZmqServiceException(string message, string reason) : base(message)
        {
            Reason = reason;
        }
    }
}
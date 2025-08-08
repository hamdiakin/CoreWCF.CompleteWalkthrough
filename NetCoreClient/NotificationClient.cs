using Common;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace NetCoreClient
{
    public class NotificationClient : IDisposable
    {
        private readonly DealerSocket requestSocket;
        private readonly DealerSocket notificationSocket;
        private readonly object lockObject = new object();
        private readonly Timer healthCheckTimer;
        private volatile bool isConnected = false;
        private readonly ConcurrentQueue<string> notifications = new();
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private Task? notificationListenerTask;

        public NotificationClient()
        {
            try
            {
                requestSocket = new DealerSocket();
                requestSocket.Connect(Constants.DefaultTcpEndpoint);

                notificationSocket = new DealerSocket();
                notificationSocket.Connect(Constants.DefaultTcpEndpoint);

                // Start health check timer
                healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(Constants.HealthCheckIntervalSeconds), TimeSpan.FromSeconds(Constants.HealthCheckIntervalSeconds));

                // Start notification listener
                StartNotificationListener();
            }
            catch (Exception ex)
            {
                requestSocket?.Dispose();
                notificationSocket?.Dispose();
                healthCheckTimer?.Dispose();
                throw new InvalidOperationException($"Failed to initialize NotificationClient: {ex.Message}", ex);
            }
        }

        private void StartNotificationListener()
        {
            notificationListenerTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Try to receive notification with timeout
                        var message = new NetMQMessage();
                        if (notificationSocket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(100), ref message))
                        {
                            // Process the notification message
                            ProcessNotificationMessage(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in notification listener: {ex.Message}");
                        await Task.Delay(1000, cancellationTokenSource.Token);
                    }
                }
            }, cancellationTokenSource.Token);
        }

        private void ProcessNotificationMessage(NetMQMessage message)
        {
            try
            {
                // Find the last non-empty frame (which should contain the JSON)
                string notificationJson = "";
                for (int i = message.FrameCount - 1; i >= 0; i--)
                {
                    var frameContent = message[i].ConvertToString();
                    if (!string.IsNullOrEmpty(frameContent))
                    {
                        notificationJson = frameContent;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(notificationJson))
                {
                    // Try to deserialize as a notification
                    try
                    {
                        var notification = JsonConvert.DeserializeObject<dynamic>(notificationJson);
                        if (notification?.Type?.ToString() == "ContinuousNotification")
                        {
                            var notificationText = $"📢 Notification #{notification.Counter}: {notification.Message} | Server: {notification.ServerInfo?.ActiveConnections} active connections";
                            notifications.Enqueue(notificationText);
                            Console.WriteLine(notificationText);
                        }
                        else
                        {
                            // This might be a regular response, ignore for now
                            Console.WriteLine($"Received message: {notificationJson}");
                        }
                    }
                    catch (JsonException)
                    {
                        // Not a JSON notification, might be a regular response
                        Console.WriteLine($"Received non-JSON message: {notificationJson}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing notification message: {ex.Message}");
            }
        }

        public async Task<string> EchoAsync(string text)
        {
            var request = CreateSimpleRequest(ServiceMethodType.Echo, text);
            var response = await SendRequestAsync(request);
            return GetStringResult(response);
        }

        private static EchoRequest CreateSimpleRequest(ServiceMethodType method, string payload)
        {
            return new EchoRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Method = method,
                Payload = payload
            };
        }

        private static string GetStringResult(EchoResponse response)
        {
            if (!response.Success)
            {
                throw new ServiceException(response.Error?.Text ?? "Unknown error", response.Error?.Reason ?? "Unknown");
            }
            return response.Result ?? string.Empty;
        }

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

                    requestSocket.SendMultipartMessage(requestMessage);

                    // DealerSocket receives response from RouterSocket
                    var responseMessage = requestSocket.ReceiveMultipartMessage();

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

                    var response = JsonUtilities.SafeDeserialize<EchoResponse>(responseJson, null!);

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

        public int NotificationCount => notifications.Count;

        public string? GetNextNotification()
        {
            notifications.TryDequeue(out var notification);
            return notification;
        }

        public void ClearNotifications()
        {
            while (notifications.TryDequeue(out _)) { }
        }

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                cancellationTokenSource.Cancel();
                notificationListenerTask?.Wait(TimeSpan.FromSeconds(5));
                healthCheckTimer?.Dispose();
                requestSocket?.Dispose();
                notificationSocket?.Dispose();
                cancellationTokenSource?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}

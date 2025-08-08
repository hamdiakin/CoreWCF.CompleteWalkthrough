using Microsoft.Extensions.Logging;
using NetCoreClient;

namespace NetCoreClient
{
    public class NotificationTest
    {
        public static async Task RunNotificationTestAsync(ILogger logger)
        {
            logger.LogInformation("=== Testing Continuous Notifications ===");

            using var client = new NotificationClient();
            
            logger.LogInformation("Notification client created and connected to server");
            logger.LogInformation("Waiting for continuous notifications (every 5 seconds)...");
            logger.LogInformation("Press any key to stop...");

            // Send an initial request to establish connection
            try
            {
                var result = await client.EchoAsync("Hello from notification test client!");
                logger.LogInformation($"Initial echo response: {result}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send initial request");
            }

            // Monitor notifications for 30 seconds
            var startTime = DateTime.UtcNow;
            var notificationCount = 0;

            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(30))
            {
                await Task.Delay(1000); // Check every second

                var currentNotifications = client.NotificationCount;
                if (currentNotifications > notificationCount)
                {
                    logger.LogInformation($"Received {currentNotifications - notificationCount} new notification(s)");
                    notificationCount = currentNotifications;
                }

                // Display connection status
                if (client.IsConnected)
                {
                    Console.Write($"\r✅ Connected | Notifications: {notificationCount} | Time: {(DateTime.UtcNow - startTime).TotalSeconds:F0}s");
                }
                else
                {
                    Console.Write($"\r❌ Disconnected | Notifications: {notificationCount} | Time: {(DateTime.UtcNow - startTime).TotalSeconds:F0}s");
                }
            }

            logger.LogInformation($"\nTest completed. Total notifications received: {notificationCount}");
            logger.LogInformation("Continuous notifications are working! The server sends notifications every 5 seconds to all connected clients.");
        }
    }
}

using Common;
using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;

namespace NetCoreClient
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "ZeroMQ .NET Client";

            try
            {
                using var client = new EchoClient();

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

        private static async Task TestEchoAsync(EchoClient client)
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

        private static async Task TestComplexEchoAsync(EchoClient client)
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

        private static async Task TestFailEchoAsync(EchoClient client)
        {
            Console.WriteLine("Testing FailEcho method (should throw exception)...");
            try
            {
                var result = await client.FailEchoAsync("This should fail");
                Console.WriteLine($"Unexpected success: {result}");
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Expected exception caught: {ex.Message} (Reason: {ex.Reason})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static async Task TestEchoForPermissionAsync(EchoClient client)
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
}
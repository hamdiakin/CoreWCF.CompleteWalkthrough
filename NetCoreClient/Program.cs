using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetCoreClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Title = "ZeroMQ .NET Client";

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<EchoClient>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var client = host.Services.GetRequiredService<EchoClient>();

            try
            {
                logger.LogInformation("Testing ZeroMQ Echo Service with Complex Methods...");

                // Test original methods
                await TestEchoAsync(client, logger);
                await TestComplexEchoAsync(client, logger);
                await TestFailEchoAsync(client, logger);
                await TestEchoForPermissionAsync(client, logger);

                // Test new complex methods
                await TestProcessUserProfileAsync(client, logger);
                await TestValidateUserDataAsync(client, logger);
                await TestProcessWithOptionsAsync(client, logger);
                await TestGetProcessingOptionsAsync(client, logger);
                await TestUpdateUserStatusAsync(client, logger);
                await TestProcessComplexDataAsync(client, logger);

                logger.LogInformation("All tests completed successfully!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during client execution");
            }
        }

        private static async Task TestEchoAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing Echo method ===");
            try
            {
                var result = await client.EchoAsync("Hello World from ZeroMQ!");
                logger.LogInformation("Response: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Echo failed");
            }
        }

        private static async Task TestComplexEchoAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing ComplexEcho method ===");
            try
            {
                var message = TestDataFactory.CreateTestEchoMessage();
                var result = await client.ComplexEchoAsync(message);
                logger.LogInformation("Response: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ComplexEcho failed");
            }
        }

        private static async Task TestFailEchoAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing FailEcho method (should throw exception) ===");
            try
            {
                var result = await client.FailEchoAsync("This should fail");
                logger.LogWarning("Unexpected success: {Result}", result);
            }
            catch (ServiceException ex)
            {
                logger.LogInformation("Expected exception caught: {Message} (Reason: {Reason})", ex.Message, ex.Reason);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception");
            }
        }

        private static async Task TestEchoForPermissionAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing EchoForPermission method ===");
            try
            {
                var result = await client.EchoForPermissionAsync("Permission test message");
                logger.LogInformation("Response: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EchoForPermission failed");
            }
        }

        // New complex method tests
        private static async Task TestProcessUserProfileAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing ProcessUserProfile method ===");
            try
            {
                var user = TestDataFactory.CreateTestUser1();
                var result = await client.ProcessUserProfileAsync(user, Constants.UpdateProfileOperation, false);
                logger.LogInformation("Processed User Profile - ID: {Id}, Name: {Name}, Roles: {Roles}", 
                    result.Id, result.Name, string.Join(", ", result.Roles));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProcessUserProfile failed");
            }
        }

        private static async Task TestValidateUserDataAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing ValidateUserData method ===");
            try
            {
                var user = new UserProfile
                {
                    Id = Constants.TestUserId456,
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Age = 25,
                    IsActive = true,
                    Roles = new List<string> { "User" },
                    Metadata = new Dictionary<string, object>()
                };

                var result = await client.ValidateUserDataAsync(user, true, 5);
                logger.LogInformation("Validation Result - IsValid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
                    result.IsValid, result.Errors.Count, result.Warnings.Count);
                
                if (result.Errors.Any())
                {
                    logger.LogWarning("Validation Errors: {Errors}", string.Join(", ", result.Errors));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ValidateUserData failed");
            }
        }

        private static async Task TestProcessWithOptionsAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing ProcessWithOptions method ===");
            try
            {
                var options = new ProcessingOptions
                {
                    EnableLogging = true,
                    MaxRetries = 3,
                    Timeout = TimeSpan.FromSeconds(30),
                    ProcessingMode = "Standard",
                    CustomSettings = new Dictionary<string, string>
                    {
                        ["priority"] = "normal",
                        ["batchSize"] = "100"
                    }
                };

                var result = await client.ProcessWithOptionsAsync("Sample data to process", options, true);
                logger.LogInformation("Processed with options: {Result}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProcessWithOptions failed");
            }
        }

        private static async Task TestGetProcessingOptionsAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing GetProcessingOptions method ===");
            try
            {
                var result = await client.GetProcessingOptionsAsync("Premium", true);
                logger.LogInformation("Processing Options - Mode: {Mode}, MaxRetries: {MaxRetries}, Timeout: {Timeout}s", 
                    result.ProcessingMode, result.MaxRetries, result.Timeout.TotalSeconds);
                
                logger.LogInformation("Custom Settings: {Settings}", 
                    string.Join(", ", result.CustomSettings.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetProcessingOptions failed");
            }
        }

        private static async Task TestUpdateUserStatusAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing UpdateUserStatus method ===");
            try
            {
                var user = new UserProfile
                {
                    Id = "user789",
                    Name = "Bob Wilson",
                    Email = "bob.wilson@example.com",
                    Age = 35,
                    IsActive = false,
                    Roles = new List<string> { "User", "Admin" },
                    Metadata = new Dictionary<string, object>()
                };

                var result = await client.UpdateUserStatusAsync(user, "ACTIVE", true, 1);
                logger.LogInformation("Status update result: {Success}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UpdateUserStatus failed");
            }
        }

        private static async Task TestProcessComplexDataAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing ProcessComplexData method ===");
            try
            {
                var user = new UserProfile
                {
                    Id = "user999",
                    Name = "Alice Johnson",
                    Email = "alice.johnson@example.com",
                    Age = 28,
                    IsActive = true,
                    Roles = new List<string> { "User", "Manager" },
                    Metadata = new Dictionary<string, object>
                    {
                        ["team"] = "Marketing",
                        ["level"] = "Senior"
                    }
                };

                var options = new ProcessingOptions
                {
                    EnableLogging = true,
                    MaxRetries = 5,
                    Timeout = TimeSpan.FromSeconds(60),
                    ProcessingMode = "Premium",
                    CustomSettings = new Dictionary<string, string>
                    {
                        ["priority"] = "high",
                        ["batchSize"] = "50"
                    }
                };

                var result = await client.ProcessComplexDataAsync(user, options, "DATA_MIGRATION", false);
                logger.LogInformation("Complex data processing completed - Status: {Status}, User: {UserName}, Operation: {Operation}", 
                    result.GetValueOrDefault("status"), result.GetValueOrDefault("userName"), result.GetValueOrDefault("operation"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProcessComplexData failed");
            }
        }
    }
}
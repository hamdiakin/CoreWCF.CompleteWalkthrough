using Common;
using Common.Animals;
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

                //// Test original methods
                //await TestEchoAsync(client, logger);
                //await TestComplexEchoAsync(client, logger);
                //await TestFailEchoAsync(client, logger);
                //await TestEchoForPermissionAsync(client, logger);

                //// Test new complex methods
                //await TestProcessUserProfileAsync(client, logger);
                //await TestValidateUserDataAsync(client, logger);
                //await TestProcessWithOptionsAsync(client, logger);
                //await TestGetProcessingOptionsAsync(client, logger);
                //await TestUpdateUserStatusAsync(client, logger);
                //await TestProcessComplexDataAsync(client, logger);

                // Test polymorphic animals
                using var animalTestClient = new EchoClient();
                await TestPolymorphicAnimalsAsync(animalTestClient, logger);
                
                // Test getting all animals
                await TestGetAllAnimalsAsync(animalTestClient, logger);
                
                // Test multiple concurrent clients
                await TestMultipleClientsAsync(logger);

                logger.LogInformation("All tests completed successfully!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during client execution");
            }
        }

        private static async Task TestMultipleClientsAsync(ILogger logger)
        {
            logger.LogInformation("=== Testing Multiple Concurrent Clients ===");
            
            var tasks = new List<Task>();
            var clients = new List<EchoClient>();
            
            try
            {
                // Create multiple clients
                for (int i = 0; i < 5; i++)
                {
                    var client = new EchoClient();
                    clients.Add(client);
                    
                    var clientId = i + 1;
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            logger.LogInformation($"Client {clientId} starting...");
                            var result = await client.EchoAsync($"Hello from client {clientId}!");
                            logger.LogInformation($"Client {clientId} received: {result}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Client {clientId} failed");
                        }
                    });
                    
                    tasks.Add(task);
                }
                
                // Wait for all clients to complete
                await Task.WhenAll(tasks);
                logger.LogInformation("All concurrent clients completed successfully!");
            }
            finally
            {
                // Dispose all clients
                foreach (var client in clients)
                {
                    client.Dispose();
                }
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

        /// <summary>
        /// Demonstrates polymorphic animal processing
        /// </summary>
        private static async Task TestPolymorphicAnimalsAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing Polymorphic Animals ===");

            try
            {
                // Create different types of animals
                var animals = new List<Animal>
                {
                    new Dog
                    {
                        Name = "Buddy",
                        Age = 3,
                        Species = "Golden Retriever",
                        Type = AnimalType.Dog,
                        Breed = "Golden Retriever",
                        IsGoodBoy = true
                    },
                    new Cat
                    {
                        Name = "Whiskers",
                        Age = 2,
                        Species = "Persian Cat",
                        Type = AnimalType.Cat,
                        Color = "Orange",
                        IsIndoor = true,
                        LivesRemaining = 9
                    },
                    new Bird
                    {
                        Name = "Tweety",
                        Age = 1,
                        Species = "Canary",
                        Type = AnimalType.Bird,
                        WingSpan = 15.5,
                        CanFly = true,
                        FeatherColor = "Yellow"
                    },
                    new Mouse
                    {
                        Name = "Jerry",
                        Age = 1,
                        Species = "House Mouse",
                        Type = AnimalType.Mouse,
                        Size = 8.5,
                        IsNocturnal = true,
                        FavoriteFood = "Cheese"
                    }
                };

                // Test individual animal processing
                logger.LogInformation("--- Processing Individual Animals ---");
                foreach (var animal in animals)
                {
                    logger.LogInformation("Processing {AnimalType}: {Name}", animal.GetType().Name, animal.Name);

                    // Test polymorphic serialization/deserialization
                    var processedAnimal = await client.ProcessAnimalAsync(animal);
                    if (processedAnimal != null)
                    {
                        logger.LogInformation("✅ Processed: {Info}", processedAnimal.GetInfo());
                    }

                    // Test getting animal info
                    var info = await client.GetAnimalInfoAsync(animal);
                    logger.LogInformation("ℹ️ Info: {Info}", info);

                    // Test making sounds
                    var sound = await client.MakeAnimalSoundAsync(animal);
                    logger.LogInformation("🔊 Sound: {Sound}", sound);

                    logger.LogInformation("");
                }

                // Test group processing
                logger.LogInformation("--- Processing Animal Group ---");
                var groupResult = await client.ProcessAnimalGroupAsync(animals);
                
                logger.LogInformation("📊 Group Results:");
                logger.LogInformation("   Total Animals: {Total}", groupResult.GetValueOrDefault("totalAnimals"));
                
                if (groupResult.TryGetValue("animalTypes", out var typesObj) && typesObj is Dictionary<string, object> types)
                {
                    logger.LogInformation("   Animal Types:");
                    foreach (var kvp in types)
                    {
                        logger.LogInformation("     - {Type}: {Count}", kvp.Key, kvp.Value);
                    }
                }

                if (groupResult.TryGetValue("sounds", out var soundsObj) && soundsObj is List<object> sounds)
                {
                    logger.LogInformation("   All Sounds:");
                    foreach (var sound in sounds)
                    {
                        logger.LogInformation("     🔊 {Sound}", sound);
                    }
                }

                logger.LogInformation("✅ Polymorphic animal processing completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Polymorphic animal processing failed");
            }
        }

        private static async Task TestGetAllAnimalsAsync(EchoClient client, ILogger logger)
        {
            logger.LogInformation("=== Testing GetAllAnimals method ===");
            try
            {
                var animals = await client.GetAllAnimalsAsync();
                logger.LogInformation("Retrieved {Count} animals from the server:", animals.Count);
                foreach (var animal in animals)
                {
                    logger.LogInformation(" - {Name} ({Type})", animal.Name, animal.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetAllAnimals failed");
            }
        }
    }
}
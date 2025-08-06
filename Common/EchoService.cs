using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Animals;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Common
{
    public class EchoService : IEchoService
    {
        private static readonly JsonSerializerSettings jsonSettings =
            JsonUtilities.StandardSettings;
        private readonly ILogger<EchoService> logger;

        public EchoService(ILogger<EchoService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Echo(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            logger.LogInformation("Received {Text} from client!", text);
            return await Task.FromResult(text);
        }

        public async Task<string?> ComplexEcho(EchoMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            logger.LogInformation("Received {Text} from client!", message.Text);
            return await Task.FromResult(message.Text);
        }

        public Task<string> FailEcho(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            return Task.FromException<string>(
                new ServiceException(Constants.EchoFailedMessage, Constants.FailReason)
            );
        }

        public async Task<string> EchoForPermission(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            return await Task.FromResult(text);
        }

        public async Task<UserProfile> ProcessUserProfile(
            UserProfile user,
            string operationId,
            bool validateOnly
        )
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(operationId);

            logger.LogInformation(
                "Processing user profile for operation {OperationId}, validateOnly: {ValidateOnly}",
                operationId,
                validateOnly
            );

            // Simulate processing
            var processedUser = new UserProfile
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                IsActive = validateOnly ? user.IsActive : true,
                Roles = new List<string>(user.Roles) { "Processed" },
                Metadata = new Dictionary<string, object>(user.Metadata)
                {
                    ["processedAt"] = DateTime.UtcNow,
                    ["operationId"] = operationId,
                    ["validateOnly"] = validateOnly,
                },
            };

            return await Task.FromResult(processedUser);
        }

        public async Task<ValidationResult> ValidateUserData(
            UserProfile user,
            bool strictValidation,
            int maxErrors
        )
        {
            ArgumentNullException.ThrowIfNull(user);

            logger.LogInformation(
                "Validating user data with strictValidation: {StrictValidation}, maxErrors: {MaxErrors}",
                strictValidation,
                maxErrors
            );

            var result = new ValidationResult
            {
                IsValid = true,
                ValidationData = new Dictionary<string, object>
                {
                    ["validationTimestamp"] = DateTime.UtcNow,
                    ["strictValidation"] = strictValidation,
                    ["maxErrors"] = maxErrors,
                },
            };

            // Simulate validation logic
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                result.Errors.Add("Name is required");
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                result.Errors.Add("Email is required");
                result.IsValid = false;
            }

            if (user.Age < Constants.MinUserAge || user.Age > Constants.MaxUserAge)
            {
                result.Errors.Add(Constants.AgeRangeMessage);
                result.IsValid = false;
            }

            if (strictValidation && user.Roles.Count == 0)
            {
                result.Warnings.Add(Constants.NoRolesWarningMessage);
            }

            return await Task.FromResult(result);
        }

        public async Task<string> ProcessWithOptions(
            string data,
            ProcessingOptions options,
            bool isPriority
        )
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(options);

            logger.LogInformation(
                "Processing data with options, isPriority: {IsPriority}, mode: {Mode}",
                isPriority,
                options.ProcessingMode
            );

            // Simulate processing with options
            var result = $"Processed: {data}";
            if (isPriority)
            {
                result += " [PRIORITY]";
            }

            if (options.EnableLogging)
            {
                result += $" [Logged with {options.MaxRetries} max retries]";
            }

            return await Task.FromResult(result);
        }

        public async Task<ProcessingOptions> GetProcessingOptions(
            string profileType,
            bool includeAdvanced
        )
        {
            ArgumentNullException.ThrowIfNull(profileType);

            logger.LogInformation(
                "Getting processing options for profile type: {ProfileType}, includeAdvanced: {IncludeAdvanced}",
                profileType,
                includeAdvanced
            );

            var isPremium = profileType.ToLower() == Constants.PremiumProfileType;
            var options = new ProcessingOptions
            {
                EnableLogging = true,
                MaxRetries = isPremium ? Constants.PremiumMaxRetries : Constants.StandardMaxRetries,
                Timeout = TimeSpan.FromSeconds(
                    isPremium ? Constants.PremiumTimeoutSeconds : Constants.StandardTimeoutSeconds
                ),
                ProcessingMode = isPremium ? Constants.PremiumMode : Constants.StandardMode,
                CustomSettings = new Dictionary<string, string>
                {
                    [Constants.ProfileTypeKey] = profileType,
                    [Constants.IncludeAdvancedKey] = includeAdvanced.ToString(),
                },
            };

            if (includeAdvanced)
            {
                options.CustomSettings[Constants.AdvancedFeaturesKey] =
                    Constants.AdvancedFeaturesEnabled;
                options.CustomSettings[Constants.PriorityKey] = Constants.HighPriority;
            }

            return await Task.FromResult(options);
        }

        public async Task<bool> UpdateUserStatus(
            UserProfile user,
            string newStatus,
            bool notifyUser,
            int priority
        )
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(newStatus);

            logger.LogInformation(
                "Updating user status to {NewStatus}, notifyUser: {NotifyUser}, priority: {Priority}",
                newStatus,
                notifyUser,
                priority
            );

            // Simulate status update
            var success = !string.IsNullOrWhiteSpace(newStatus) && priority >= 0;

            if (success && notifyUser)
            {
                logger.LogInformation(
                    "User notification sent for status update to {NewStatus}",
                    newStatus
                );
            }

            return await Task.FromResult(success);
        }

        public async Task<Dictionary<string, object>> ProcessComplexData(
            UserProfile user,
            ProcessingOptions options,
            string operation,
            bool dryRun
        )
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(operation);

            logger.LogInformation(
                "Processing complex data for operation: {Operation}, dryRun: {DryRun}",
                operation,
                dryRun
            );

            var result = new Dictionary<string, object>
            {
                ["userId"] = user.Id,
                ["operation"] = operation,
                ["dryRun"] = dryRun,
                ["processedAt"] = DateTime.UtcNow,
                ["processingMode"] = options.ProcessingMode,
                ["maxRetries"] = options.MaxRetries,
                ["userName"] = user.Name,
                ["userEmail"] = user.Email,
                ["userAge"] = user.Age,
                ["userActive"] = user.IsActive,
                ["userRoles"] = user.Roles,
                ["customSettings"] = options.CustomSettings,
            };

            if (!dryRun)
            {
                result["status"] = "completed";
                result["processingTime"] = DateTime.UtcNow - DateTime.UtcNow.AddSeconds(-1);
            }
            else
            {
                result["status"] = "simulated";
            }

            return await Task.FromResult(result);
        }

        #region Animal Processing Methods

        /// <summary>
        /// Processes any animal polymorphically - demonstrates polymorphic serialization
        /// </summary>
        public async Task<Animal> ProcessAnimal(Animal animal)
        {
            ArgumentNullException.ThrowIfNull(animal);

            logger.LogInformation(
                "Processing {AnimalType}: {Name}",
                animal.GetType().Name,
                animal.Name
            );

            // Simulate processing - add some metadata based on animal type
            switch (animal)
            {
                case Dog dog:
                    dog.IsGoodBoy = true;
                    logger.LogInformation("Dog {Name} is definitely a good boy!", dog.Name);
                    break;

                case Cat cat:
                    cat.LivesRemaining = Math.Max(1, cat.LivesRemaining - 1);
                    logger.LogInformation(
                        "Cat {Name} now has {Lives} lives remaining",
                        cat.Name,
                        cat.LivesRemaining
                    );
                    break;

                case Bird bird:
                    if (bird.CanFly)
                    {
                        logger.LogInformation("Bird {Name} is ready to soar!", bird.Name);
                    }
                    break;

                case Mouse mouse:
                    mouse.FavoriteFood = "Premium Cheese";
                    logger.LogInformation(
                        "Mouse {Name} has been upgraded to premium cheese!",
                        mouse.Name
                    );
                    break;
            }

            return await Task.FromResult(animal);
        }

        /// <summary>
        /// Gets information about any animal type
        /// </summary>
        public async Task<string> GetAnimalInfo(Animal animal)
        {
            ArgumentNullException.ThrowIfNull(animal);

            logger.LogInformation(
                "Getting info for {AnimalType}: {Name}",
                animal.GetType().Name,
                animal.Name
            );

            var info = animal.GetInfo();
            var sound = animal.MakeSound();

            return await Task.FromResult($"{info} - Says: '{sound}'");
        }

        /// <summary>
        /// Makes an animal sound
        /// </summary>
        public async Task<string> MakeAnimalSound(Animal animal)
        {
            ArgumentNullException.ThrowIfNull(animal);

            logger.LogInformation(
                "Making sound for {AnimalType}: {Name}",
                animal.GetType().Name,
                animal.Name
            );

            var sound = animal.MakeSound();
            return await Task.FromResult($"{animal.Name} says: {sound}");
        }

        /// <summary>
        /// Processes a group of animals (demonstrates collections of polymorphic objects)
        /// </summary>
        public async Task<Dictionary<string, object>> ProcessAnimalGroup(List<Animal> animals)
        {
            ArgumentNullException.ThrowIfNull(animals);

            logger.LogInformation("Processing group of {Count} animals", animals.Count);

            var result = new Dictionary<string, object>
            {
                ["totalAnimals"] = animals.Count,
                ["processedAt"] = DateTime.UtcNow,
                ["animalTypes"] = new Dictionary<string, int>(),
                ["animalDetails"] = new List<Dictionary<string, object>>(),
                ["sounds"] = new List<string>(),
            };

            var animalTypes = (Dictionary<string, int>)result["animalTypes"];
            var animalDetails = (List<Dictionary<string, object>>)result["animalDetails"];
            var sounds = (List<string>)result["sounds"];

            foreach (var animal in animals)
            {
                var typeName = animal.GetType().Name;
                animalTypes[typeName] = animalTypes.GetValueOrDefault(typeName, 0) + 1;

                animalDetails.Add(
                    new Dictionary<string, object>
                    {
                        ["name"] = animal.Name,
                        ["type"] = typeName,
                        ["age"] = animal.Age,
                        ["species"] = animal.Species,
                        ["info"] = animal.GetInfo(),
                    }
                );

                sounds.Add($"{animal.Name}: {animal.MakeSound()}");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Returns a list of all available animal types
        /// </summary>
        public async Task<List<Animal>> GetAllAnimals()
        {
            logger.LogInformation("Getting all animals");

            var animals = new List<Animal>
            {
                new Dog
                {
                    Name = "Fido",
                    Age = 3,
                    Species = "Golden Retriever",
                    Type = AnimalType.Dog,
                    Breed = "Golden Retriever",
                    IsGoodBoy = true,
                },
                new Cat
                {
                    Name = "Whiskers",
                    Age = 5,
                    Species = "Persian Cat",
                    Type = AnimalType.Cat,
                    Color = "Gray",
                    LivesRemaining = 9,
                },
                new Bird
                {
                    Name = "Polly",
                    Age = 2,
                    Species = "Parrot",
                    Type = AnimalType.Bird,
                    CanFly = true,
                },
                new Mouse
                {
                    Name = "Squeaky",
                    Age = 1,
                    Species = "House Mouse",
                    Type = AnimalType.Mouse,
                    FavoriteFood = "Cheese",
                },
            };

            return await Task.FromResult(animals);
        }

        #endregion

        public async Task<EchoResponse> ProcessRequest(EchoRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            try
            {
                var response = new EchoResponse { RequestId = request.RequestId, Success = true };

                switch (request.Method)
                {
                    case ServiceMethodType.Echo:
                        response.Result = await Echo(request.Payload ?? string.Empty);
                        break;

                    case ServiceMethodType.ComplexEcho:
                        if (string.IsNullOrWhiteSpace(request.Payload))
                        {
                            response.Result = null;
                        }
                        else
                        {
                            try
                            {
                                var message = JsonConvert.DeserializeObject<EchoMessage>(
                                    request.Payload,
                                    jsonSettings
                                );
                                response.Result = await ComplexEcho(message ?? new EchoMessage());
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(
                                    ex,
                                    "Failed to deserialize or process ComplexEcho payload."
                                );
                                response.Result = null;
                            }
                        }
                        break;

                    case ServiceMethodType.FailEcho:
                        try
                        {
                            response.Result = await FailEcho(request.Payload ?? string.Empty);
                        }
                        catch (ServiceException ex)
                        {
                            response.Success = false;
                            response.Error = new EchoFault
                            {
                                Text = ex.Message,
                                Reason = ex.Reason,
                            };
                        }
                        break;

                    case ServiceMethodType.EchoForPermission:
                        response.Result = await EchoForPermission(request.Payload ?? string.Empty);
                        break;

                    // Animal processing methods
                    case ServiceMethodType.ProcessAnimal:
                        if (!string.IsNullOrWhiteSpace(request.Payload))
                        {
                            try
                            {
                                var animal = JsonUtilities.SafeDeserialize<Animal>(
                                    request.Payload,
                                    null!
                                );
                                if (animal != null)
                                {
                                    var processedAnimal = await ProcessAnimal(animal);
                                    response.Result = JsonUtilities.SafeSerialize(processedAnimal);
                                }
                                else
                                {
                                    response.Success = false;
                                    response.Error = new EchoFault
                                    {
                                        Text = "Invalid animal data",
                                        Reason = "DeserializationError",
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to process animal from payload");
                                response.Success = false;
                                response.Error = new EchoFault
                                {
                                    Text = ex.Message,
                                    Reason = "ProcessingError",
                                };
                            }
                        }
                        break;

                    case ServiceMethodType.GetAnimalInfo:
                        if (!string.IsNullOrWhiteSpace(request.Payload))
                        {
                            try
                            {
                                var animal = JsonUtilities.SafeDeserialize<Animal>(
                                    request.Payload,
                                    null!
                                );
                                if (animal != null)
                                {
                                    response.Result = await GetAnimalInfo(animal);
                                }
                                else
                                {
                                    response.Success = false;
                                    response.Error = new EchoFault
                                    {
                                        Text = "Invalid animal data",
                                        Reason = "DeserializationError",
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to get animal info from payload");
                                response.Success = false;
                                response.Error = new EchoFault
                                {
                                    Text = ex.Message,
                                    Reason = "ProcessingError",
                                };
                            }
                        }
                        break;

                    case ServiceMethodType.MakeAnimalSound:
                        if (!string.IsNullOrWhiteSpace(request.Payload))
                        {
                            try
                            {
                                var animal = JsonUtilities.SafeDeserialize<Animal>(
                                    request.Payload,
                                    null!
                                );
                                if (animal != null)
                                {
                                    response.Result = await MakeAnimalSound(animal);
                                }
                                else
                                {
                                    response.Success = false;
                                    response.Error = new EchoFault
                                    {
                                        Text = "Invalid animal data",
                                        Reason = "DeserializationError",
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to make animal sound from payload");
                                response.Success = false;
                                response.Error = new EchoFault
                                {
                                    Text = ex.Message,
                                    Reason = "ProcessingError",
                                };
                            }
                        }
                        break;

                    case ServiceMethodType.ProcessAnimalGroup:
                        if (!string.IsNullOrWhiteSpace(request.Payload))
                        {
                            try
                            {
                                var animals = JsonUtilities.SafeDeserialize<List<Animal>>(
                                    request.Payload,
                                    null!
                                );
                                if (animals != null && animals.Count > 0)
                                {
                                    var result = await ProcessAnimalGroup(animals);
                                    response.Result = JsonUtilities.SafeSerialize(result);
                                }
                                else
                                {
                                    response.Success = false;
                                    response.Error = new EchoFault
                                    {
                                        Text = "Invalid or empty animal group data",
                                        Reason = "DeserializationError",
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to process animal group from payload");
                                response.Success = false;
                                response.Error = new EchoFault
                                {
                                    Text = ex.Message,
                                    Reason = "ProcessingError",
                                };
                            }
                        }
                        break;

                    default:
                        response.Success = false;
                        response.Error = new EchoFault
                        {
                            Text = $"Unknown method: {request.Method}",
                            Reason = "InvalidMethod",
                        };
                        break;
                }

                return response;
            }
            catch (ServiceException ex)
            {
                logger.LogWarning(
                    ex,
                    "ServiceException occurred while processing request {RequestId}",
                    request.RequestId
                );
                return new EchoResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Error = new EchoFault { Text = ex.Message, Reason = ex.Reason },
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error occurred while processing request {RequestId}",
                    request.RequestId
                );
                return new EchoResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Error = new EchoFault { Text = ex.Message, Reason = "UnknownError" },
                };
            }
        }
    }
}

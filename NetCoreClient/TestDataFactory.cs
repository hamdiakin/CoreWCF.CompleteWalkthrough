using Common;

namespace NetCoreClient
{
    public static class TestDataFactory
    {
        public static UserProfile CreateTestUser1()
        {
            return new UserProfile
            {
                Id = Constants.TestUserId123,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Age = 30,
                IsActive = true,
                Roles = new List<string> { "User", "Editor" },
                Metadata = new Dictionary<string, object>
                {
                    ["department"] = "Engineering",
                    ["location"] = "New York"
                }
            };
        }

        public static UserProfile CreateTestUser2()
        {
            return new UserProfile
            {
                Id = Constants.TestUserId456,
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                Age = 25,
                IsActive = true,
                Roles = new List<string> { "User" },
                Metadata = new Dictionary<string, object>()
            };
        }

        public static UserProfile CreateTestUser3()
        {
            return new UserProfile
            {
                Id = Constants.TestUserId789,
                Name = "Bob Wilson",
                Email = "bob.wilson@example.com",
                Age = 35,
                IsActive = false,
                Roles = new List<string> { "User", "Admin" },
                Metadata = new Dictionary<string, object>()
            };
        }

        public static UserProfile CreateTestUser4()
        {
            return new UserProfile
            {
                Id = Constants.TestUserId999,
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
        }

        public static ProcessingOptions CreateStandardProcessingOptions()
        {
            return new ProcessingOptions
            {
                EnableLogging = true,
                MaxRetries = Constants.StandardMaxRetries,
                Timeout = TimeSpan.FromSeconds(Constants.StandardTimeoutSeconds),
                ProcessingMode = Constants.StandardMode,
                CustomSettings = new Dictionary<string, string>
                {
                    [Constants.PriorityKey] = Constants.NormalPriority,
                    ["batchSize"] = "100"
                }
            };
        }

        public static ProcessingOptions CreatePremiumProcessingOptions()
        {
            return new ProcessingOptions
            {
                EnableLogging = true,
                MaxRetries = Constants.PremiumMaxRetries,
                Timeout = TimeSpan.FromSeconds(Constants.PremiumTimeoutSeconds),
                ProcessingMode = Constants.PremiumMode,
                CustomSettings = new Dictionary<string, string>
                {
                    [Constants.PriorityKey] = Constants.HighPriority,
                    ["batchSize"] = "50"
                }
            };
        }

        public static EchoMessage CreateTestEchoMessage(string text = "Complex message from ZeroMQ!")
        {
            return new EchoMessage { Text = text };
        }
    }
} 
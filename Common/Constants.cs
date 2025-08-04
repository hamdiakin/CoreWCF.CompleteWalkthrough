namespace Common
{
    public static class Constants
    {
        // Network Configuration
        public const string DefaultTcpEndpoint = "tcp://localhost:8088";
        public const int DefaultTcpPort = 8088;
        public const int SecureTcpPort = 8443;
        public const int NetworkTimeoutMs = 100;
        
        // Connection Management
        public const int MaxConcurrentConnections = 20;
        public const int ConnectionTimeoutMinutes = 5;
        public const int HealthCheckIntervalSeconds = 30;
        public const int RequestTimeoutSeconds = 30;
        public const int ConnectionRetryDelayMs = 1000;
        public const int MaxConnectionRetries = 3;
        
        // User Profile Validation
        public const int MinUserAge = 0;
        public const int MaxUserAge = 150;
        
        // Processing Options
        public const string PremiumProfileType = "premium";
        public const string StandardProfileType = "standard";
        public const int PremiumMaxRetries = 5;
        public const int StandardMaxRetries = 3;
        public const int PremiumTimeoutSeconds = 60;
        public const int StandardTimeoutSeconds = 30;
        
        // Error Reasons
        public const string InvalidJsonReason = "InvalidJson";
        public const string InvalidMethodReason = "InvalidMethod";
        public const string ProcessingErrorReason = "ProcessingError";
        public const string ComplexMethodErrorReason = "ComplexMethodError";
        public const string InvalidPayloadReason = "InvalidPayload";
        public const string FailReason = "FailReason";
        public const string CapacityLimitReason = "CapacityLimit";
        public const string ConnectionTimeoutReason = "ConnectionTimeout";
        
        // Operation Types
        public const string UpdateProfileOperation = "UPDATE_PROFILE";
        public const string DataMigrationOperation = "DATA_MIGRATION";
        
        // Status Values
        public const string ActiveStatus = "ACTIVE";
        public const string InactiveStatus = "INACTIVE";
        
        // Priority Levels
        public const string HighPriority = "high";
        public const string NormalPriority = "normal";
        
        // Processing Modes
        public const string PremiumMode = "Premium";
        public const string StandardMode = "Standard";
        
        // Feature Flags
        public const string AdvancedFeaturesEnabled = "enabled";
        public const string AdvancedFeaturesKey = "advancedFeatures";
        public const string PriorityKey = "priority";
        public const string ProfileTypeKey = "profileType";
        public const string IncludeAdvancedKey = "includeAdvanced";
        
        // Test Data
        public const string TestUserId123 = "user123";
        public const string TestUserId456 = "user456";
        public const string TestUserId789 = "user789";
        public const string TestUserId999 = "user999";
        
        // Error Messages
        public const string InvalidComplexEchoPayloadMessage = "Invalid ComplexEcho payload";
        public const string EchoFailedMessage = "Echo failed as requested.";
        public const string MissingPayloadMessage = "Missing payload";
        public const string UnknownMethodMessage = "Unknown method";
        public const string UserNotificationSentMessage = "User notification sent for status update to {0}";
        public const string ServerAtCapacityMessage = "Server at capacity";
        public const string ConnectionTimeoutMessage = "Connection timeout";
        
        // Validation Messages
        public const string InvalidEmailMessage = "Invalid email format";
        public const string AgeRangeMessage = "Age must be between 0 and 150";
        public const string NoRolesWarningMessage = "User has no roles assigned";
    }
} 
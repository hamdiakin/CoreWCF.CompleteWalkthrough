namespace Common
{
    public enum ServiceMethodType
    {
        // Original simple methods
        Echo,
        ComplexEcho,
        FailEcho,
        EchoForPermission,
        
        // New complex methods
        ProcessUserProfile,
        ValidateUserData,
        ProcessWithOptions,
        GetProcessingOptions,
        UpdateUserStatus,
        ProcessComplexData,
        
        // Animal-related polymorphic methods
        ProcessAnimal,
        GetAnimalInfo,
        MakeAnimalSound,
        ProcessAnimalGroup,
        GetAllAnimals,
        GetAnimalsByType
    }
} 
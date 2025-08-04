using System.Threading.Tasks;

namespace Common
{
    public interface IEchoService
    {
        Task<string> Echo(string text);
        Task<string?> ComplexEcho(EchoMessage message);
        Task<string> FailEcho(string text);
        Task<string> EchoForPermission(string text);
        Task<EchoResponse> ProcessRequest(EchoRequest request);
        
        // New complex methods with multiple parameters and class transfers
        Task<UserProfile> ProcessUserProfile(UserProfile user, string operationId, bool validateOnly);
        Task<ValidationResult> ValidateUserData(UserProfile user, bool strictValidation, int maxErrors);
        Task<string> ProcessWithOptions(string data, ProcessingOptions options, bool isPriority);
        Task<ProcessingOptions> GetProcessingOptions(string profileType, bool includeAdvanced);
        Task<bool> UpdateUserStatus(UserProfile user, string newStatus, bool notifyUser, int priority);
        Task<Dictionary<string, object>> ProcessComplexData(UserProfile user, ProcessingOptions options, string operation, bool dryRun);
    }
}
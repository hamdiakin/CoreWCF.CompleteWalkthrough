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
    }
}
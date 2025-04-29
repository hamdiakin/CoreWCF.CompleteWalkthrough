using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IEchoService
    {
        [OperationContract]
        string Echo(string text);

        [OperationContract]
        string? ComplexEcho(EchoMessage text);

        [OperationContract]
        [FaultContract(typeof(EchoFault))]
        string FailEcho(string text);

        [OperationContract]
        string EchoForPermission(string text);
    }
}

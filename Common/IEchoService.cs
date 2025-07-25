namespace Common
{
    public interface IEchoService
    {
        string Echo(string text);
        string? ComplexEcho(EchoMessage text);
        string FailEcho(string text);
        string EchoForPermission(string text);
        ZmqResponse ProcessRequest(ZmqRequest request);
    }
}
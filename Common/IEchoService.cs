namespace Common
{
    public interface IEchoService
    {
        string Echo(string text);
        string? ComplexEcho(EchoMessage message);
        string FailEcho(string text);
        string EchoForPermission(string text);
        EchoResponse ProcessRequest(EchoRequest request);
    }
}
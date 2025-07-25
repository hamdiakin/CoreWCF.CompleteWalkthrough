using Newtonsoft.Json;

namespace Common
{
    public class EchoService : IEchoService
    {
        public string Echo(string text)
        {
            Console.WriteLine($"Received {text} from client!");
            return text;
        }

        public string? ComplexEcho(EchoMessage text)
        {
            Console.WriteLine($"Received {text.Text} from client!");
            return text?.Text;
        }

        public string FailEcho(string text)
        {
            throw new EchoServiceException("WCF Fault OK", "FailReason");
        }

        public string EchoForPermission(string echo)
        {
            // Note: Permission checking would need to be implemented at the transport level in ZeroMQ
            return echo;
        }

        public ZmqResponse ProcessRequest(ZmqRequest request)
        {
            try
            {
                var response = new ZmqResponse
                {
                    RequestId = request.RequestId,
                    Success = true
                };

                switch (request.Method.ToLower())
                {
                    case "echo":
                        response.Result = Echo(request.Payload ?? string.Empty);
                        break;

                    case "complexecho":
                        var echoMessage = JsonConvert.DeserializeObject<EchoMessage>(request.Payload ?? "{}");
                        response.Result = ComplexEcho(echoMessage);
                        break;

                    case "failecho":
                        response.Result = FailEcho(request.Payload ?? string.Empty);
                        break;

                    case "echoforpermission":
                        response.Result = EchoForPermission(request.Payload ?? string.Empty);
                        break;

                    default:
                        response.Success = false;
                        response.Error = new EchoFault
                        {
                            Text = $"Unknown method: {request.Method}",
                            Reason = "InvalidMethod"
                        };
                        break;
                }

                return response;
            }
            catch (EchoServiceException ex)
            {
                return new ZmqResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Error = new EchoFault
                    {
                        Text = ex.Message,
                        Reason = ex.Reason
                    }
                };
            }
            catch (Exception ex)
            {
                return new ZmqResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Error = new EchoFault
                    {
                        Text = ex.Message,
                        Reason = "UnknownError"
                    }
                };
            }
        }
    }

    public class EchoServiceException : Exception
    {
        public string Reason { get; }

        public EchoServiceException(string message, string reason) : base(message)
        {
            Reason = reason;
        }
    }
}
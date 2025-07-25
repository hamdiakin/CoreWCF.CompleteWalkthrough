using System.Text.Json;

namespace Common
{
    public class EchoService : IEchoService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public string Echo(string text)
        {
            Console.WriteLine($"Received {text} from client!");
            return text;
        }

        public string? ComplexEcho(EchoMessage message)
        {
            Console.WriteLine($"Received {message.Text} from client!");
            return message?.Text;
        }

        public string FailEcho(string text)
        {
            throw new ServiceException("Echo failed as requested.", "FailReason");
        }

        public string EchoForPermission(string text)
        {
            return text;
        }

        public EchoResponse ProcessRequest(EchoRequest request)
        {
            try
            {
                var response = new EchoResponse
                {
                    RequestId = request.RequestId,
                    Success = true
                };

                var methodMap = new Dictionary<string, Func<string?, string?>>
                {
                    { "echo", payload => Echo(payload ?? string.Empty) },
                    { "complexecho", payload =>
                        {
                            if (string.IsNullOrWhiteSpace(payload)) return null;
                            try
                            {
                                var message = JsonSerializer.Deserialize<EchoMessage>(payload, JsonOptions);
                                return ComplexEcho(message ?? new EchoMessage());
                            }
                            catch
                            {
                                return null;
                            }
                        }
                    },
                    { "failecho", payload => FailEcho(payload ?? string.Empty) },
                    { "echoforpermission", payload => EchoForPermission(payload ?? string.Empty) }
                };

                if (methodMap.TryGetValue(request.Method.ToLower(), out var handler))
                {
                    response.Result = handler(request.Payload);
                }
                else
                {
                    response.Success = false;
                    response.Error = new EchoFault
                    {
                        Text = $"Unknown method: {request.Method}",
                        Reason = "InvalidMethod"
                    };
                }

                return response;
            }
            catch (ServiceException ex)
            {
                return new EchoResponse
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
                return new EchoResponse
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
}
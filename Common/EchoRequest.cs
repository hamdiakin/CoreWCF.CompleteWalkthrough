namespace Common
{
    public class EchoRequest
    {
        public ServiceMethodType Method { get; set; }

        public string? Payload { get; set; }

        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }
} 
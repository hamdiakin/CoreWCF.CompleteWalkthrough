namespace Common
{
    public class EchoRequest
    {
        public string Method { get; set; } = string.Empty;

        public string? Payload { get; set; }

        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }
} 
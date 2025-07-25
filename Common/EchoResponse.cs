namespace Common
{
    public class EchoResponse
    {
        public bool Success { get; set; }

        public string? Result { get; set; }

        public EchoFault? Error { get; set; }

        public string RequestId { get; set; } = string.Empty;
    }
} 
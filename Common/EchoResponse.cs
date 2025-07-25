using System.Text.Json.Serialization;

namespace Common
{
    public class EchoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("error")]
        public EchoFault? Error { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;
    }
} 
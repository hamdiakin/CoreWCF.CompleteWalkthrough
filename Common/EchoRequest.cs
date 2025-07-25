using System.Text.Json.Serialization;

namespace Common
{
    public class EchoRequest
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }
} 
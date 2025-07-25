using Newtonsoft.Json;

namespace Common
{
    public class ZmqRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("payload")]
        public string? Payload { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    public class ZmqResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("result")]
        public string? Result { get; set; }

        [JsonProperty("error")]
        public EchoFault? Error { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;
    }
}
using System.Text.Json.Serialization;

namespace Common
{
    public class EchoFault
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}

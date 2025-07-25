using System.Text.Json.Serialization;

namespace Common
{
    public class EchoMessage
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
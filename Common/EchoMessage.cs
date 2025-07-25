using Newtonsoft.Json;

namespace Common
{
    public class EchoMessage
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }
}
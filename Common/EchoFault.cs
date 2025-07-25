using Newtonsoft.Json;

namespace Common
{
    public class EchoFault
    {
        private string? text;

        [JsonProperty("text")]
        public string? Text
        {
            get { return text; }
            set { text = value; }
        }

        [JsonProperty("reason")]
        public string? Reason { get; set; }
    }
}

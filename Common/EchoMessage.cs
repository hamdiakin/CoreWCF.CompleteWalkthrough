using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class EchoMessage
    {
        [DataMember]
        public string? Text { get; set; }
    }
}

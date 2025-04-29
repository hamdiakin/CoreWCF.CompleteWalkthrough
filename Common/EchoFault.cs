using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class EchoFault
    {
        private string? text;

        [DataMember]
        public string? Text
        {
            get { return text; }
            set { text = value; }
        }
    }
}

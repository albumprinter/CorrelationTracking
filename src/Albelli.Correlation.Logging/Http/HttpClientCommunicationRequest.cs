using System.Runtime.Serialization;
using System.Text;

namespace Albelli.Correlation.Http
{
    [DataContract]
    public class HttpClientCommunicationRequest : HttpClientCommunicationBase
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Method { get; set; }

        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append("BeforeSendRequest: ");
            output.Append("Method: ").Append(Method);
            output.Append(", RequestUri: '").Append(Url ?? "<null>");
            output.AppendLine(", Headers: {");
            output.AppendLine(HeadersAsString);
            output.Append("}");
            output.Append(", Content: ");
            output.Append(Content ?? "<null>");
            return output.ToString();
        }
    }
}

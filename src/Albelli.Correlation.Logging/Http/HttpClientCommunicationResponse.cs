using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace Albelli.Correlation.Http
{
    [DataContract]
    public class HttpClientCommunicationResponse : HttpClientCommunicationBase
    {
        [DataMember]
        public TimeSpan Duration { get; set; }

        [DataMember]
        public HttpStatusCode StatusCode { get; set; }

        [DataMember]
        public string ReasonPhrase { get; set; }

        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append("AfterReceiveResponse: ");
            output.Append("StatusCode: ").Append(StatusCode);
            output.Append(", ReasonPhrase: '").Append(ReasonPhrase ?? "<null>");
            output.AppendLine("', Headers: {");
            output.AppendLine(HeadersAsString);
            output.Append("}");
            output.Append(", Content: ");
            output.Append(Content ?? "<null>");
            return output.ToString();
        }
    }
}

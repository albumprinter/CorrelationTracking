using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Albelli.Correlation.Http
{
    [DataContract]
    public abstract class HttpClientCommunicationBase
    {
        [DataMember]
        public Guid OperationId { get; set; }

        [DataMember]
        public List<KeyValuePair<string, string>> Headers { get; set; }

        [DataMember]
        public string Content { get; set; }

        [IgnoreDataMember]
        protected string HeadersAsString
        {
            get { return string.Join(Environment.NewLine, Headers.Select(h => $"{h.Key}: {h.Value}")); }
        }
    }
}

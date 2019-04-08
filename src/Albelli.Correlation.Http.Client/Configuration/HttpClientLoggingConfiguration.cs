using Albumprinter.CorrelationTracking.Correlation.Core;
using System;
using System.Collections.Generic;

namespace Albelli.Correlation.Http.Client.Configuration
{
    public interface IHttpClientLoggingConfiguration
    {
        ICollection<string> AllowedHeaders { get; }
        bool LogRequest { get; }
        bool LogRequestContent { get; }
        bool LogResponse { get; }
        bool LogResponseContent { get; }
    }


    public class HttpClientLoggingConfiguration : IHttpClientLoggingConfiguration
    {
        public ICollection<string> AllowedHeaders { get; }
        public bool LogRequest { get; }
        public bool LogRequestContent { get; }
        public bool LogResponse { get; }
        public bool LogResponseContent { get; }

        private static readonly HashSet<string> _allowedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Accept",
            "Content-Type",
            CorrelationKeys.CorrelationId,
            CorrelationKeys.RequestId
        };

        public HttpClientLoggingConfiguration() : this(_allowedHeaders)
        {
        }

        public HttpClientLoggingConfiguration(
            ICollection<string> allowedHeaders,
            bool logRequest = true,
            bool logRequestContent = true,
            bool logResponse = true,
            bool logResponseContent = true)
        {
            AllowedHeaders = allowedHeaders;
            LogRequest = logRequest;
            LogRequestContent = logRequestContent;
            LogResponse = logResponse;
            LogResponseContent = logResponseContent;
        }
    }
}

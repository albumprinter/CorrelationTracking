using System;
using System.Collections.Generic;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albelli.Correlation.Http
{
    public interface IHttpClientLoggingConfiguration
    {
        IHttpClientLogger Logger { get; }
        ICollection<string> AllowedHeaders { get; }
        bool LogRequest { get; }
        bool LogRequestContent { get; }
        bool LogResponse { get; }
        bool LogResponseContent { get; }
    }


    public class HttpClientLoggingConfiguration : IHttpClientLoggingConfiguration
    {
        public IHttpClientLogger Logger { get; }
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

        public HttpClientLoggingConfiguration(IHttpClientLogger logger) : this(logger, _allowedHeaders)
        {
        }

        public HttpClientLoggingConfiguration(IHttpClientLogger logger,
            ICollection<string> allowedHeaders,
            bool logRequest = true,
            bool logRequestContent = true,
            bool logResponse = true,
            bool logResponseContent = true)
        {
            Logger = logger;
            AllowedHeaders = allowedHeaders;
            LogRequest = logRequest;
            LogRequestContent = logRequestContent;
            LogResponse = logResponse;
            LogResponseContent = logResponseContent;
        }
    }
}

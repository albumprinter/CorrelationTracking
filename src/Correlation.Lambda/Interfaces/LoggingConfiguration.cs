using System;
using System.Collections.Generic;

namespace Albumprinter.CorrelationTracking.Correlation.Interfaces
{
    public interface ILoggingConfiguration
    {
        IHttpClientLogger Logger { get; }
        ICollection<string> AllowedHeaders { get; }
        bool LogRequest { get; }
        bool LogRequestContent { get; }
        bool LogResponse { get; }
        bool LogResponseContent { get; }
    }


    public class LoggingConfiguration : ILoggingConfiguration
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
            "X-CorrelationId",
            "X-RequestId"
        };

        public LoggingConfiguration(IHttpClientLogger logger) : this(logger, _allowedHeaders)
        {
        }

        public LoggingConfiguration(IHttpClientLogger logger,
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

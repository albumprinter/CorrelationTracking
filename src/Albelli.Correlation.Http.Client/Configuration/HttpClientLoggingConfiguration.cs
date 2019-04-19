using Albumprinter.CorrelationTracking.Correlation.Core;
using System;
using System.Collections.Generic;
using System.Net;
// ReSharper disable UnusedMember.Global

namespace Albelli.Correlation.Http.Client.Configuration
{
    public interface IHttpClientLoggingConfiguration
    {
        ICollection<string> AllowedHeaders { get; }
        bool LogRequest { get; }
        bool LogRequestContent { get; }
        bool LogResponse { get; }
        bool LogResponseContent { get; }
        ICollection<HttpStatusCode> WhiteListedCodes { get; }
    }

    public class HttpClientLoggingConfiguration : IHttpClientLoggingConfiguration
    {
        public ICollection<string> AllowedHeaders { get; }
        public bool LogRequest { get; }
        public bool LogRequestContent { get; }
        public bool LogResponse { get; }
        public bool LogResponseContent { get; }
        public ICollection<HttpStatusCode> WhiteListedCodes { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly HashSet<string> DefaultAllowedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Accept",
            "Content-Type",
            CorrelationKeys.CorrelationId,
            CorrelationKeys.RequestId
        };

        public HttpClientLoggingConfiguration() : this(DefaultAllowedHeaders)
        {
            WhiteListedCodes = new List<HttpStatusCode>();
        }

        /// <summary>
        /// </summary>
        /// <param name="allowedHeaders">These headers will appear in logs</param>
        /// <param name="logRequest">Whether log request at all</param>
        /// <param name="logRequestContent">Whether log request body (content)</param>
        /// <param name="logResponse">Whether log response at all</param>
        /// <param name="logResponseContent">Whether log response body (content)</param>
        /// <param name="whiteListedCodes">These codes will considered as info message (e.g. you could add 404 if it is ok case for you)</param>
        public HttpClientLoggingConfiguration(
            ICollection<string> allowedHeaders,
            bool logRequest = true,
            bool logRequestContent = true,
            bool logResponse = true,
            bool logResponseContent = true,
            ICollection<HttpStatusCode> whiteListedCodes = null)
        {
            AllowedHeaders = allowedHeaders;
            WhiteListedCodes = whiteListedCodes ?? new List<HttpStatusCode>();
            LogRequest = logRequest;
            LogRequestContent = logRequestContent;
            LogResponse = logResponse;
            LogResponseContent = logResponseContent;
        }
    }
}

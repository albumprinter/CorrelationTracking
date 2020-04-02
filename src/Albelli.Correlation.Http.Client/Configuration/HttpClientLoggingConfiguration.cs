using Albumprinter.CorrelationTracking.Correlation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using JetBrains.Annotations;

namespace Albelli.Correlation.Http.Client.Configuration
{
    [PublicAPI]
    public interface IHttpClientLoggingConfiguration
    {
        ICollection<string> AllowedHeaders { get; }
        Func<HttpRequestMessage, bool> LogRequest { get; }
        Func<HttpRequestMessage, bool> LogRequestContent { get; }
        Func<HttpResponseMessage, bool> LogResponse { get; }
        Func<HttpResponseMessage, bool> LogResponseContent { get; }
        ICollection<HttpStatusCode> WhiteListedCodes { get; }
    }

    [PublicAPI]
    public class HttpClientLoggingConfiguration : IHttpClientLoggingConfiguration
    {
        public ICollection<string> AllowedHeaders { get; }
        public Func<HttpRequestMessage, bool> LogRequest { get; }
        public Func<HttpRequestMessage, bool> LogRequestContent { get; }
        public Func<HttpResponseMessage, bool> LogResponse { get; }
        public Func<HttpResponseMessage, bool> LogResponseContent { get; }
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
        }

        /// <summary>
        /// </summary>
        /// <param name="allowedHeaders">These headers will appear in logs</param>
        /// <param name="logRequest">Whether log request at all, if null - logs</param>
        /// <param name="logRequestContent">Whether log request body (content), if null - logs</param>
        /// <param name="logResponse">Whether log response at all, if null - logs</param>
        /// <param name="logResponseContent">Whether log response body (content), if null - logs</param>
        /// <param name="whiteListedCodes">These codes will considered as info message (e.g. you could add 404 if it is ok case for you)</param>
        public HttpClientLoggingConfiguration(
            ICollection<string> allowedHeaders,
            Func<HttpRequestMessage, bool> logRequest = null,
            Func<HttpRequestMessage, bool> logRequestContent = null,
            Func<HttpResponseMessage, bool> logResponse = null,
            Func<HttpResponseMessage, bool> logResponseContent = null,
            ICollection<HttpStatusCode> whiteListedCodes = null)
        {
            AllowedHeaders = allowedHeaders;
            WhiteListedCodes = whiteListedCodes ?? new List<HttpStatusCode>();
            LogRequest = logRequest ?? True;
            LogRequestContent = logRequestContent ?? True;
            LogResponse = logResponse ?? True;
            LogResponseContent = logResponseContent ?? True;
        }

        private static bool True(HttpRequestMessage message) => true;
        private static bool True(HttpResponseMessage message) => true;
    }
}

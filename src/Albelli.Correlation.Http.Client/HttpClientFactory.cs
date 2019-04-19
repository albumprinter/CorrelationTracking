using Albelli.Correlation.Http.Client.Configuration;
using Albelli.Correlation.Http.Client.Handlers;
using System;
using System.Net.Http;
// ReSharper disable UnusedMember.Global

namespace Albelli.Correlation.Http.Client
{
    public class HttpClientFactory
    {
        public static HttpClient Create(IHttpClientLoggingConfiguration _loggingConfiguration = null)
        {
            var loggingHandler = _loggingConfiguration == null ? null : new LoggingDelegatingHandler(_loggingConfiguration);

            var correlationHandler = new CorrelationDelegatingHandler
            {
                InnerHandler = loggingHandler
            };
            return new HttpClient(correlationHandler);
        }

        public static HttpClient Create(CorrelationDelegatingHandler correlationHandler, LoggingDelegatingHandler loggingHandler)
        {
            if (correlationHandler == null)
            {
                throw new ArgumentNullException(nameof(correlationHandler));
            }

            correlationHandler.InnerHandler = loggingHandler;
            return new HttpClient(correlationHandler);
        }
    }
}

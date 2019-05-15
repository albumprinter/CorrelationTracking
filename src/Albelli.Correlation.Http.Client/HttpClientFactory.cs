using Albelli.Correlation.Http.Client.Configuration;
using Albelli.Correlation.Http.Client.Handlers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Albelli.Correlation.Http.Client
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class HttpClientFactory
    {
        public static HttpClient Create(IHttpClientLoggingConfiguration _loggingConfiguration = null)
        {
            var httpHandler = new HttpClientHandler(); // the most internal

            HttpMessageHandler loggingHandler = _loggingConfiguration == null
                ? null
                : new LoggingDelegatingHandler(_loggingConfiguration)
                {
                    InnerHandler = httpHandler
                };

            var correlationHandler = new CorrelationDelegatingHandler
            {
                InnerHandler = loggingHandler ?? httpHandler
            };
            return new HttpClient(correlationHandler);
        }

        public static HttpClient Create(CorrelationDelegatingHandler correlationHandler, LoggingDelegatingHandler loggingHandler)
        {
            if (correlationHandler == null)
            {
                throw new ArgumentNullException(nameof(correlationHandler));
            }

            var httpHandler = new HttpClientHandler();
            loggingHandler.InnerHandler = httpHandler;
            correlationHandler.InnerHandler = loggingHandler;
            return new HttpClient(correlationHandler);
        }
    }
}

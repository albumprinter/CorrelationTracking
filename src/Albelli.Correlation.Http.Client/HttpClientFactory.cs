using Albelli.Correlation.Http.Client.Configuration;
using Albelli.Correlation.Http.Client.Handlers;
using JetBrains.Annotations;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Albelli.Correlation.Http.Client
{
    [PublicAPI]
    public sealed class HttpClientFactory
    {
        public static HttpClient Create(ILoggerFactory loggerFactory, IHttpClientLoggingConfiguration loggingConfiguration = null)
        {
            var handler = BuildHttpMessageHandler(loggerFactory, loggingConfiguration);

            return new HttpClient(handler);
        }

        public static HttpClient Create(CorrelationDelegatingHandler correlationHandler, LoggingDelegatingHandler loggingHandler)
        {
            if (correlationHandler == null)
            {
                throw new ArgumentNullException(nameof(correlationHandler));
            }

            if (loggingHandler == null)
            {
                throw new ArgumentNullException(nameof(correlationHandler));
            }

            var httpHandler = new HttpClientHandler();
            loggingHandler.InnerHandler = httpHandler;
            correlationHandler.InnerHandler = loggingHandler;
            return new HttpClient(correlationHandler);
        }

        public static HttpMessageHandler BuildHttpMessageHandler([NotNull] ILoggerFactory loggerFactory, IHttpClientLoggingConfiguration loggingConfiguration = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            var httpHandler = new HttpClientHandler(); // the most internal

            HttpMessageHandler loggingHandler = loggingConfiguration == null
                ? null
                : new LoggingDelegatingHandler(loggerFactory, loggingConfiguration)
                {
                    InnerHandler = httpHandler
                };

            var correlationHandler = new CorrelationDelegatingHandler
            {
                InnerHandler = loggingHandler ?? httpHandler
            };

            return correlationHandler;
        }
    }
}

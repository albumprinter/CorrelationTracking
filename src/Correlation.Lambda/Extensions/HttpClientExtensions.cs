using Albumprinter.CorrelationTracking.Correlation.Handlers;
using Albumprinter.CorrelationTracking.Correlation.Interfaces;
using System;
using System.Net.Http;
using System.Reflection;

namespace Albumprinter.CorrelationTracking.Correlation.Extensions
{
    public static class HttpClientExtensions
    {
        private static readonly FieldInfo HandlerField;

        static HttpClientExtensions()
        {
            HandlerField = typeof(HttpMessageInvoker).GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static HttpClient UseCorrelationTracking(this HttpClient client, IHttpClientLoggingConfiguration config = null)
        {
            ValidateContext();
            var oldHandler = HandlerField.GetValue(client) as HttpMessageHandler;
            if (oldHandler is CorrelationDelegatingHandler) // already set
            {
                return client;
            }

            var newHandler = new CorrelationDelegatingHandler
            {
                InnerHandler = config == null
                    ? oldHandler
                    : new LoggingDelegatingHandler(config) { InnerHandler = oldHandler }
            };
            HandlerField.SetValue(client, newHandler);
            return client;
        }

        public static THttpMessageHandler GetHttpMessageHandler<THttpMessageHandler>(this HttpClient client)
            where THttpMessageHandler : HttpMessageHandler
        {
            ValidateContext();
            var handler = HandlerField.GetValue(client) as HttpMessageHandler;
            while (handler != null)
            {
                if (handler is THttpMessageHandler httpMessageHandler)
                {
                    return httpMessageHandler;
                }
                var delegatingHandler = handler as DelegatingHandler;
                handler = delegatingHandler?.InnerHandler;
            }
            return null;
        }

        private static void ValidateContext()
        {
            if (HandlerField == null || !typeof(HttpMessageInvoker).IsAssignableFrom(typeof(HttpClient)))
            {
                throw new NotSupportedException("Could not apply the Correlation Tracking to HttpClient.");
            }
        }
    }
}

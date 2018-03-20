using System;
using System.Net.Http;
using System.Reflection;
using Albumprinter.CorrelationTracking.Tracing.Http;

namespace Albumprinter.CorrelationTracking.Http
{
    public static class HttpClientExtensions
    {
        private static readonly FieldInfo HandlerField;

        static HttpClientExtensions()
        {
            HandlerField = typeof(HttpMessageInvoker).GetField(
                "handler",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static HttpClient UseCorrelationTracking(this HttpClient client)
        {
            if (HandlerField == null || !typeof(HttpMessageInvoker).IsAssignableFrom(typeof(HttpClient)))
            {
                throw new NotSupportedException("Could not apply the Correlation Tracking to HttpClient.");
            }
            var oldHandler = (HttpMessageHandler) HandlerField.GetValue(client);
            if (oldHandler is CorrelationDelegatingHandler == false)
            {
                var newHandler = new CorrelationDelegatingHandler
                {
                    InnerHandler = new Log4NetDelegatingHandler { InnerHandler = oldHandler }
                };
                HandlerField.SetValue(client, newHandler);
            }
            return client;
        }

        public static THttpMessageHandler GetHttpMessageHandler<THttpMessageHandler>(this HttpClient client)
            where THttpMessageHandler : HttpMessageHandler
        {
            if (HandlerField == null || !typeof(HttpMessageInvoker).IsAssignableFrom(typeof(HttpClient)))
            {
                throw new NotSupportedException("Could not apply the Correlation Tracking to HttpClient.");
            }
            var handler = HandlerField.GetValue(client) as HttpMessageHandler;
            while (handler != null)
            {
                var httpMessageHandler = handler as THttpMessageHandler;
                if (httpMessageHandler != null)
                {
                    return httpMessageHandler;
                }
                var delegatingHandler = handler as DelegatingHandler;
                handler = delegatingHandler?.InnerHandler;
            }
            return null;
        }
    }
}
using System;
using System.Linq;
using Amazon.Runtime.Internal;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingPipelineCustomizer : IRuntimePipelineCustomizer
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly LoggingOptions _options;

        public LoggingPipelineCustomizer([NotNull] ILoggerFactory loggerFactory, LoggingOptions options = null)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options;
        }

        public void Customize(Type type, RuntimePipeline pipeline)
        {
            //type can be AmazonSimpleNotificationServiceClient or AmazonSQSClient
            var loggingPipelineHandler = new LoggingPipelineHandler(_loggerFactory, _options);
            var handlers = pipeline.Handlers;
            if (handlers.Any(handler => handler.GetType() == typeof(HttpHandler<System.IO.Stream>)))
            {
                pipeline.AddHandlerBefore<HttpHandler<System.IO.Stream>>(loggingPipelineHandler);
            }
            else if (handlers.Any(handler => handler.GetType() == typeof(HttpHandler<System.Net.Http.HttpContent>)))

            {
                pipeline.AddHandlerBefore<HttpHandler<System.Net.Http.HttpContent>>(loggingPipelineHandler);
            }
            else
            {
                pipeline.AddHandlerAfter<Unmarshaller>(loggingPipelineHandler);
            }
        }

        public string UniqueName => nameof(LoggingPipelineCustomizer);
    }
}
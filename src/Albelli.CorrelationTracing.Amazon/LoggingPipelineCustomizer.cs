using System;
using System.Linq;
using Amazon.Runtime.Internal;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingPipelineCustomizer : IRuntimePipelineCustomizer
    {
        private readonly LoggingOptions _options;

        public LoggingPipelineCustomizer(LoggingOptions options = null)
        {
            _options = options;
        }

        public void Customize(Type type, RuntimePipeline pipeline)
        {
            //type can be AmazonSimpleNotificationServiceClient or AmazonSQSClient
            var loggingPipelineHandler = new LoggingPipelineHandler(_options);
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
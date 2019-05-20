using System;
using Amazon.Runtime.Internal;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingPipelineCustomizer : IRuntimePipelineCustomizer
    {
        private readonly LoggingOptions _options;

        public LoggingPipelineCustomizer(LoggingOptions options)
        {
            _options = options;
        }

        public void Customize(Type type, RuntimePipeline pipeline)
        {
            //type can be AmazonSimpleNotificationServiceClient or AmazonSQSClient
            pipeline.AddHandlerBefore<HttpHandler<System.IO.Stream>>(new LoggingPipelineHandler(_options));
        }

        public string UniqueName => nameof(LoggingPipelineCustomizer);
    }
}
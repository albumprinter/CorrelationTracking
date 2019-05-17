using System;
using Amazon.Runtime.Internal;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingPipelineCustomizer : IRuntimePipelineCustomizer
    {
        public void Customize(Type type, RuntimePipeline pipeline)
        {
            //type can be AmazonSimpleNotificationServiceClient or AmazonSQSClient
            pipeline.AddHandlerBefore<HttpHandler<System.IO.Stream>>(new LoggingPipelineHandler());
        }

        public string UniqueName => nameof(LoggingPipelineCustomizer);
    }
}
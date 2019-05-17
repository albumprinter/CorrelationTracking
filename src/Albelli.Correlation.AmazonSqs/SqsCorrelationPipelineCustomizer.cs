using System;
using System.Linq;
using Amazon.Runtime.Internal;

namespace Albelli.Correlation.AmazonSqs
{
    public class SqsCorrelationPipelineCustomizer : IRuntimePipelineCustomizer
    {
        public void Customize(Type type, RuntimePipeline pipeline)
        {
            var handlers = pipeline.Handlers;
            if (handlers.Any(a => a is SqsCorrelationPipelineHandler))
            {
                return;
            }

            //Marshaller handler populates IRequestContext.Request
            pipeline.AddHandlerAfter<Marshaller>(new SqsCorrelationHeaderPipelineHandler());
            pipeline.AddHandler(new SqsCorrelationPipelineHandler());
        }

        public string UniqueName => nameof(SqsCorrelationPipelineCustomizer);
    }
}
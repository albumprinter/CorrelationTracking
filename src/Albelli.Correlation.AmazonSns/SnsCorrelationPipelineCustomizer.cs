using System;
using System.Linq;
using Amazon.Runtime.Internal;

namespace Albelli.Correlation.AmazonSns
{
    public class SnsCorrelationPipelineCustomizer : IRuntimePipelineCustomizer
    {
        public void Customize(Type type, RuntimePipeline pipeline)
        {
            var handlers = pipeline.Handlers;
            if (handlers.Any(a => a is SnsCorrelationPipelineHandler))
            {
                return;
            }

            //Marshaller handler populates IRequestContext.Request
            pipeline.AddHandlerAfter<Marshaller>(new SnsCorrelationHeaderPipelineHandler());
            pipeline.AddHandler(new SnsCorrelationPipelineHandler());
        }

        public string UniqueName => nameof(SnsCorrelationPipelineCustomizer);
    }
}
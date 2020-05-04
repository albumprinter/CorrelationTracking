using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.Runtime.Internal;

namespace Albelli.Correlation.AmazonSns
{
    public class SnsCorrelationHeaderPipelineHandler : PipelineHandler
    {
        public override void InvokeSync(IExecutionContext executionContext)
        {
            AddCorrelationAttributeIfAbsent(executionContext.RequestContext);
            base.InvokeSync(executionContext);
        }

        public override async Task<T> InvokeAsync<T>(IExecutionContext executionContext)
        {
            AddCorrelationAttributeIfAbsent(executionContext.RequestContext);
            var result = await base.InvokeAsync<T>(executionContext);
            return result;
        }

        private static void AddCorrelationAttributeIfAbsent(IRequestContext requestContext)
        {
            //that piece of code works only *after* Marshaller
            var awsRequest = requestContext?.Request;
            if (awsRequest != null && !awsRequest.Headers.ContainsKey(CorrelationKeys.CorrelationId))
            {
                if (CorrelationScope.Current != null)
                {
                    awsRequest.Headers[CorrelationKeys.CorrelationId] = CorrelationScope.Current.CorrelationId.ToString();
                }
            }
        }
    }
}
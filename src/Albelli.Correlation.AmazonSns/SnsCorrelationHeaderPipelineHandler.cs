using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.SimpleNotificationService.Model;

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
            if (!(requestContext?.OriginalRequest is PublishRequest))
            {
                return;
            }

            var request = requestContext.Request;

            TrySetHeader(request, CorrelationKeys.CorrelationId, CorrelationScope.Current?.CorrelationId.ToString());
        }

        private static void TrySetHeader(IRequest request, string key, string value)
        {
            if (request == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            request.Headers[key] = value;
        }
    }
}
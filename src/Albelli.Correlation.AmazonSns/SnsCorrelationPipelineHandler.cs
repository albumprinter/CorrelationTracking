using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.SimpleNotificationService.Model;

namespace Albelli.Correlation.AmazonSns
{
    public class SnsCorrelationPipelineHandler : PipelineHandler
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
            //that piece of code works only *before* Marshaller
            if (!(requestContext.OriginalRequest is PublishRequest request))
            {
                return;
            }

            var activity = Activity.Current;

            TrySetAttribute(request, CorrelationKeys.CorrelationId, CorrelationScope.Current?.CorrelationId.ToString());
            TrySetAttribute(request, CorrelationKeys.TraceParent, activity?.Id);
            TrySetAttribute(request, CorrelationKeys.TraceState, activity?.TraceStateString);
        }

        private static void TrySetAttribute(PublishRequest request, string key, string value)
        {
            if (request == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            request.MessageAttributes ??= new Dictionary<string, MessageAttributeValue>();
            request.MessageAttributes[key] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = value
            };
        }
    }
}
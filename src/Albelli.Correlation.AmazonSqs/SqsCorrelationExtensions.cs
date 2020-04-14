using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime.Internal;
using Amazon.SQS.Model;
using JetBrains.Annotations;

namespace Albelli.Correlation.AmazonSqs
{
    [PublicAPI]
    public static class SqsCorrelationExtensions
    {
        public static void ConfigureHandlers()
        {
            RuntimePipelineCustomizerRegistry.Instance.Register(new SqsCorrelationPipelineCustomizer());
        }

        public static IDisposable UseCorrelationScope(this Message message)
        {
            if (message.MessageAttributes == null)
            {
                return null;
            }

            if (message.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out var correlationId))
            {
                if (Guid.TryParse(correlationId.StringValue, out var correlationIdGuid))
                {
                    return CorrelationManager.Instance.UseScope(correlationIdGuid);
                }
            }

            return null;
        }
    }
}

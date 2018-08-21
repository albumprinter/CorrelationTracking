using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Lambda.SQSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs.Lambda
{
    public static class CorrelationExtensions
    {
        public static Guid? ExtractCorrelationId(this SQSEvent.SQSMessage sqsMessage)
        {
            if (sqsMessage?.MessageAttributes != null &&
                sqsMessage.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out var attribute) &&
                attribute?.StringValue != null &&
                Guid.TryParse(attribute.StringValue, out var correlationId))
            {
                return correlationId;
            }
            return null;
        }

        public static IDisposable TrackCorrelationId(this SQSEvent.SQSMessage sqsMessage)
        {
            return CorrelationManager.Instance.UseScope(sqsMessage.ExtractCorrelationId() ?? Guid.NewGuid(), Guid.NewGuid());
        }
    }
}
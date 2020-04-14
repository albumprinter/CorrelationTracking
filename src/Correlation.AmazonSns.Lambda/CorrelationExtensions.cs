using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Lambda.SNSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns.Lambda
{
    public static class CorrelationExtensions
    {
        public static Guid? ExtractCorrelationId(this SNSEvent.SNSMessage snsMessage)
        {
            if (snsMessage?.MessageAttributes != null &&
                snsMessage.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out var attribute) &&
                attribute?.Value != null &&
                Guid.TryParse(attribute.Value, out var correlationId))
            {
                return correlationId;
            }
            return null;
        }

        public static IDisposable TrackCorrelationId(this SNSEvent.SNSRecord snsRecord)
        {
            return CorrelationManager.Instance.UseScope(snsRecord.Sns.ExtractCorrelationId() ?? Guid.NewGuid(), Guid.NewGuid().ToString());
        }
    }
}
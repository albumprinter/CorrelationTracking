using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Lambda.SNSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns.Lambda
{
    public static class CorrelationExtensions
    {
        public static Guid? ExtractCorrelationId(this SNSEvent.SNSMessage snsMessage)
        {
            Guid? correlationId;
            if (snsMessage?.MessageAttributes == null) return null;
            snsMessage.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out var attribute);
            correlationId = Guid.Parse(attribute?.Value);
            return correlationId;
        }
    }
}
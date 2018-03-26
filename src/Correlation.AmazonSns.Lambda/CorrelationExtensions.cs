using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Lambda.SNSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns.Lambda
{
    public static class CorrelationExtensions
    {
        public static Guid? ExtractCorrelationId(this SNSEvent.SNSMessage snsMessage)
        {
            SNSEvent.MessageAttribute attribute;
            var correlationId =
                snsMessage.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out attribute)
                    ? Guid.Parse(attribute.Value)
                    : (Guid?) null;
            return correlationId;
        }
    }
}
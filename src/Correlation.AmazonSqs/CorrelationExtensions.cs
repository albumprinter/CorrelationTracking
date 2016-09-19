using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.SQS.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs
{
    public static class CorrelationExtensions
    {
        public static IDisposable SetCorrelationScopeFromMessage(this Message message)
        {
            var correlationId = ExtractCorrelationId(message);

            return CorrelationManager.Instance.UseScope(correlationId ?? Guid.NewGuid());
        }

        public static Guid? ExtractCorrelationId(this Message message)
        {
            MessageAttributeValue value;
            var correlationId =
                message.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out value)
                    ? Guid.Parse(value.StringValue)
                    : (Guid?) null;

            return correlationId;
        }
    }
}
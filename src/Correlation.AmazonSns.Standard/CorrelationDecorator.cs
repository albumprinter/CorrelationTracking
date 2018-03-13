using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns
{
    public class CorrelationDecorator : AmazonSimpleNotificationServiceClient
    {
        public override Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            AddCorrelationAttributeIfAbsent(request);
            return base.PublishAsync(request, cancellationToken);
        }

        private static void AddCorrelationAttributeIfAbsent(PublishRequest request)
        {
            if (request.MessageAttributes == null)
            {
                request.MessageAttributes = new Dictionary<string, MessageAttributeValue>();
            }

            MessageAttributeValue value;
            if (request.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out value) == false)
            {
                request.MessageAttributes.Add(CorrelationKeys.CorrelationId,
                    new MessageAttributeValue { DataType = "String", StringValue = CorrelationScope.Current.CorrelationId.ToString() });
            }
        }
    }
}

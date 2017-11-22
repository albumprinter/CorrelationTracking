using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns
{
    public class CorrelationSnsClient : AmazonSimpleNotificationServiceClient
    {
        public CorrelationSnsClient(AWSCredentials credentials, RegionEndpoint region) : base(credentials, region)
        {

        }

        public new Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = default(CancellationToken))
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

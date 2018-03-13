using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns
{
    public class CorrelationDecorator : AmazonSimpleNotificationServiceClient
    {
        public CorrelationDecorator()
        {
        }

        public CorrelationDecorator(RegionEndpoint region) : base(region)
        {
        }

        public CorrelationDecorator(AmazonSimpleNotificationServiceConfig config) : base(config)
        {
        }

        public CorrelationDecorator(AWSCredentials credentials) : base(credentials)
        {
        }

        public CorrelationDecorator(AWSCredentials credentials, RegionEndpoint region) : base(credentials, region)
        {
        }

        public CorrelationDecorator(AWSCredentials credentials, AmazonSimpleNotificationServiceConfig clientConfig) : base(credentials, clientConfig)
        {
        }

        public CorrelationDecorator(string awsAccessKeyId, string awsSecretAccessKey) : base(awsAccessKeyId, awsSecretAccessKey)
        {
        }

        public CorrelationDecorator(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region) : base(awsAccessKeyId, awsSecretAccessKey, region)
        {
        }

        public CorrelationDecorator(string awsAccessKeyId, string awsSecretAccessKey, AmazonSimpleNotificationServiceConfig clientConfig) : base(awsAccessKeyId, awsSecretAccessKey, clientConfig)
        {
        }

        public CorrelationDecorator(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken) : base(awsAccessKeyId, awsSecretAccessKey, awsSessionToken)
        {
        }

        public CorrelationDecorator(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken, RegionEndpoint region) : base(awsAccessKeyId, awsSecretAccessKey, awsSessionToken, region)
        {
        }

        public CorrelationDecorator(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken, AmazonSimpleNotificationServiceConfig clientConfig) : base(awsAccessKeyId, awsSecretAccessKey, awsSessionToken, clientConfig)
        {
        }

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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.Runtime.SharedInterfaces;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns
{
    public sealed class CorrelationDecorator : IAmazonSimpleNotificationService
    {
        private readonly IAmazonSimpleNotificationService _client;

        public CorrelationDecorator(IAmazonSimpleNotificationService client)
        {
            _client = client;
        }

        public Task<PublishResponse> PublishAsync(PublishRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            AddCorrelationAttributeIfAbsent(request);
            return _client.PublishAsync(request, cancellationToken);
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
                    new MessageAttributeValue
                        {DataType = "String", StringValue = CorrelationScope.Current.CorrelationId.ToString()});
            }
        }

        #region Delegated members

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<string> SubscribeQueueAsync(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueAsync(topicArn, sqsClient, sqsQueueUrl);
        }

        public Task<IDictionary<string, string>> SubscribeQueueToTopicsAsync(IList<string> topicArns,
            ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueToTopicsAsync(topicArns, sqsClient, sqsQueueUrl);
        }

        public Task<Topic> FindTopicAsync(string topicName)
        {
            return _client.FindTopicAsync(topicName);
        }

        public Task AuthorizeS3ToPublishAsync(string topicArn, string bucket)
        {
            return _client.AuthorizeS3ToPublishAsync(topicArn, bucket);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(string topicArn, string label, List<string> awsAccountId,
            List<string> actionName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(topicArn, label, awsAccountId, actionName, cancellationToken);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(request, cancellationToken);
        }

        public Task<CheckIfPhoneNumberIsOptedOutResponse> CheckIfPhoneNumberIsOptedOutAsync(
            CheckIfPhoneNumberIsOptedOutRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CheckIfPhoneNumberIsOptedOutAsync(request, cancellationToken);
        }

        public Task<ConfirmSubscriptionResponse> ConfirmSubscriptionAsync(string topicArn, string token,
            string authenticateOnUnsubscribe,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ConfirmSubscriptionAsync(topicArn, token, authenticateOnUnsubscribe, cancellationToken);
        }

        public Task<ConfirmSubscriptionResponse> ConfirmSubscriptionAsync(string topicArn, string token,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ConfirmSubscriptionAsync(topicArn, token, cancellationToken);
        }

        public Task<ConfirmSubscriptionResponse> ConfirmSubscriptionAsync(ConfirmSubscriptionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ConfirmSubscriptionAsync(request, cancellationToken);
        }

        public Task<CreatePlatformApplicationResponse> CreatePlatformApplicationAsync(
            CreatePlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreatePlatformApplicationAsync(request, cancellationToken);
        }

        public Task<CreatePlatformEndpointResponse> CreatePlatformEndpointAsync(CreatePlatformEndpointRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreatePlatformEndpointAsync(request, cancellationToken);
        }

        public Task<CreateTopicResponse> CreateTopicAsync(string name,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateTopicAsync(name, cancellationToken);
        }

        public Task<CreateTopicResponse> CreateTopicAsync(CreateTopicRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateTopicAsync(request, cancellationToken);
        }

        public Task<DeleteEndpointResponse> DeleteEndpointAsync(DeleteEndpointRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteEndpointAsync(request, cancellationToken);
        }

        public Task<DeletePlatformApplicationResponse> DeletePlatformApplicationAsync(
            DeletePlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeletePlatformApplicationAsync(request, cancellationToken);
        }

        public Task<DeleteTopicResponse> DeleteTopicAsync(string topicArn,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteTopicAsync(topicArn, cancellationToken);
        }

        public Task<DeleteTopicResponse> DeleteTopicAsync(DeleteTopicRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteTopicAsync(request, cancellationToken);
        }

        public Task<GetEndpointAttributesResponse> GetEndpointAttributesAsync(GetEndpointAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetEndpointAttributesAsync(request, cancellationToken);
        }

        public Task<GetPlatformApplicationAttributesResponse> GetPlatformApplicationAttributesAsync(
            GetPlatformApplicationAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetPlatformApplicationAttributesAsync(request, cancellationToken);
        }

        public Task<GetSMSAttributesResponse> GetSMSAttributesAsync(GetSMSAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSMSAttributesAsync(request, cancellationToken);
        }

        public Task<GetSubscriptionAttributesResponse> GetSubscriptionAttributesAsync(string subscriptionArn,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSubscriptionAttributesAsync(subscriptionArn, cancellationToken);
        }

        public Task<GetSubscriptionAttributesResponse> GetSubscriptionAttributesAsync(
            GetSubscriptionAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSubscriptionAttributesAsync(request, cancellationToken);
        }

        public Task<GetTopicAttributesResponse> GetTopicAttributesAsync(string topicArn,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetTopicAttributesAsync(topicArn, cancellationToken);
        }

        public Task<GetTopicAttributesResponse> GetTopicAttributesAsync(GetTopicAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetTopicAttributesAsync(request, cancellationToken);
        }

        public Task<ListEndpointsByPlatformApplicationResponse> ListEndpointsByPlatformApplicationAsync(
            ListEndpointsByPlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListEndpointsByPlatformApplicationAsync(request, cancellationToken);
        }

        public Task<ListPhoneNumbersOptedOutResponse> ListPhoneNumbersOptedOutAsync(
            ListPhoneNumbersOptedOutRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPhoneNumbersOptedOutAsync(request, cancellationToken);
        }

        public Task<ListPlatformApplicationsResponse> ListPlatformApplicationsAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPlatformApplicationsAsync(cancellationToken);
        }

        public Task<ListPlatformApplicationsResponse> ListPlatformApplicationsAsync(
            ListPlatformApplicationsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPlatformApplicationsAsync(request, cancellationToken);
        }

        public Task<ListSubscriptionsResponse> ListSubscriptionsAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsAsync(cancellationToken);
        }

        public Task<ListSubscriptionsResponse> ListSubscriptionsAsync(string nextToken,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsAsync(nextToken, cancellationToken);
        }

        public Task<ListSubscriptionsResponse> ListSubscriptionsAsync(ListSubscriptionsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsAsync(request, cancellationToken);
        }

        public Task<ListSubscriptionsByTopicResponse> ListSubscriptionsByTopicAsync(string topicArn, string nextToken,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsByTopicAsync(topicArn, nextToken, cancellationToken);
        }

        public Task<ListSubscriptionsByTopicResponse> ListSubscriptionsByTopicAsync(string topicArn,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsByTopicAsync(topicArn, cancellationToken);
        }

        public Task<ListSubscriptionsByTopicResponse> ListSubscriptionsByTopicAsync(
            ListSubscriptionsByTopicRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsByTopicAsync(request, cancellationToken);
        }

        public Task<ListTagsForResourceResponse> ListTagsForResourceAsync(ListTagsForResourceRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTagsForResourceAsync(request, cancellationToken);
        }

        public Task<ListTopicsResponse> ListTopicsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTopicsAsync(cancellationToken);
        }

        public Task<ListTopicsResponse> ListTopicsAsync(string nextToken,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTopicsAsync(nextToken, cancellationToken);
        }

        public Task<ListTopicsResponse> ListTopicsAsync(ListTopicsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTopicsAsync(request, cancellationToken);
        }

        public Task<OptInPhoneNumberResponse> OptInPhoneNumberAsync(OptInPhoneNumberRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.OptInPhoneNumberAsync(request, cancellationToken);
        }

        public Task<PublishResponse> PublishAsync(string topicArn, string message,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PublishAsync(topicArn, message, cancellationToken);
        }

        public Task<PublishResponse> PublishAsync(string topicArn, string message, string subject,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PublishAsync(topicArn, message, subject, cancellationToken);
        }

        public Task<RemovePermissionResponse> RemovePermissionAsync(string topicArn, string label,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(topicArn, label, cancellationToken);
        }

        public Task<RemovePermissionResponse> RemovePermissionAsync(RemovePermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(request, cancellationToken);
        }

        public Task<SetEndpointAttributesResponse> SetEndpointAttributesAsync(SetEndpointAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetEndpointAttributesAsync(request, cancellationToken);
        }

        public Task<SetPlatformApplicationAttributesResponse> SetPlatformApplicationAttributesAsync(
            SetPlatformApplicationAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetPlatformApplicationAttributesAsync(request, cancellationToken);
        }


        public Task<SetSMSAttributesResponse> SetSMSAttributesAsync(SetSMSAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSMSAttributesAsync(request, cancellationToken);
        }


        public Task<SetSubscriptionAttributesResponse> SetSubscriptionAttributesAsync(string subscriptionArn,
            string attributeName, string attributeValue,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSubscriptionAttributesAsync(subscriptionArn, attributeName, attributeValue,
                cancellationToken);
        }

        public Task<SetSubscriptionAttributesResponse> SetSubscriptionAttributesAsync(
            SetSubscriptionAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSubscriptionAttributesAsync(request, cancellationToken);
        }

        public Task<SetTopicAttributesResponse> SetTopicAttributesAsync(string topicArn, string attributeName,
            string attributeValue,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetTopicAttributesAsync(topicArn, attributeName, attributeValue, cancellationToken);
        }

        public Task<SetTopicAttributesResponse> SetTopicAttributesAsync(SetTopicAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetTopicAttributesAsync(request, cancellationToken);
        }


        public Task<SubscribeResponse> SubscribeAsync(string topicArn, string protocol, string endpoint,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SubscribeAsync(topicArn, protocol, endpoint, cancellationToken);
        }

        public Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SubscribeAsync(request, cancellationToken);
        }

        public Task<TagResourceResponse> TagResourceAsync(TagResourceRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.TagResourceAsync(request, cancellationToken);
        }

        public Task<UnsubscribeResponse> UnsubscribeAsync(string subscriptionArn,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UnsubscribeAsync(subscriptionArn, cancellationToken);
        }

        public Task<UnsubscribeResponse> UnsubscribeAsync(UnsubscribeRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UnsubscribeAsync(request, cancellationToken);
        }

        public Task<UntagResourceResponse> UntagResourceAsync(UntagResourceRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UntagResourceAsync(request, cancellationToken);
        }

        #endregion

        public IClientConfig Config => _client.Config;
    }
}
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
    public class CorrelationDecorator : IAmazonSimpleNotificationService
    {
        private readonly IAmazonSimpleNotificationService _client;

        public CorrelationDecorator(IAmazonSimpleNotificationService client)
        {
            _client = client;
        }

        public Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = new CancellationToken())
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
                    new MessageAttributeValue { DataType = "String", StringValue = CorrelationScope.Current.CorrelationId.ToString() });
            }
        }

        #region Delegated members
        public void Dispose()
        {
            _client.Dispose();
        }

#if !IS_MIN_NETSTANDARD1_3
        public string SubscribeQueue(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueue(topicArn, sqsClient, sqsQueueUrl);
        }
#endif

        public Task<string> SubscribeQueueAsync(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueAsync(topicArn, sqsClient, sqsQueueUrl);
        }

#if !IS_MIN_NETSTANDARD1_3
        public IDictionary<string, string> SubscribeQueueToTopics(IList<string> topicArns, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueToTopics(topicArns, sqsClient, sqsQueueUrl);
        }
#endif

        public Task<IDictionary<string, string>> SubscribeQueueToTopicsAsync(IList<string> topicArns, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueToTopicsAsync(topicArns, sqsClient, sqsQueueUrl);
        }

#if !IS_MIN_NETSTANDARD1_3
        public Topic FindTopic(string topicName)
        {
            return _client.FindTopic(topicName);
        }
#endif

        public Task<Topic> FindTopicAsync(string topicName)
        {
            return _client.FindTopicAsync(topicName);
        }

#if !IS_MIN_NETSTANDARD1_3
        public void AuthorizeS3ToPublish(string topicArn, string bucket)
        {
            _client.AuthorizeS3ToPublish(topicArn, bucket);
        }
#endif

        public Task AuthorizeS3ToPublishAsync(string topicArn, string bucket)
        {
            return _client.AuthorizeS3ToPublishAsync(topicArn, bucket);
        }

#if !IS_MIN_NETSTANDARD1_3
        public AddPermissionResponse AddPermission(string topicArn, string label, List<string> awsAccountId, List<string> actionName)
        {
            return _client.AddPermission(topicArn, label, awsAccountId, actionName);
        }

        public AddPermissionResponse AddPermission(AddPermissionRequest request)
        {
            return _client.AddPermission(request);
        }
#endif

        public Task<AddPermissionResponse> AddPermissionAsync(string topicArn, string label, List<string> awsAccountId, List<string> actionName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(topicArn, label, awsAccountId, actionName, cancellationToken);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public CheckIfPhoneNumberIsOptedOutResponse CheckIfPhoneNumberIsOptedOut(CheckIfPhoneNumberIsOptedOutRequest request)
        {
            return _client.CheckIfPhoneNumberIsOptedOut(request);
        }
#endif

        public Task<CheckIfPhoneNumberIsOptedOutResponse> CheckIfPhoneNumberIsOptedOutAsync(CheckIfPhoneNumberIsOptedOutRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CheckIfPhoneNumberIsOptedOutAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ConfirmSubscriptionResponse ConfirmSubscription(string topicArn, string token, string authenticateOnUnsubscribe)
        {
            return _client.ConfirmSubscription(topicArn, token, authenticateOnUnsubscribe);
        }

        public ConfirmSubscriptionResponse ConfirmSubscription(string topicArn, string token)
        {
            return _client.ConfirmSubscription(topicArn, token);
        }

        public ConfirmSubscriptionResponse ConfirmSubscription(ConfirmSubscriptionRequest request)
        {
            return _client.ConfirmSubscription(request);
        }
#endif

        public Task<ConfirmSubscriptionResponse> ConfirmSubscriptionAsync(string topicArn, string token, string authenticateOnUnsubscribe,
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

#if !IS_MIN_NETSTANDARD1_3
        public CreatePlatformApplicationResponse CreatePlatformApplication(CreatePlatformApplicationRequest request)
        {
            return _client.CreatePlatformApplication(request);
        }
#endif

        public Task<CreatePlatformApplicationResponse> CreatePlatformApplicationAsync(CreatePlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreatePlatformApplicationAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public CreatePlatformEndpointResponse CreatePlatformEndpoint(CreatePlatformEndpointRequest request)
        {
            return _client.CreatePlatformEndpoint(request);
        }
#endif

        public Task<CreatePlatformEndpointResponse> CreatePlatformEndpointAsync(CreatePlatformEndpointRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreatePlatformEndpointAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public CreateTopicResponse CreateTopic(string name)
        {
            return _client.CreateTopic(name);
        }

        public CreateTopicResponse CreateTopic(CreateTopicRequest request)
        {
            return _client.CreateTopic(request);
        }
#endif

        public Task<CreateTopicResponse> CreateTopicAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateTopicAsync(name, cancellationToken);
        }

        public Task<CreateTopicResponse> CreateTopicAsync(CreateTopicRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateTopicAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public DeleteEndpointResponse DeleteEndpoint(DeleteEndpointRequest request)
        {
            return _client.DeleteEndpoint(request);
        }
#endif

        public Task<DeleteEndpointResponse> DeleteEndpointAsync(DeleteEndpointRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteEndpointAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public DeletePlatformApplicationResponse DeletePlatformApplication(DeletePlatformApplicationRequest request)
        {
            return _client.DeletePlatformApplication(request);
        }
#endif

        public Task<DeletePlatformApplicationResponse> DeletePlatformApplicationAsync(DeletePlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeletePlatformApplicationAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public DeleteTopicResponse DeleteTopic(string topicArn)
        {
            return _client.DeleteTopic(topicArn);
        }

        public DeleteTopicResponse DeleteTopic(DeleteTopicRequest request)
        {
            return _client.DeleteTopic(request);
        }
#endif

        public Task<DeleteTopicResponse> DeleteTopicAsync(string topicArn, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteTopicAsync(topicArn, cancellationToken);
        }

        public Task<DeleteTopicResponse> DeleteTopicAsync(DeleteTopicRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteTopicAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetEndpointAttributesResponse GetEndpointAttributes(GetEndpointAttributesRequest request)
        {
            return _client.GetEndpointAttributes(request);
        }
#endif

        public Task<GetEndpointAttributesResponse> GetEndpointAttributesAsync(GetEndpointAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetEndpointAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetPlatformApplicationAttributesResponse GetPlatformApplicationAttributes(
            GetPlatformApplicationAttributesRequest request)
        {
            return _client.GetPlatformApplicationAttributes(request);
        }

#endif
        public Task<GetPlatformApplicationAttributesResponse> GetPlatformApplicationAttributesAsync(GetPlatformApplicationAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetPlatformApplicationAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetSMSAttributesResponse GetSMSAttributes(GetSMSAttributesRequest request)
        {
            return _client.GetSMSAttributes(request);
        }
#endif

        public Task<GetSMSAttributesResponse> GetSMSAttributesAsync(GetSMSAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSMSAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetSubscriptionAttributesResponse GetSubscriptionAttributes(string subscriptionArn)
        {
            return _client.GetSubscriptionAttributes(subscriptionArn);
        }

        public GetSubscriptionAttributesResponse GetSubscriptionAttributes(GetSubscriptionAttributesRequest request)
        {
            return _client.GetSubscriptionAttributes(request);
        }
#endif

        public Task<GetSubscriptionAttributesResponse> GetSubscriptionAttributesAsync(string subscriptionArn,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSubscriptionAttributesAsync(subscriptionArn, cancellationToken);
        }

        public Task<GetSubscriptionAttributesResponse> GetSubscriptionAttributesAsync(GetSubscriptionAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSubscriptionAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetTopicAttributesResponse GetTopicAttributes(string topicArn)
        {
            return _client.GetTopicAttributes(topicArn);
        }

        public GetTopicAttributesResponse GetTopicAttributes(GetTopicAttributesRequest request)
        {
            return _client.GetTopicAttributes(request);
        }
#endif

        public Task<GetTopicAttributesResponse> GetTopicAttributesAsync(string topicArn, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetTopicAttributesAsync(topicArn, cancellationToken);
        }

        public Task<GetTopicAttributesResponse> GetTopicAttributesAsync(GetTopicAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetTopicAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListEndpointsByPlatformApplicationResponse ListEndpointsByPlatformApplication(
            ListEndpointsByPlatformApplicationRequest request)
        {
            return _client.ListEndpointsByPlatformApplication(request);
        }
#endif

        public Task<ListEndpointsByPlatformApplicationResponse> ListEndpointsByPlatformApplicationAsync(ListEndpointsByPlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListEndpointsByPlatformApplicationAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListPhoneNumbersOptedOutResponse ListPhoneNumbersOptedOut(ListPhoneNumbersOptedOutRequest request)
        {
            return _client.ListPhoneNumbersOptedOut(request);
        }
#endif

        public Task<ListPhoneNumbersOptedOutResponse> ListPhoneNumbersOptedOutAsync(ListPhoneNumbersOptedOutRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPhoneNumbersOptedOutAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListPlatformApplicationsResponse ListPlatformApplications()
        {
            return _client.ListPlatformApplications();
        }

        public ListPlatformApplicationsResponse ListPlatformApplications(ListPlatformApplicationsRequest request)
        {
            return _client.ListPlatformApplications(request);
        }
#endif

        public Task<ListPlatformApplicationsResponse> ListPlatformApplicationsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPlatformApplicationsAsync(cancellationToken);
        }

        public Task<ListPlatformApplicationsResponse> ListPlatformApplicationsAsync(ListPlatformApplicationsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPlatformApplicationsAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListSubscriptionsResponse ListSubscriptions()
        {
            return _client.ListSubscriptions();
        }

        public ListSubscriptionsResponse ListSubscriptions(string nextToken)
        {
            return _client.ListSubscriptions(nextToken);
        }

        public ListSubscriptionsResponse ListSubscriptions(ListSubscriptionsRequest request)
        {
            return _client.ListSubscriptions(request);
        }
#endif

        public Task<ListSubscriptionsResponse> ListSubscriptionsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsAsync(cancellationToken);
        }

        public Task<ListSubscriptionsResponse> ListSubscriptionsAsync(string nextToken, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsAsync(nextToken, cancellationToken);
        }

        public Task<ListSubscriptionsResponse> ListSubscriptionsAsync(ListSubscriptionsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListSubscriptionsByTopicResponse ListSubscriptionsByTopic(string topicArn, string nextToken)
        {
            return _client.ListSubscriptionsByTopic(topicArn, nextToken);
        }

        public ListSubscriptionsByTopicResponse ListSubscriptionsByTopic(string topicArn)
        {
            return _client.ListSubscriptionsByTopic(topicArn);
        }

        public ListSubscriptionsByTopicResponse ListSubscriptionsByTopic(ListSubscriptionsByTopicRequest request)
        {
            return _client.ListSubscriptionsByTopic(request);
        }
#endif

        public Task<ListSubscriptionsByTopicResponse> ListSubscriptionsByTopicAsync(string topicArn, string nextToken,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsByTopicAsync(topicArn, nextToken, cancellationToken);
        }

        public Task<ListSubscriptionsByTopicResponse> ListSubscriptionsByTopicAsync(string topicArn, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsByTopicAsync(topicArn, cancellationToken);
        }

        public Task<ListSubscriptionsByTopicResponse> ListSubscriptionsByTopicAsync(ListSubscriptionsByTopicRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListSubscriptionsByTopicAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListTopicsResponse ListTopics()
        {
            return _client.ListTopics();
        }

        public ListTopicsResponse ListTopics(string nextToken)
        {
            return _client.ListTopics(nextToken);
        }

        public ListTopicsResponse ListTopics(ListTopicsRequest request)
        {
            return _client.ListTopics(request);
        }
#endif

        public Task<ListTopicsResponse> ListTopicsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTopicsAsync(cancellationToken);
        }

        public Task<ListTopicsResponse> ListTopicsAsync(string nextToken, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTopicsAsync(nextToken, cancellationToken);
        }

        public Task<ListTopicsResponse> ListTopicsAsync(ListTopicsRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListTopicsAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public OptInPhoneNumberResponse OptInPhoneNumber(OptInPhoneNumberRequest request)
        {
            return _client.OptInPhoneNumber(request);
        }
#endif

        public Task<OptInPhoneNumberResponse> OptInPhoneNumberAsync(OptInPhoneNumberRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.OptInPhoneNumberAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public PublishResponse Publish(string topicArn, string message)
        {
            return _client.Publish(topicArn, message);
        }

        public PublishResponse Publish(string topicArn, string message, string subject)
        {
            return _client.Publish(topicArn, message, subject);
        }

        public PublishResponse Publish(PublishRequest request)
        {
            return _client.Publish(request);
        }
#endif

        public Task<PublishResponse> PublishAsync(string topicArn, string message, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PublishAsync(topicArn, message, cancellationToken);
        }

        public Task<PublishResponse> PublishAsync(string topicArn, string message, string subject,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PublishAsync(topicArn, message, subject, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public RemovePermissionResponse RemovePermission(string topicArn, string label)
        {
            return _client.RemovePermission(topicArn, label);
        }

        public RemovePermissionResponse RemovePermission(RemovePermissionRequest request)
        {
            return _client.RemovePermission(request);
        }
#endif

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

#if !IS_MIN_NETSTANDARD1_3
        public SetEndpointAttributesResponse SetEndpointAttributes(SetEndpointAttributesRequest request)
        {
            return _client.SetEndpointAttributes(request);
        }
#endif

        public Task<SetEndpointAttributesResponse> SetEndpointAttributesAsync(SetEndpointAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetEndpointAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SetPlatformApplicationAttributesResponse SetPlatformApplicationAttributes(
            SetPlatformApplicationAttributesRequest request)
        {
            return _client.SetPlatformApplicationAttributes(request);
        }
#endif

        public Task<SetPlatformApplicationAttributesResponse> SetPlatformApplicationAttributesAsync(SetPlatformApplicationAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetPlatformApplicationAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SetSMSAttributesResponse SetSMSAttributes(SetSMSAttributesRequest request)
        {
            return _client.SetSMSAttributes(request);
        }
#endif

        public Task<SetSMSAttributesResponse> SetSMSAttributesAsync(SetSMSAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSMSAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SetSubscriptionAttributesResponse SetSubscriptionAttributes(string subscriptionArn, string attributeName,
            string attributeValue)
        {
            return _client.SetSubscriptionAttributes(subscriptionArn, attributeName, attributeValue);
        }

        public SetSubscriptionAttributesResponse SetSubscriptionAttributes(SetSubscriptionAttributesRequest request)
        {
            return _client.SetSubscriptionAttributes(request);
        }
#endif

        public Task<SetSubscriptionAttributesResponse> SetSubscriptionAttributesAsync(string subscriptionArn, string attributeName, string attributeValue,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSubscriptionAttributesAsync(subscriptionArn, attributeName, attributeValue, cancellationToken);
        }

        public Task<SetSubscriptionAttributesResponse> SetSubscriptionAttributesAsync(SetSubscriptionAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSubscriptionAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SetTopicAttributesResponse SetTopicAttributes(string topicArn, string attributeName, string attributeValue)
        {
            return _client.SetTopicAttributes(topicArn, attributeName, attributeValue);
        }

        public SetTopicAttributesResponse SetTopicAttributes(SetTopicAttributesRequest request)
        {
            return _client.SetTopicAttributes(request);
        }
#endif

        public Task<SetTopicAttributesResponse> SetTopicAttributesAsync(string topicArn, string attributeName, string attributeValue,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetTopicAttributesAsync(topicArn, attributeName, attributeValue, cancellationToken);
        }

        public Task<SetTopicAttributesResponse> SetTopicAttributesAsync(SetTopicAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetTopicAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SubscribeResponse Subscribe(string topicArn, string protocol, string endpoint)
        {
            return _client.Subscribe(topicArn, protocol, endpoint);
        }

        public SubscribeResponse Subscribe(SubscribeRequest request)
        {
            return _client.Subscribe(request);
        }
#endif

        public Task<SubscribeResponse> SubscribeAsync(string topicArn, string protocol, string endpoint,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SubscribeAsync(topicArn, protocol, endpoint, cancellationToken);
        }

        public Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SubscribeAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public UnsubscribeResponse Unsubscribe(string subscriptionArn)
        {
            return _client.Unsubscribe(subscriptionArn);
        }

        public UnsubscribeResponse Unsubscribe(UnsubscribeRequest request)
        {
            return _client.Unsubscribe(request);
        }
#endif

        public Task<UnsubscribeResponse> UnsubscribeAsync(string subscriptionArn, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UnsubscribeAsync(subscriptionArn, cancellationToken);
        }

        public Task<UnsubscribeResponse> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UnsubscribeAsync(request, cancellationToken);
        }
        #endregion

        public IClientConfig Config => _client.Config;
    }
}
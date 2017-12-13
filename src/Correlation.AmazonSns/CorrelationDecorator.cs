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

        public string SubscribeQueue(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueue(topicArn, sqsClient, sqsQueueUrl);
        }

        public Task<string> SubscribeQueueAsync(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueAsync(topicArn, sqsClient, sqsQueueUrl);
        }

        public IDictionary<string, string> SubscribeQueueToTopics(IList<string> topicArns, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueToTopics(topicArns, sqsClient, sqsQueueUrl);
        }

        public Task<IDictionary<string, string>> SubscribeQueueToTopicsAsync(IList<string> topicArns, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            return _client.SubscribeQueueToTopicsAsync(topicArns, sqsClient, sqsQueueUrl);
        }

        public Topic FindTopic(string topicName)
        {
            return _client.FindTopic(topicName);
        }

        public Task<Topic> FindTopicAsync(string topicName)
        {
            return _client.FindTopicAsync(topicName);
        }

        public void AuthorizeS3ToPublish(string topicArn, string bucket)
        {
            _client.AuthorizeS3ToPublish(topicArn, bucket);
        }

        public Task AuthorizeS3ToPublishAsync(string topicArn, string bucket)
        {
            return _client.AuthorizeS3ToPublishAsync(topicArn, bucket);
        }

        public AddPermissionResponse AddPermission(string topicArn, string label, List<string> awsAccountId, List<string> actionName)
        {
            return _client.AddPermission(topicArn, label, awsAccountId, actionName);
        }

        public AddPermissionResponse AddPermission(AddPermissionRequest request)
        {
            return _client.AddPermission(request);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(string topicArn, string label, List<string> awsAccountId, List<string> actionName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(topicArn, label, awsAccountId, actionName, cancellationToken);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(request, cancellationToken);
        }

        public CheckIfPhoneNumberIsOptedOutResponse CheckIfPhoneNumberIsOptedOut(CheckIfPhoneNumberIsOptedOutRequest request)
        {
            return _client.CheckIfPhoneNumberIsOptedOut(request);
        }

        public Task<CheckIfPhoneNumberIsOptedOutResponse> CheckIfPhoneNumberIsOptedOutAsync(CheckIfPhoneNumberIsOptedOutRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CheckIfPhoneNumberIsOptedOutAsync(request, cancellationToken);
        }

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

        public CreatePlatformApplicationResponse CreatePlatformApplication(CreatePlatformApplicationRequest request)
        {
            return _client.CreatePlatformApplication(request);
        }

        public Task<CreatePlatformApplicationResponse> CreatePlatformApplicationAsync(CreatePlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreatePlatformApplicationAsync(request, cancellationToken);
        }

        public CreatePlatformEndpointResponse CreatePlatformEndpoint(CreatePlatformEndpointRequest request)
        {
            return _client.CreatePlatformEndpoint(request);
        }

        public Task<CreatePlatformEndpointResponse> CreatePlatformEndpointAsync(CreatePlatformEndpointRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreatePlatformEndpointAsync(request, cancellationToken);
        }

        public CreateTopicResponse CreateTopic(string name)
        {
            return _client.CreateTopic(name);
        }

        public CreateTopicResponse CreateTopic(CreateTopicRequest request)
        {
            return _client.CreateTopic(request);
        }

        public Task<CreateTopicResponse> CreateTopicAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateTopicAsync(name, cancellationToken);
        }

        public Task<CreateTopicResponse> CreateTopicAsync(CreateTopicRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateTopicAsync(request, cancellationToken);
        }

        public DeleteEndpointResponse DeleteEndpoint(DeleteEndpointRequest request)
        {
            return _client.DeleteEndpoint(request);
        }

        public Task<DeleteEndpointResponse> DeleteEndpointAsync(DeleteEndpointRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteEndpointAsync(request, cancellationToken);
        }

        public DeletePlatformApplicationResponse DeletePlatformApplication(DeletePlatformApplicationRequest request)
        {
            return _client.DeletePlatformApplication(request);
        }

        public Task<DeletePlatformApplicationResponse> DeletePlatformApplicationAsync(DeletePlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeletePlatformApplicationAsync(request, cancellationToken);
        }

        public DeleteTopicResponse DeleteTopic(string topicArn)
        {
            return _client.DeleteTopic(topicArn);
        }

        public DeleteTopicResponse DeleteTopic(DeleteTopicRequest request)
        {
            return _client.DeleteTopic(request);
        }

        public Task<DeleteTopicResponse> DeleteTopicAsync(string topicArn, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteTopicAsync(topicArn, cancellationToken);
        }

        public Task<DeleteTopicResponse> DeleteTopicAsync(DeleteTopicRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteTopicAsync(request, cancellationToken);
        }

        public GetEndpointAttributesResponse GetEndpointAttributes(GetEndpointAttributesRequest request)
        {
            return _client.GetEndpointAttributes(request);
        }

        public Task<GetEndpointAttributesResponse> GetEndpointAttributesAsync(GetEndpointAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetEndpointAttributesAsync(request, cancellationToken);
        }

        public GetPlatformApplicationAttributesResponse GetPlatformApplicationAttributes(
            GetPlatformApplicationAttributesRequest request)
        {
            return _client.GetPlatformApplicationAttributes(request);
        }

        public Task<GetPlatformApplicationAttributesResponse> GetPlatformApplicationAttributesAsync(GetPlatformApplicationAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetPlatformApplicationAttributesAsync(request, cancellationToken);
        }

        public GetSMSAttributesResponse GetSMSAttributes(GetSMSAttributesRequest request)
        {
            return _client.GetSMSAttributes(request);
        }

        public Task<GetSMSAttributesResponse> GetSMSAttributesAsync(GetSMSAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetSMSAttributesAsync(request, cancellationToken);
        }

        public GetSubscriptionAttributesResponse GetSubscriptionAttributes(string subscriptionArn)
        {
            return _client.GetSubscriptionAttributes(subscriptionArn);
        }

        public GetSubscriptionAttributesResponse GetSubscriptionAttributes(GetSubscriptionAttributesRequest request)
        {
            return _client.GetSubscriptionAttributes(request);
        }

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

        public GetTopicAttributesResponse GetTopicAttributes(string topicArn)
        {
            return _client.GetTopicAttributes(topicArn);
        }

        public GetTopicAttributesResponse GetTopicAttributes(GetTopicAttributesRequest request)
        {
            return _client.GetTopicAttributes(request);
        }

        public Task<GetTopicAttributesResponse> GetTopicAttributesAsync(string topicArn, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetTopicAttributesAsync(topicArn, cancellationToken);
        }

        public Task<GetTopicAttributesResponse> GetTopicAttributesAsync(GetTopicAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetTopicAttributesAsync(request, cancellationToken);
        }

        public ListEndpointsByPlatformApplicationResponse ListEndpointsByPlatformApplication(
            ListEndpointsByPlatformApplicationRequest request)
        {
            return _client.ListEndpointsByPlatformApplication(request);
        }

        public Task<ListEndpointsByPlatformApplicationResponse> ListEndpointsByPlatformApplicationAsync(ListEndpointsByPlatformApplicationRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListEndpointsByPlatformApplicationAsync(request, cancellationToken);
        }

        public ListPhoneNumbersOptedOutResponse ListPhoneNumbersOptedOut(ListPhoneNumbersOptedOutRequest request)
        {
            return _client.ListPhoneNumbersOptedOut(request);
        }

        public Task<ListPhoneNumbersOptedOutResponse> ListPhoneNumbersOptedOutAsync(ListPhoneNumbersOptedOutRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPhoneNumbersOptedOutAsync(request, cancellationToken);
        }

        public ListPlatformApplicationsResponse ListPlatformApplications()
        {
            return _client.ListPlatformApplications();
        }

        public ListPlatformApplicationsResponse ListPlatformApplications(ListPlatformApplicationsRequest request)
        {
            return _client.ListPlatformApplications(request);
        }

        public Task<ListPlatformApplicationsResponse> ListPlatformApplicationsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPlatformApplicationsAsync(cancellationToken);
        }

        public Task<ListPlatformApplicationsResponse> ListPlatformApplicationsAsync(ListPlatformApplicationsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListPlatformApplicationsAsync(request, cancellationToken);
        }

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

        public OptInPhoneNumberResponse OptInPhoneNumber(OptInPhoneNumberRequest request)
        {
            return _client.OptInPhoneNumber(request);
        }

        public Task<OptInPhoneNumberResponse> OptInPhoneNumberAsync(OptInPhoneNumberRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.OptInPhoneNumberAsync(request, cancellationToken);
        }

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

        public Task<PublishResponse> PublishAsync(string topicArn, string message, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PublishAsync(topicArn, message, cancellationToken);
        }

        public Task<PublishResponse> PublishAsync(string topicArn, string message, string subject,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PublishAsync(topicArn, message, subject, cancellationToken);
        }

        public RemovePermissionResponse RemovePermission(string topicArn, string label)
        {
            return _client.RemovePermission(topicArn, label);
        }

        public RemovePermissionResponse RemovePermission(RemovePermissionRequest request)
        {
            return _client.RemovePermission(request);
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

        public SetEndpointAttributesResponse SetEndpointAttributes(SetEndpointAttributesRequest request)
        {
            return _client.SetEndpointAttributes(request);
        }

        public Task<SetEndpointAttributesResponse> SetEndpointAttributesAsync(SetEndpointAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetEndpointAttributesAsync(request, cancellationToken);
        }

        public SetPlatformApplicationAttributesResponse SetPlatformApplicationAttributes(
            SetPlatformApplicationAttributesRequest request)
        {
            return _client.SetPlatformApplicationAttributes(request);
        }

        public Task<SetPlatformApplicationAttributesResponse> SetPlatformApplicationAttributesAsync(SetPlatformApplicationAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetPlatformApplicationAttributesAsync(request, cancellationToken);
        }

        public SetSMSAttributesResponse SetSMSAttributes(SetSMSAttributesRequest request)
        {
            return _client.SetSMSAttributes(request);
        }

        public Task<SetSMSAttributesResponse> SetSMSAttributesAsync(SetSMSAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetSMSAttributesAsync(request, cancellationToken);
        }

        public SetSubscriptionAttributesResponse SetSubscriptionAttributes(string subscriptionArn, string attributeName,
            string attributeValue)
        {
            return _client.SetSubscriptionAttributes(subscriptionArn, attributeName, attributeValue);
        }

        public SetSubscriptionAttributesResponse SetSubscriptionAttributes(SetSubscriptionAttributesRequest request)
        {
            return _client.SetSubscriptionAttributes(request);
        }

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

        public SetTopicAttributesResponse SetTopicAttributes(string topicArn, string attributeName, string attributeValue)
        {
            return _client.SetTopicAttributes(topicArn, attributeName, attributeValue);
        }

        public SetTopicAttributesResponse SetTopicAttributes(SetTopicAttributesRequest request)
        {
            return _client.SetTopicAttributes(request);
        }

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

        public SubscribeResponse Subscribe(string topicArn, string protocol, string endpoint)
        {
            return _client.Subscribe(topicArn, protocol, endpoint);
        }

        public SubscribeResponse Subscribe(SubscribeRequest request)
        {
            return _client.Subscribe(request);
        }

        public Task<SubscribeResponse> SubscribeAsync(string topicArn, string protocol, string endpoint,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SubscribeAsync(topicArn, protocol, endpoint, cancellationToken);
        }

        public Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SubscribeAsync(request, cancellationToken);
        }

        public UnsubscribeResponse Unsubscribe(string subscriptionArn)
        {
            return _client.Unsubscribe(subscriptionArn);
        }

        public UnsubscribeResponse Unsubscribe(UnsubscribeRequest request)
        {
            return _client.Unsubscribe(request);
        }

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

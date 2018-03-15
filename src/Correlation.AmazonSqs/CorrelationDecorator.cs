using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs
{
    public class CorrelationDecorator : IAmazonSQS
    {
        private readonly IAmazonSQS _client;

        public CorrelationDecorator(IAmazonSQS client)
        {
            _client = client;
        }

#if !IS_MIN_NETSTANDARD1_3
        public ReceiveMessageResponse ReceiveMessage(string queueUrl)
        {
            var request = new ReceiveMessageRequest { QueueUrl = queueUrl, MessageAttributeNames = new List<string> { CorrelationKeys.CorrelationId } };
            return _client.ReceiveMessage(request);
        }

        public ReceiveMessageResponse ReceiveMessage(ReceiveMessageRequest request)
        {
            request.MessageAttributeNames.Add(CorrelationKeys.CorrelationId);
            return _client.ReceiveMessage(request);
        }
#endif

        public Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            var request = new ReceiveMessageRequest { QueueUrl = queueUrl, MessageAttributeNames = new List<string> { CorrelationKeys.CorrelationId } };
            return _client.ReceiveMessageAsync(request, cancellationToken);
        }

        public Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            request.MessageAttributeNames.Add(CorrelationKeys.CorrelationId);
            return _client.ReceiveMessageAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SendMessageResponse SendMessage(string queueUrl, string messageBody)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessage(request);
        }

        public SendMessageResponse SendMessage(SendMessageRequest request)
        {
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessage(request);
        }
#endif

        public Task<SendMessageResponse> SendMessageAsync(string queueUrl, string messageBody, CancellationToken cancellationToken = new CancellationToken())
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessageAsync(request, cancellationToken);
        }

        public Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessageAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SendMessageBatchResponse SendMessageBatch(string queueUrl, List<SendMessageBatchRequestEntry> entries)
        {
            AddCorrelationAttributeIfAbsent(entries);
            return _client.SendMessageBatch(queueUrl, entries);
        }

        public SendMessageBatchResponse SendMessageBatch(SendMessageBatchRequest request)
        {
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessageBatch(request);
        }
#endif

        public Task<SendMessageBatchResponse> SendMessageBatchAsync(string queueUrl, List<SendMessageBatchRequestEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            AddCorrelationAttributeIfAbsent(entries);
            return _client.SendMessageBatchAsync(queueUrl, entries, cancellationToken);
        }

        public Task<SendMessageBatchResponse> SendMessageBatchAsync(SendMessageBatchRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessageBatchAsync(request, cancellationToken);
        }

        private static void AddCorrelationAttributeIfAbsent(SendMessageRequest request)
        {
            MessageAttributeValue value;
            if (request.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out value) == false)
            {
                request.MessageAttributes.Add(CorrelationKeys.CorrelationId,
                    new MessageAttributeValue { DataType = "String", StringValue = CorrelationScope.Current.CorrelationId.ToString() });
            }
        }

        private static void AddCorrelationAttributeIfAbsent(SendMessageBatchRequestEntry request)
        {
            MessageAttributeValue value;
            if (request.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out value) == false)
            {
                request.MessageAttributes.Add(CorrelationKeys.CorrelationId,
                    new MessageAttributeValue { DataType = "String", StringValue = CorrelationScope.Current.CorrelationId.ToString() });
            }
        }

        private static void AddCorrelationAttributeIfAbsent(SendMessageBatchRequest request)
        {
            AddCorrelationAttributeIfAbsent(request.Entries);
        }

        private static void AddCorrelationAttributeIfAbsent(IEnumerable<SendMessageBatchRequestEntry> entries)
        {
            foreach (var entry in entries)
            {
                AddCorrelationAttributeIfAbsent(entry);
            }
        }

        #region Delegated members

#if !IS_MIN_NETSTANDARD1_3
        public Dictionary<string, string> GetAttributes(string queueUrl)
        {
            return _client.GetAttributes(queueUrl);
        }
#endif

        public Task<Dictionary<string, string>> GetAttributesAsync(string queueUrl)
        {
            return _client.GetAttributesAsync(queueUrl);
        }

#if !IS_MIN_NETSTANDARD1_3
        public void SetAttributes(string queueUrl, Dictionary<string, string> attributes)
        {
            _client.SetAttributes(queueUrl, attributes);
        }
#endif

        public Task SetAttributesAsync(string queueUrl, Dictionary<string, string> attributes)
        {
            return _client.SetAttributesAsync(queueUrl, attributes);
        }

#if !IS_MIN_NETSTANDARD1_3
        public string AuthorizeS3ToSendMessage(string queueUrl, string bucket)
        {
            return _client.AuthorizeS3ToSendMessage(queueUrl, bucket);
        }
#endif

        public Task<string> AuthorizeS3ToSendMessageAsync(string queueUrl, string bucket)
        {
            return _client.AuthorizeS3ToSendMessageAsync(queueUrl, bucket);
        }

#if !IS_MIN_NETSTANDARD1_3
        public AddPermissionResponse AddPermission(string queueUrl, string label, List<string> awsAccountIds, List<string> actions)
        {
            return _client.AddPermission(queueUrl, label, awsAccountIds, actions);
        }

        public AddPermissionResponse AddPermission(AddPermissionRequest request)
        {
            return _client.AddPermission(request);
        }
#endif

        public Task<AddPermissionResponse> AddPermissionAsync(string queueUrl, string label, List<string> awsAccountIds, List<string> actions,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(queueUrl, label, awsAccountIds, actions, cancellationToken);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ChangeMessageVisibilityResponse ChangeMessageVisibility(string queueUrl, string receiptHandle, int visibilityTimeout)
        {
            return _client.ChangeMessageVisibility(queueUrl, receiptHandle, visibilityTimeout);
        }

        public ChangeMessageVisibilityResponse ChangeMessageVisibility(ChangeMessageVisibilityRequest request)
        {
            return _client.ChangeMessageVisibility(request);
        }
#endif

        public Task<ChangeMessageVisibilityResponse> ChangeMessageVisibilityAsync(string queueUrl, string receiptHandle, int visibilityTimeout,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityAsync(queueUrl, receiptHandle, visibilityTimeout, cancellationToken);
        }

        public Task<ChangeMessageVisibilityResponse> ChangeMessageVisibilityAsync(ChangeMessageVisibilityRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ChangeMessageVisibilityBatchResponse ChangeMessageVisibilityBatch(string queueUrl, List<ChangeMessageVisibilityBatchRequestEntry> entries)
        {
            return _client.ChangeMessageVisibilityBatch(queueUrl, entries);
        }

        public ChangeMessageVisibilityBatchResponse ChangeMessageVisibilityBatch(ChangeMessageVisibilityBatchRequest request)
        {
            return _client.ChangeMessageVisibilityBatch(request);
        }
#endif

        public Task<ChangeMessageVisibilityBatchResponse> ChangeMessageVisibilityBatchAsync(string queueUrl,
            List<ChangeMessageVisibilityBatchRequestEntry> entries, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityBatchAsync(queueUrl, entries, cancellationToken);
        }

        public Task<ChangeMessageVisibilityBatchResponse> ChangeMessageVisibilityBatchAsync(ChangeMessageVisibilityBatchRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityBatchAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public CreateQueueResponse CreateQueue(string queueName)
        {
            return _client.CreateQueue(queueName);
        }

        public CreateQueueResponse CreateQueue(CreateQueueRequest request)
        {
            return _client.CreateQueue(request);
        }
#endif

        public Task<CreateQueueResponse> CreateQueueAsync(string queueName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateQueueAsync(queueName, cancellationToken);
        }

        public Task<CreateQueueResponse> CreateQueueAsync(CreateQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateQueueAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public DeleteMessageResponse DeleteMessage(string queueUrl, string receiptHandle)
        {
            return _client.DeleteMessage(queueUrl, receiptHandle);
        }

        public DeleteMessageResponse DeleteMessage(DeleteMessageRequest request)
        {
            return _client.DeleteMessage(request);
        }
#endif

        public Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken);
        }

        public Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public DeleteMessageBatchResponse DeleteMessageBatch(string queueUrl, List<DeleteMessageBatchRequestEntry> entries)
        {
            return _client.DeleteMessageBatch(queueUrl, entries);
        }

        public DeleteMessageBatchResponse DeleteMessageBatch(DeleteMessageBatchRequest request)
        {
            return _client.DeleteMessageBatch(request);
        }
#endif

        public Task<DeleteMessageBatchResponse> DeleteMessageBatchAsync(string queueUrl, List<DeleteMessageBatchRequestEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageBatchAsync(queueUrl, entries, cancellationToken);
        }

        public Task<DeleteMessageBatchResponse> DeleteMessageBatchAsync(DeleteMessageBatchRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageBatchAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public DeleteQueueResponse DeleteQueue(string queueUrl)
        {
            return _client.DeleteQueue(queueUrl);
        }

        public DeleteQueueResponse DeleteQueue(DeleteQueueRequest request)
        {
            return _client.DeleteQueue(request);
        }
#endif

        public Task<DeleteQueueResponse> DeleteQueueAsync(string queueUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteQueueAsync(queueUrl, cancellationToken);
        }

        public Task<DeleteQueueResponse> DeleteQueueAsync(DeleteQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteQueueAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetQueueAttributesResponse GetQueueAttributes(string queueUrl, List<string> attributeNames)
        {
            return _client.GetQueueAttributes(queueUrl, attributeNames);
        }

        public GetQueueAttributesResponse GetQueueAttributes(GetQueueAttributesRequest request)
        {
            return _client.GetQueueAttributes(request);
        }
#endif

        public Task<GetQueueAttributesResponse> GetQueueAttributesAsync(string queueUrl, List<string> attributeNames,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueAttributesAsync(queueUrl, attributeNames, cancellationToken);
        }

        public Task<GetQueueAttributesResponse> GetQueueAttributesAsync(GetQueueAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public GetQueueUrlResponse GetQueueUrl(string queueName)
        {
            return _client.GetQueueUrl(queueName);
        }

        public GetQueueUrlResponse GetQueueUrl(GetQueueUrlRequest request)
        {
            return _client.GetQueueUrl(request);
        }
#endif

        public Task<GetQueueUrlResponse> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueUrlAsync(queueName, cancellationToken);
        }

        public Task<GetQueueUrlResponse> GetQueueUrlAsync(GetQueueUrlRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueUrlAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListDeadLetterSourceQueuesResponse ListDeadLetterSourceQueues(ListDeadLetterSourceQueuesRequest request)
        {
            return _client.ListDeadLetterSourceQueues(request);
        }
#endif

        public Task<ListDeadLetterSourceQueuesResponse> ListDeadLetterSourceQueuesAsync(ListDeadLetterSourceQueuesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListDeadLetterSourceQueuesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListQueuesResponse ListQueues(string queueNamePrefix)
        {
            return _client.ListQueues(queueNamePrefix);
        }

        public ListQueuesResponse ListQueues(ListQueuesRequest request)
        {
            return _client.ListQueues(request);
        }
#endif

        public Task<ListQueuesResponse> ListQueuesAsync(string queueNamePrefix, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueuesAsync(queueNamePrefix, cancellationToken);
        }

        public Task<ListQueuesResponse> ListQueuesAsync(ListQueuesRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueuesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public ListQueueTagsResponse ListQueueTags(ListQueueTagsRequest request)
        {
            return _client.ListQueueTags(request);
        }
#endif

        public Task<ListQueueTagsResponse> ListQueueTagsAsync(ListQueueTagsRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueueTagsAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public PurgeQueueResponse PurgeQueue(string queueUrl)
        {
            return _client.PurgeQueue(queueUrl);
        }

        public PurgeQueueResponse PurgeQueue(PurgeQueueRequest request)
        {
            return _client.PurgeQueue(request);
        }
#endif

        public Task<PurgeQueueResponse> PurgeQueueAsync(string queueUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PurgeQueueAsync(queueUrl, cancellationToken);
        }

        public Task<PurgeQueueResponse> PurgeQueueAsync(PurgeQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PurgeQueueAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public RemovePermissionResponse RemovePermission(string queueUrl, string label)
        {
            return _client.RemovePermission(queueUrl, label);
        }

        public RemovePermissionResponse RemovePermission(RemovePermissionRequest request)
        {
            return _client.RemovePermission(request);
        }
#endif

        public Task<RemovePermissionResponse> RemovePermissionAsync(string queueUrl, string label, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(queueUrl, label, cancellationToken);
        }

        public Task<RemovePermissionResponse> RemovePermissionAsync(RemovePermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public SetQueueAttributesResponse SetQueueAttributes(string queueUrl, Dictionary<string, string> attributes)
        {
            return _client.SetQueueAttributes(queueUrl, attributes);
        }

        public SetQueueAttributesResponse SetQueueAttributes(SetQueueAttributesRequest request)
        {
            return _client.SetQueueAttributes(request);
        }
#endif

        public Task<SetQueueAttributesResponse> SetQueueAttributesAsync(string queueUrl, Dictionary<string, string> attributes,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetQueueAttributesAsync(queueUrl, attributes, cancellationToken);
        }

        public Task<SetQueueAttributesResponse> SetQueueAttributesAsync(SetQueueAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetQueueAttributesAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public TagQueueResponse TagQueue(TagQueueRequest request)
        {
            return _client.TagQueue(request);
        }
#endif

        public Task<TagQueueResponse> TagQueueAsync(TagQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.TagQueueAsync(request, cancellationToken);
        }

#if !IS_MIN_NETSTANDARD1_3
        public UntagQueueResponse UntagQueue(UntagQueueRequest request)
        {
            return _client.UntagQueue(request);
        }
#endif

        public Task<UntagQueueResponse> UntagQueueAsync(UntagQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UntagQueueAsync(request, cancellationToken);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        #endregion

        public IClientConfig Config => _client.Config;
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Albumprinter.CorrelationTracking.Tracing.AmazonSqs
{
    public class Log4NetDecorator : IAmazonSQS
    {
        private readonly IAmazonSQS _client;

        public bool LogReceiveMessageResponse { get; }

        public Log4NetDecorator(IAmazonSQS client, bool logReceiveMessageResponse)
        {
            _client = client;
            LogReceiveMessageResponse = logReceiveMessageResponse;
        }

        public SendMessageResponse SendMessage(string queueUrl, string messageBody)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };
            request.Log();
            return _client.SendMessage(request);
        }

        public SendMessageResponse SendMessage(SendMessageRequest request)
        {
            request.Log();
            return _client.SendMessage(request);
        }

        public Task<SendMessageResponse> SendMessageAsync(string queueUrl, string messageBody, CancellationToken cancellationToken = new CancellationToken())
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };
            request.Log();
            return _client.SendMessageAsync(request, cancellationToken);
        }

        public Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            request.Log();
            return _client.SendMessageAsync(request, cancellationToken);
        }

        public SendMessageBatchResponse SendMessageBatch(string queueUrl, List<SendMessageBatchRequestEntry> entries)
        {
            entries.Log();
            return _client.SendMessageBatch(queueUrl, entries);
        }

        public SendMessageBatchResponse SendMessageBatch(SendMessageBatchRequest request)
        {
            request.Log();
            return _client.SendMessageBatch(request);
        }

        public Task<SendMessageBatchResponse> SendMessageBatchAsync(string queueUrl, List<SendMessageBatchRequestEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            entries.Log();
            return _client.SendMessageBatchAsync(queueUrl, entries, cancellationToken);
        }

        public Task<SendMessageBatchResponse> SendMessageBatchAsync(SendMessageBatchRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            request.Log();
            return _client.SendMessageBatchAsync(request, cancellationToken);
        }

        public ReceiveMessageResponse ReceiveMessage(string queueUrl)
        {
            var receiveMessageResponse = _client.ReceiveMessage(queueUrl);
            DoLogReceiveMessageResponse(receiveMessageResponse);
            return receiveMessageResponse;
        }

        public ReceiveMessageResponse ReceiveMessage(ReceiveMessageRequest request)
        {
            var receiveMessageResponse = _client.ReceiveMessage(request);
            DoLogReceiveMessageResponse(receiveMessageResponse);
            return receiveMessageResponse;
        }

        public async Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            var receiveMessageResponse = await _client.ReceiveMessageAsync(queueUrl, cancellationToken);
            DoLogReceiveMessageResponse(receiveMessageResponse);
            return receiveMessageResponse;
        }

        public async Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var receiveMessageResponse = await _client.ReceiveMessageAsync(request, cancellationToken);
            DoLogReceiveMessageResponse(receiveMessageResponse);
            return receiveMessageResponse;
        }

        private void DoLogReceiveMessageResponse(ReceiveMessageResponse receiveMessageResponse)
        {
            if (LogReceiveMessageResponse)
            {
                receiveMessageResponse.Log();
            }
        }

        #region Delegated members

        public Dictionary<string, string> GetAttributes(string queueUrl)
        {
            return _client.GetAttributes(queueUrl);
        }

        public Task<Dictionary<string, string>> GetAttributesAsync(string queueUrl)
        {
            return _client.GetAttributesAsync(queueUrl);
        }

        public void SetAttributes(string queueUrl, Dictionary<string, string> attributes)
        {
            _client.SetAttributes(queueUrl, attributes);
        }

        public Task SetAttributesAsync(string queueUrl, Dictionary<string, string> attributes)
        {
            return _client.SetAttributesAsync(queueUrl, attributes);
        }

        public string AuthorizeS3ToSendMessage(string queueUrl, string bucket)
        {
            return _client.AuthorizeS3ToSendMessage(queueUrl, bucket);
        }

        public Task<string> AuthorizeS3ToSendMessageAsync(string queueUrl, string bucket)
        {
            return _client.AuthorizeS3ToSendMessageAsync(queueUrl, bucket);
        }

        public AddPermissionResponse AddPermission(string queueUrl, string label, List<string> awsAccountIds, List<string> actions)
        {
            return _client.AddPermission(queueUrl, label, awsAccountIds, actions);
        }

        public AddPermissionResponse AddPermission(AddPermissionRequest request)
        {
            return _client.AddPermission(request);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(string queueUrl, string label, List<string> awsAccountIds, List<string> actions,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(queueUrl, label, awsAccountIds, actions, cancellationToken);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(request, cancellationToken);
        }

        public ChangeMessageVisibilityResponse ChangeMessageVisibility(string queueUrl, string receiptHandle, int visibilityTimeout)
        {
            return _client.ChangeMessageVisibility(queueUrl, receiptHandle, visibilityTimeout);
        }

        public ChangeMessageVisibilityResponse ChangeMessageVisibility(ChangeMessageVisibilityRequest request)
        {
            return _client.ChangeMessageVisibility(request);
        }

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

        public ChangeMessageVisibilityBatchResponse ChangeMessageVisibilityBatch(string queueUrl, List<ChangeMessageVisibilityBatchRequestEntry> entries)
        {
            return _client.ChangeMessageVisibilityBatch(queueUrl, entries);
        }

        public ChangeMessageVisibilityBatchResponse ChangeMessageVisibilityBatch(ChangeMessageVisibilityBatchRequest request)
        {
            return _client.ChangeMessageVisibilityBatch(request);
        }

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

        public CreateQueueResponse CreateQueue(string queueName)
        {
            return _client.CreateQueue(queueName);
        }

        public CreateQueueResponse CreateQueue(CreateQueueRequest request)
        {
            return _client.CreateQueue(request);
        }

        public Task<CreateQueueResponse> CreateQueueAsync(string queueName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateQueueAsync(queueName, cancellationToken);
        }

        public Task<CreateQueueResponse> CreateQueueAsync(CreateQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateQueueAsync(request, cancellationToken);
        }

        public DeleteMessageResponse DeleteMessage(string queueUrl, string receiptHandle)
        {
            return _client.DeleteMessage(queueUrl, receiptHandle);
        }

        public DeleteMessageResponse DeleteMessage(DeleteMessageRequest request)
        {
            return _client.DeleteMessage(request);
        }

        public Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken);
        }

        public Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageAsync(request, cancellationToken);
        }

        public DeleteMessageBatchResponse DeleteMessageBatch(string queueUrl, List<DeleteMessageBatchRequestEntry> entries)
        {
            return _client.DeleteMessageBatch(queueUrl, entries);
        }

        public DeleteMessageBatchResponse DeleteMessageBatch(DeleteMessageBatchRequest request)
        {
            return _client.DeleteMessageBatch(request);
        }

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

        public DeleteQueueResponse DeleteQueue(string queueUrl)
        {
            return _client.DeleteQueue(queueUrl);
        }

        public DeleteQueueResponse DeleteQueue(DeleteQueueRequest request)
        {
            return _client.DeleteQueue(request);
        }

        public Task<DeleteQueueResponse> DeleteQueueAsync(string queueUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteQueueAsync(queueUrl, cancellationToken);
        }

        public Task<DeleteQueueResponse> DeleteQueueAsync(DeleteQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteQueueAsync(request, cancellationToken);
        }

        public GetQueueAttributesResponse GetQueueAttributes(string queueUrl, List<string> attributeNames)
        {
            return _client.GetQueueAttributes(queueUrl, attributeNames);
        }

        public GetQueueAttributesResponse GetQueueAttributes(GetQueueAttributesRequest request)
        {
            return _client.GetQueueAttributes(request);
        }

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

        public GetQueueUrlResponse GetQueueUrl(string queueName)
        {
            return _client.GetQueueUrl(queueName);
        }

        public GetQueueUrlResponse GetQueueUrl(GetQueueUrlRequest request)
        {
            return _client.GetQueueUrl(request);
        }

        public Task<GetQueueUrlResponse> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueUrlAsync(queueName, cancellationToken);
        }

        public Task<GetQueueUrlResponse> GetQueueUrlAsync(GetQueueUrlRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueUrlAsync(request, cancellationToken);
        }

        public ListDeadLetterSourceQueuesResponse ListDeadLetterSourceQueues(ListDeadLetterSourceQueuesRequest request)
        {
            return _client.ListDeadLetterSourceQueues(request);
        }

        public Task<ListDeadLetterSourceQueuesResponse> ListDeadLetterSourceQueuesAsync(ListDeadLetterSourceQueuesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListDeadLetterSourceQueuesAsync(request, cancellationToken);
        }

        public ListQueuesResponse ListQueues(string queueNamePrefix)
        {
            return _client.ListQueues(queueNamePrefix);
        }

        public ListQueuesResponse ListQueues(ListQueuesRequest request)
        {
            return _client.ListQueues(request);
        }

        public Task<ListQueuesResponse> ListQueuesAsync(string queueNamePrefix, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueuesAsync(queueNamePrefix, cancellationToken);
        }

        public Task<ListQueuesResponse> ListQueuesAsync(ListQueuesRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueuesAsync(request, cancellationToken);
        }

        public ListQueueTagsResponse ListQueueTags(ListQueueTagsRequest request)
        {
            return _client.ListQueueTags(request);
        }

        public Task<ListQueueTagsResponse> ListQueueTagsAsync(ListQueueTagsRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueueTagsAsync(request, cancellationToken);
        }

        public PurgeQueueResponse PurgeQueue(string queueUrl)
        {
            return _client.PurgeQueue(queueUrl);
        }

        public PurgeQueueResponse PurgeQueue(PurgeQueueRequest request)
        {
            return _client.PurgeQueue(request);
        }

        public Task<PurgeQueueResponse> PurgeQueueAsync(string queueUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PurgeQueueAsync(queueUrl, cancellationToken);
        }

        public Task<PurgeQueueResponse> PurgeQueueAsync(PurgeQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PurgeQueueAsync(request, cancellationToken);
        }

        public RemovePermissionResponse RemovePermission(string queueUrl, string label)
        {
            return _client.RemovePermission(queueUrl, label);
        }

        public RemovePermissionResponse RemovePermission(RemovePermissionRequest request)
        {
            return _client.RemovePermission(request);
        }

        public Task<RemovePermissionResponse> RemovePermissionAsync(string queueUrl, string label, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(queueUrl, label, cancellationToken);
        }

        public Task<RemovePermissionResponse> RemovePermissionAsync(RemovePermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(request, cancellationToken);
        }

        public SetQueueAttributesResponse SetQueueAttributes(string queueUrl, Dictionary<string, string> attributes)
        {
            return _client.SetQueueAttributes(queueUrl, attributes);
        }

        public SetQueueAttributesResponse SetQueueAttributes(SetQueueAttributesRequest request)
        {
            return _client.SetQueueAttributes(request);
        }

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

        public TagQueueResponse TagQueue(TagQueueRequest request)
        {
            return _client.TagQueue(request);
        }

        public Task<TagQueueResponse> TagQueueAsync(TagQueueRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.TagQueueAsync(request, cancellationToken);
        }

        public UntagQueueResponse UntagQueue(UntagQueueRequest request)
        {
            return _client.UntagQueue(request);
        }

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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs
{
    public sealed class CorrelationDecorator : IAmazonSQS
    {
        private readonly IAmazonSQS _client;

        public CorrelationDecorator(IAmazonSQS client)
        {
            _client = client;
        }

        public Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var request = new ReceiveMessageRequest
                {QueueUrl = queueUrl, MessageAttributeNames = new List<string> {CorrelationKeys.CorrelationId}};
            return _client.ReceiveMessageAsync(request, cancellationToken);
        }

        public Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            request.MessageAttributeNames.Add(CorrelationKeys.CorrelationId);
            return _client.ReceiveMessageAsync(request, cancellationToken);
        }

        public Task<SendMessageResponse> SendMessageAsync(string queueUrl, string messageBody,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessageAsync(request, cancellationToken);
        }

        public Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            AddCorrelationAttributeIfAbsent(request);
            return _client.SendMessageAsync(request, cancellationToken);
        }


        public Task<SendMessageBatchResponse> SendMessageBatchAsync(string queueUrl,
            List<SendMessageBatchRequestEntry> entries,
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
            var currentScope = CorrelationScope.Current;
            if (currentScope != null && request.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out value) == false)
            {
                request.MessageAttributes.Add(CorrelationKeys.CorrelationId,
                    new MessageAttributeValue
                        {DataType = "String", StringValue = currentScope.CorrelationId.ToString()});
            }
        }

        private static void AddCorrelationAttributeIfAbsent(SendMessageBatchRequestEntry request)
        {
            MessageAttributeValue value;
            var currentScope = CorrelationScope.Current;
            if (currentScope != null && request.MessageAttributes.TryGetValue(CorrelationKeys.CorrelationId, out value) == false)
            {
                request.MessageAttributes.Add(CorrelationKeys.CorrelationId,
                    new MessageAttributeValue
                        {DataType = "String", StringValue = currentScope.CorrelationId.ToString()});
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

        public Task<Dictionary<string, string>> GetAttributesAsync(string queueUrl)
        {
            return _client.GetAttributesAsync(queueUrl);
        }

        public Task SetAttributesAsync(string queueUrl, Dictionary<string, string> attributes)
        {
            return _client.SetAttributesAsync(queueUrl, attributes);
        }

        public Task<string> AuthorizeS3ToSendMessageAsync(string queueUrl, string bucket)
        {
            return _client.AuthorizeS3ToSendMessageAsync(queueUrl, bucket);
        }


        public Task<AddPermissionResponse> AddPermissionAsync(string queueUrl, string label, List<string> awsAccountIds,
            List<string> actions,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(queueUrl, label, awsAccountIds, actions, cancellationToken);
        }

        public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.AddPermissionAsync(request, cancellationToken);
        }


        public Task<ChangeMessageVisibilityResponse> ChangeMessageVisibilityAsync(string queueUrl, string receiptHandle,
            int visibilityTimeout,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityAsync(queueUrl, receiptHandle, visibilityTimeout, cancellationToken);
        }

        public Task<ChangeMessageVisibilityResponse> ChangeMessageVisibilityAsync(
            ChangeMessageVisibilityRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityAsync(request, cancellationToken);
        }


        public Task<ChangeMessageVisibilityBatchResponse> ChangeMessageVisibilityBatchAsync(string queueUrl,
            List<ChangeMessageVisibilityBatchRequestEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityBatchAsync(queueUrl, entries, cancellationToken);
        }

        public Task<ChangeMessageVisibilityBatchResponse> ChangeMessageVisibilityBatchAsync(
            ChangeMessageVisibilityBatchRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ChangeMessageVisibilityBatchAsync(request, cancellationToken);
        }


        public Task<CreateQueueResponse> CreateQueueAsync(string queueName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateQueueAsync(queueName, cancellationToken);
        }

        public Task<CreateQueueResponse> CreateQueueAsync(CreateQueueRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.CreateQueueAsync(request, cancellationToken);
        }

        public Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken);
        }

        public Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageAsync(request, cancellationToken);
        }


        public Task<DeleteMessageBatchResponse> DeleteMessageBatchAsync(string queueUrl,
            List<DeleteMessageBatchRequestEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageBatchAsync(queueUrl, entries, cancellationToken);
        }

        public Task<DeleteMessageBatchResponse> DeleteMessageBatchAsync(DeleteMessageBatchRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteMessageBatchAsync(request, cancellationToken);
        }


        public Task<DeleteQueueResponse> DeleteQueueAsync(string queueUrl,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteQueueAsync(queueUrl, cancellationToken);
        }

        public Task<DeleteQueueResponse> DeleteQueueAsync(DeleteQueueRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.DeleteQueueAsync(request, cancellationToken);
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


        public Task<GetQueueUrlResponse> GetQueueUrlAsync(string queueName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueUrlAsync(queueName, cancellationToken);
        }

        public Task<GetQueueUrlResponse> GetQueueUrlAsync(GetQueueUrlRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.GetQueueUrlAsync(request, cancellationToken);
        }

        public Task<ListDeadLetterSourceQueuesResponse> ListDeadLetterSourceQueuesAsync(
            ListDeadLetterSourceQueuesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListDeadLetterSourceQueuesAsync(request, cancellationToken);
        }

        public Task<ListQueuesResponse> ListQueuesAsync(string queueNamePrefix,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueuesAsync(queueNamePrefix, cancellationToken);
        }

        public Task<ListQueuesResponse> ListQueuesAsync(ListQueuesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueuesAsync(request, cancellationToken);
        }


        public Task<ListQueueTagsResponse> ListQueueTagsAsync(ListQueueTagsRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.ListQueueTagsAsync(request, cancellationToken);
        }


        public Task<PurgeQueueResponse> PurgeQueueAsync(string queueUrl,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PurgeQueueAsync(queueUrl, cancellationToken);
        }

        public Task<PurgeQueueResponse> PurgeQueueAsync(PurgeQueueRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.PurgeQueueAsync(request, cancellationToken);
        }


        public Task<RemovePermissionResponse> RemovePermissionAsync(string queueUrl, string label,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(queueUrl, label, cancellationToken);
        }

        public Task<RemovePermissionResponse> RemovePermissionAsync(RemovePermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.RemovePermissionAsync(request, cancellationToken);
        }


        public Task<SetQueueAttributesResponse> SetQueueAttributesAsync(string queueUrl,
            Dictionary<string, string> attributes,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetQueueAttributesAsync(queueUrl, attributes, cancellationToken);
        }

        public Task<SetQueueAttributesResponse> SetQueueAttributesAsync(SetQueueAttributesRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.SetQueueAttributesAsync(request, cancellationToken);
        }

        public Task<TagQueueResponse> TagQueueAsync(TagQueueRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.TagQueueAsync(request, cancellationToken);
        }

        public Task<UntagQueueResponse> UntagQueueAsync(UntagQueueRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _client.UntagQueueAsync(request, cancellationToken);
        }

        public ISQSPaginatorFactory Paginators => _client.Paginators;

        public void Dispose()
        {
            _client.Dispose();
        }

        #endregion

        public IClientConfig Config => _client.Config;
    }
}
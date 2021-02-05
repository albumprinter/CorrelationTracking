using System.Collections.Generic;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.SQS.Model;

namespace Albelli.Correlation.AmazonSqs
{
    public class SqsCorrelationPipelineHandler : PipelineHandler
    {
        public override void InvokeSync(IExecutionContext executionContext)
        {
            AddCorrelationAttributeIfAbsent(executionContext.RequestContext);
            base.InvokeSync(executionContext);
        }

        public override async Task<T> InvokeAsync<T>(IExecutionContext executionContext)
        {
            AddCorrelationAttributeIfAbsent(executionContext.RequestContext);
            var result = await base.InvokeAsync<T>(executionContext);
            return result;
        }

        private static void AddCorrelationAttributeIfAbsent(IRequestContext requestContext)
        {
            //that piece of code works only *before* Marshaller
            if (!(requestContext.OriginalRequest is Amazon.SQS.AmazonSQSRequest))
            {
                return;
            }

            var currentScope = CorrelationScope.Current;
            switch (requestContext.OriginalRequest)
            {
                case ReceiveMessageRequest receiveMessageRequest:
                    if (receiveMessageRequest.MessageAttributeNames == null)
                    {
                        receiveMessageRequest.MessageAttributeNames = new List<string>();
                    }

                    if (!receiveMessageRequest.MessageAttributeNames.Contains(CorrelationKeys.CorrelationId) &&
                        !receiveMessageRequest.MessageAttributeNames.Contains("All"))
                    {
                        receiveMessageRequest.MessageAttributeNames.Add(CorrelationKeys.CorrelationId);
                    }

                    break;
                case SendMessageRequest sendMessageRequest:
                    if (sendMessageRequest.MessageAttributes == null)
                    {
                        sendMessageRequest.MessageAttributes = new Dictionary<string, MessageAttributeValue>();
                    }

                    if (currentScope != null && !sendMessageRequest.MessageAttributes.ContainsKey(CorrelationKeys.CorrelationId))
                    {
                        sendMessageRequest.MessageAttributes[CorrelationKeys.CorrelationId] = new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = currentScope.CorrelationId.ToString()
                        };
                    }

                    break;
                case SendMessageBatchRequest sendMessageBatchRequest:
                    foreach (var sendMessageBatchRequestEntry in sendMessageBatchRequest.Entries)
                    {
                        if (sendMessageBatchRequestEntry.MessageAttributes == null)
                        {
                            sendMessageBatchRequestEntry.MessageAttributes = new Dictionary<string, MessageAttributeValue>();
                        }

                        if (currentScope != null && !sendMessageBatchRequestEntry.MessageAttributes.ContainsKey(CorrelationKeys.CorrelationId))
                        {
                            sendMessageBatchRequestEntry.MessageAttributes[CorrelationKeys.CorrelationId] = new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = currentScope.CorrelationId.ToString()
                            };
                        }
                    }

                    break;
            }

        }
    }
}